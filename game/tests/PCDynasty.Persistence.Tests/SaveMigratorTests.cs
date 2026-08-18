using System.Collections.Immutable;
using PCDynasty.Persistence.Abstractions;
using PCDynasty.Persistence.Migrations;
using Xunit;

namespace PCDynasty.Persistence.Tests;

public sealed class SaveMigratorTests
{
    [Fact]
    public void AppliesEveryExplicitVersionStepInOrder()
    {
        WorldLoadResult source = CreateWorld(formatVersion: 1);
        var applied = new List<string>();
        var migrator = new SaveMigrator(
        [
            new SaveMigrationStep(1, 2, world => Upgrade(world, 2, applied)),
            new SaveMigrationStep(2, 3, world => Upgrade(world, 3, applied)),
        ]);

        SaveMigrationOutcome outcome = migrator.Migrate(source, targetFormatVersion: 3);

        Assert.True(outcome.IsSuccess);
        Assert.Same(source, outcome.RecoverableOriginal);
        Assert.Equal(3, outcome.Migrated!.Manifest.SaveFormatVersion);
        Assert.Equal(["1->2", "2->3"], applied);
        Assert.Equal([7, 8], outcome.Migrated.Snapshot.Payload.AsSpan().ToArray());
    }

    [Fact]
    public void MissingOrFailingStepsKeepTheOriginalRecoverable()
    {
        WorldLoadResult source = CreateWorld(formatVersion: 1);
        var missingStep = new SaveMigrator(
        [
            new SaveMigrationStep(1, 2, world => Upgrade(world, 2, [])),
        ]);
        var failingStep = new SaveMigrator(
        [
            new SaveMigrationStep(1, 2, _ => throw new InvalidDataException("broken input")),
        ]);

        SaveMigrationOutcome missing = missingStep.Migrate(source, targetFormatVersion: 3);
        SaveMigrationOutcome failed = failingStep.Migrate(source, targetFormatVersion: 2);

        Assert.False(missing.IsSuccess);
        Assert.False(failed.IsSuccess);
        Assert.Same(source, missing.RecoverableOriginal);
        Assert.Same(source, failed.RecoverableOriginal);
        Assert.Equal([7, 8], source.Snapshot.Payload.AsSpan().ToArray());
    }

    [Fact]
    public void RejectsAmbiguousOrNonSequentialMigrationDefinitions()
    {
        SaveMigrationStep step = new(1, 2, world => world);

        Assert.Throws<ArgumentException>(() => new SaveMigrator([step, step]));
        Assert.Throws<ArgumentException>(() => new SaveMigrationStep(1, 3, world => world));
    }

    private static WorldLoadResult Upgrade(
        WorldLoadResult source,
        int targetVersion,
        List<string> applied)
    {
        applied.Add($"{source.Manifest.SaveFormatVersion}->{targetVersion}");
        var manifest = new SaveManifest(
            source.Manifest.SaveId,
            targetVersion,
            source.Manifest.WorldSchemaVersion,
            source.Manifest.SnapshotSequence,
            source.Manifest.LastFactSequence,
            source.Manifest.PackHashes);
        return new WorldLoadResult(manifest, source.Snapshot, source.Facts);
    }

    private static WorldLoadResult CreateWorld(int formatVersion)
    {
        var manifest = new SaveManifest(
            "save:career:1",
            formatVersion,
            worldSchemaVersion: 7,
            snapshotSequence: 0,
            lastFactSequence: 0,
            new Dictionary<string, string> { ["pack:football"] = "sha256:abc123" });
        return new WorldLoadResult(
            manifest,
            new WorldSnapshot(0, 7, [7, 8]),
            ImmutableArray<WorldFact>.Empty);
    }
}
