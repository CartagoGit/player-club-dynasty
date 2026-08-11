using System.Diagnostics.CodeAnalysis;
using PCDynasty.Persistence.Abstractions;

namespace PCDynasty.Persistence.Migrations;

public sealed class SaveMigrationStep
{
    private readonly Func<WorldLoadResult, WorldLoadResult> migrate;

    public SaveMigrationStep(
        int sourceFormatVersion,
        int targetFormatVersion,
        Func<WorldLoadResult, WorldLoadResult> migrate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceFormatVersion);
        if (targetFormatVersion != sourceFormatVersion + 1)
        {
            throw new ArgumentException(
                "Save migrations must declare one explicit version boundary at a time.",
                nameof(targetFormatVersion));
        }

        ArgumentNullException.ThrowIfNull(migrate);
        SourceFormatVersion = sourceFormatVersion;
        TargetFormatVersion = targetFormatVersion;
        this.migrate = migrate;
    }

    public int SourceFormatVersion { get; }
    public int TargetFormatVersion { get; }

    internal WorldLoadResult Apply(WorldLoadResult source)
    {
        return migrate(source);
    }
}

public sealed class SaveMigrationOutcome
{
    private SaveMigrationOutcome(
        WorldLoadResult recoverableOriginal,
        WorldLoadResult? migrated,
        string? failureReason)
    {
        RecoverableOriginal = recoverableOriginal;
        Migrated = migrated;
        FailureReason = failureReason;
    }

    public WorldLoadResult RecoverableOriginal { get; }
    public WorldLoadResult? Migrated { get; }
    public string? FailureReason { get; }
    public bool IsSuccess => Migrated is not null;

    internal static SaveMigrationOutcome Success(
        WorldLoadResult original,
        WorldLoadResult migrated)
    {
        return new SaveMigrationOutcome(original, migrated, failureReason: null);
    }

    internal static SaveMigrationOutcome Failure(WorldLoadResult original, string reason)
    {
        return new SaveMigrationOutcome(original, migrated: null, reason);
    }
}

public sealed class SaveMigrator
{
    private readonly Dictionary<int, SaveMigrationStep> stepsBySourceVersion;

    public SaveMigrator(IEnumerable<SaveMigrationStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        var copy = new Dictionary<int, SaveMigrationStep>();
        foreach (SaveMigrationStep step in steps)
        {
            ArgumentNullException.ThrowIfNull(step);
            if (!copy.TryAdd(step.SourceFormatVersion, step))
            {
                throw new ArgumentException(
                    "Only one migration may own a source format version.",
                    nameof(steps));
            }
        }

        stepsBySourceVersion = copy;
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Migration failures are converted to a recovery result that preserves the untouched original save.")]
    public SaveMigrationOutcome Migrate(WorldLoadResult source, int targetFormatVersion)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetFormatVersion);
        if (targetFormatVersion < source.Manifest.SaveFormatVersion)
        {
            return SaveMigrationOutcome.Failure(source, "Save format downgrades are not supported.");
        }

        WorldLoadResult current = source;
        while (current.Manifest.SaveFormatVersion < targetFormatVersion)
        {
            int sourceVersion = current.Manifest.SaveFormatVersion;
            if (!stepsBySourceVersion.TryGetValue(sourceVersion, out SaveMigrationStep? step))
            {
                return SaveMigrationOutcome.Failure(
                    source,
                    $"No migration is registered from save format version {sourceVersion}.");
            }

            try
            {
                WorldLoadResult migrated = step.Apply(current)
                    ?? throw new InvalidDataException("A migration returned no save data.");
                if (migrated.Manifest.SaveFormatVersion != step.TargetFormatVersion ||
                    migrated.Manifest.SaveId != source.Manifest.SaveId)
                {
                    throw new InvalidDataException(
                        "A migration returned an unexpected version or changed the save identity.");
                }

                current = migrated;
            }
            catch (Exception exception) when (
                exception is not OutOfMemoryException and
                not StackOverflowException and
                not OperationCanceledException)
            {
                return SaveMigrationOutcome.Failure(source, exception.Message);
            }
        }

        return SaveMigrationOutcome.Success(source, current);
    }
}
