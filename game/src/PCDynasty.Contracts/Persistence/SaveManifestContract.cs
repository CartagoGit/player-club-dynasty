using System.Collections.Immutable;

namespace PCDynasty.Contracts.Persistence;

public sealed class SaveManifestContract
{
    public SaveManifestContract(
        ContractVersion contractVersion,
        string saveId,
        int saveFormatVersion,
        int worldSchemaVersion,
        long snapshotSequence,
        long lastFactSequence,
        IEnumerable<KeyValuePair<string, string>> packHashes)
    {
        if (contractVersion.Major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(contractVersion));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(saveFormatVersion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldSchemaVersion);
        ArgumentOutOfRangeException.ThrowIfNegative(snapshotSequence);
        ArgumentOutOfRangeException.ThrowIfNegative(lastFactSequence);
        ArgumentNullException.ThrowIfNull(packHashes);
        if (snapshotSequence > lastFactSequence)
        {
            throw new ArgumentException("Snapshot sequence cannot exceed the final fact sequence.");
        }

        var copiedHashes = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string packId, string hash) in packHashes)
        {
            if (!copiedHashes.TryAdd(
                ContractIdentifier.Normalize(packId, nameof(packHashes)),
                ContractIdentifier.Normalize(hash, nameof(packHashes))))
            {
                throw new ArgumentException("Pack identifiers must be unique.", nameof(packHashes));
            }
        }

        if (copiedHashes.Count == 0)
        {
            throw new ArgumentException("A save manifest must identify at least one pack.", nameof(packHashes));
        }

        ContractVersion = contractVersion;
        SaveId = ContractIdentifier.Normalize(saveId, nameof(saveId));
        SaveFormatVersion = saveFormatVersion;
        WorldSchemaVersion = worldSchemaVersion;
        SnapshotSequence = snapshotSequence;
        LastFactSequence = lastFactSequence;
        PackHashes = copiedHashes.ToImmutable();
    }

    public ContractVersion ContractVersion { get; }
    public string SaveId { get; }
    public int SaveFormatVersion { get; }
    public int WorldSchemaVersion { get; }
    public long SnapshotSequence { get; }
    public long LastFactSequence { get; }
    public ImmutableDictionary<string, string> PackHashes { get; }
}
