using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using PCDynasty.Persistence.Abstractions;

namespace PCDynasty.Persistence.Sqlite;

public sealed class SqliteWorldStore : IWorldStore
{
    private readonly SqliteWorldConnectionFactory _connections;

    public SqliteWorldStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _connections = new SqliteWorldConnectionFactory(databasePath);
    }

    public async ValueTask<WorldLoadResult?> LoadAsync(
        string saveId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveId);
        await using SqliteConnection connection = await _connections.OpenAsync(cancellationToken);
        return await SqliteWorldReader.LoadAsync(connection, saveId, cancellationToken);
    }

    public async ValueTask AppendAsync(
        string saveId,
        long expectedLastFactSequence,
        ImmutableArray<WorldFact> facts,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveId);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedLastFactSequence);
        ValidateFacts(facts, expectedLastFactSequence);
        if (facts.IsEmpty)
        {
            return;
        }

        await using SqliteConnection connection = await _connections.OpenAsync(cancellationToken);
        await SqliteWorldWriter.AppendAsync(
            connection,
            saveId,
            expectedLastFactSequence,
            facts,
            cancellationToken);
    }

    public async ValueTask CheckpointAsync(
        SaveManifest manifest,
        WorldSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (manifest.SnapshotSequence != manifest.LastFactSequence ||
            snapshot.Sequence != manifest.SnapshotSequence ||
            snapshot.SchemaVersion != manifest.WorldSchemaVersion)
        {
            throw new ArgumentException("A checkpoint must compact through the manifest's last fact.");
        }

        await using SqliteConnection connection = await _connections.OpenAsync(cancellationToken);
        await SqliteWorldWriter.CheckpointAsync(connection, manifest, snapshot, cancellationToken);
    }

    private static void ValidateFacts(ImmutableArray<WorldFact> facts, long lastSequence)
    {
        long expectedSequence = lastSequence + 1;
        foreach (WorldFact fact in facts)
        {
            ArgumentNullException.ThrowIfNull(fact);
            if (fact.Sequence != expectedSequence++)
            {
                throw new ArgumentException("Appended facts must be contiguous.", nameof(facts));
            }
        }
    }
}
