using System.Collections.Immutable;

namespace PCDynasty.Persistence.Abstractions;

public sealed class SaveManifest
{
    public SaveManifest(
        string saveId,
        int saveFormatVersion,
        int worldSchemaVersion,
        long snapshotSequence,
        long lastFactSequence,
        IEnumerable<KeyValuePair<string, string>> packHashes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(saveFormatVersion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldSchemaVersion);
        ArgumentOutOfRangeException.ThrowIfNegative(snapshotSequence);
        ArgumentOutOfRangeException.ThrowIfNegative(lastFactSequence);
        ArgumentNullException.ThrowIfNull(packHashes);
        if (snapshotSequence > lastFactSequence)
        {
            throw new ArgumentException("Snapshot sequence cannot exceed the last fact sequence.");
        }

        var hashes = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string packId, string hash) in packHashes)
        {
            if (!hashes.TryAdd(
                PersistenceValue.NormalizeIdentifier(packId, nameof(packHashes)),
                PersistenceValue.NormalizeIdentifier(hash, nameof(packHashes))))
            {
                throw new ArgumentException("Pack identifiers must be unique.", nameof(packHashes));
            }
        }

        if (hashes.Count == 0)
        {
            throw new ArgumentException("A save must identify at least one source pack.", nameof(packHashes));
        }

        SaveId = PersistenceValue.NormalizeIdentifier(saveId, nameof(saveId));
        SaveFormatVersion = saveFormatVersion;
        WorldSchemaVersion = worldSchemaVersion;
        SnapshotSequence = snapshotSequence;
        LastFactSequence = lastFactSequence;
        PackHashes = hashes.ToImmutable();
    }

    public string SaveId { get; }
    public int SaveFormatVersion { get; }
    public int WorldSchemaVersion { get; }
    public long SnapshotSequence { get; }
    public long LastFactSequence { get; }
    public ImmutableDictionary<string, string> PackHashes { get; }
}

internal static class PersistenceValue
{
    public static string NormalizeIdentifier(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
