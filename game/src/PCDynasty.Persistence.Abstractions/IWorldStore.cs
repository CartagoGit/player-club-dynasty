using System.Collections.Immutable;

namespace PCDynasty.Persistence.Abstractions;

public interface IWorldStore
{
    ValueTask<WorldLoadResult?> LoadAsync(string saveId, CancellationToken cancellationToken = default);

    ValueTask AppendAsync(
        string saveId,
        long expectedLastFactSequence,
        ImmutableArray<WorldFact> facts,
        CancellationToken cancellationToken = default);

    ValueTask CheckpointAsync(
        SaveManifest manifest,
        WorldSnapshot snapshot,
        CancellationToken cancellationToken = default);
}

public sealed class WorldSnapshot
{
    public WorldSnapshot(long sequence, int schemaVersion, IEnumerable<byte> payload)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentNullException.ThrowIfNull(payload);

        Sequence = sequence;
        SchemaVersion = schemaVersion;
        Payload = payload.ToImmutableArray();
    }

    public long Sequence { get; }
    public int SchemaVersion { get; }
    public ImmutableArray<byte> Payload { get; }
}

public sealed class WorldFact
{
    public WorldFact(long sequence, string typeId, int schemaVersion, IEnumerable<byte> payload)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentNullException.ThrowIfNull(payload);

        Sequence = sequence;
        TypeId = PersistenceValue.NormalizeIdentifier(typeId, nameof(typeId));
        SchemaVersion = schemaVersion;
        Payload = payload.ToImmutableArray();
    }

    public long Sequence { get; }
    public string TypeId { get; }
    public int SchemaVersion { get; }
    public ImmutableArray<byte> Payload { get; }
}

public sealed class WorldLoadResult
{
    public WorldLoadResult(
        SaveManifest manifest,
        WorldSnapshot snapshot,
        ImmutableArray<WorldFact> facts)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Sequence != manifest.SnapshotSequence ||
            snapshot.SchemaVersion != manifest.WorldSchemaVersion)
        {
            throw new ArgumentException("Snapshot metadata must match the save manifest.", nameof(snapshot));
        }

        long expectedSequence = snapshot.Sequence + 1;
        foreach (WorldFact fact in facts)
        {
            ArgumentNullException.ThrowIfNull(fact);
            if (fact.Sequence != expectedSequence)
            {
                throw new ArgumentException("Facts after a snapshot must be contiguous.", nameof(facts));
            }

            expectedSequence++;
        }

        if (expectedSequence - 1 != manifest.LastFactSequence)
        {
            throw new ArgumentException("Facts must end at the manifest sequence.", nameof(facts));
        }

        Manifest = manifest;
        Snapshot = snapshot;
        Facts = facts;
    }

    public SaveManifest Manifest { get; }
    public WorldSnapshot Snapshot { get; }
    public ImmutableArray<WorldFact> Facts { get; }
}
