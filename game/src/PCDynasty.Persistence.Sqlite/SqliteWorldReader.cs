using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using PCDynasty.Persistence.Abstractions;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteWorldReader
{
    internal static async Task<WorldLoadResult?> LoadAsync(
        SqliteConnection connection,
        string saveId,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT save_format_version, world_schema_version, snapshot_sequence,
                   last_fact_sequence, snapshot_payload
            FROM saves WHERE save_id = $save_id;
            """;
        command.Parameters.AddWithValue("$save_id", saveId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        int formatVersion = reader.GetInt32(0);
        int schemaVersion = reader.GetInt32(1);
        long snapshotSequence = reader.GetInt64(2);
        long lastFactSequence = reader.GetInt64(3);
        byte[] snapshotPayload = (byte[])reader.GetValue(4);
        await reader.DisposeAsync();

        ImmutableDictionary<string, string> packHashes = await LoadPackHashesAsync(
            connection,
            saveId,
            cancellationToken);
        ImmutableArray<WorldFact> facts = await LoadFactsAsync(
            connection,
            saveId,
            snapshotSequence,
            cancellationToken);
        var manifest = new SaveManifest(
            saveId,
            formatVersion,
            schemaVersion,
            snapshotSequence,
            lastFactSequence,
            packHashes);
        return new WorldLoadResult(
            manifest,
            new WorldSnapshot(snapshotSequence, schemaVersion, snapshotPayload),
            facts);
    }

    private static async Task<ImmutableDictionary<string, string>> LoadPackHashesAsync(
        SqliteConnection connection,
        string saveId,
        CancellationToken cancellationToken)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT pack_id, pack_hash FROM save_packs WHERE save_id = $save_id ORDER BY pack_id;";
        command.Parameters.AddWithValue("$save_id", saveId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            builder.Add(reader.GetString(0), reader.GetString(1));
        }

        return builder.ToImmutable();
    }

    private static async Task<ImmutableArray<WorldFact>> LoadFactsAsync(
        SqliteConnection connection,
        string saveId,
        long afterSequence,
        CancellationToken cancellationToken)
    {
        var builder = ImmutableArray.CreateBuilder<WorldFact>();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT sequence, type_id, schema_version, payload FROM facts
            WHERE save_id = $save_id AND sequence > $sequence ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$save_id", saveId);
        command.Parameters.AddWithValue("$sequence", afterSequence);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            builder.Add(new WorldFact(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetInt32(2),
                (byte[])reader.GetValue(3)));
        }

        return builder.ToImmutable();
    }
}
