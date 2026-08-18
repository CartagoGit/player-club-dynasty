using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Persistence.Abstractions;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteWorldWriter
{
    internal static async Task AppendAsync(
        SqliteConnection connection,
        string saveId,
        long expectedLastFactSequence,
        ImmutableArray<WorldFact> facts,
        CancellationToken cancellationToken)
    {
        await using SqliteTransaction transaction = connection.BeginTransaction();
        long storedSequence = await GetLastFactSequenceAsync(
            connection,
            transaction,
            saveId,
            cancellationToken);
        if (storedSequence != expectedLastFactSequence)
        {
            throw new WorldStoreConcurrencyException(
                $"Expected fact sequence {expectedLastFactSequence}, but the save is at {storedSequence}.");
        }

        foreach (WorldFact fact in facts)
        {
            await InsertFactAsync(connection, transaction, saveId, fact, cancellationToken);
        }

        await UpdateLastSequenceAsync(connection, transaction, saveId, facts[^1].Sequence, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    internal static async Task CheckpointAsync(
        SqliteConnection connection,
        SaveManifest manifest,
        WorldSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        await using SqliteTransaction transaction = connection.BeginTransaction();
        long? storedSequence = await TryGetLastFactSequenceAsync(
            connection,
            transaction,
            manifest.SaveId,
            cancellationToken);
        if (storedSequence is not null && storedSequence != manifest.LastFactSequence)
        {
            throw new WorldStoreConcurrencyException(
                $"Cannot checkpoint sequence {manifest.LastFactSequence}; stored sequence is {storedSequence}.");
        }

        await UpsertSaveAsync(connection, transaction, manifest, snapshot, cancellationToken);
        await ReplacePackHashesAsync(connection, transaction, manifest, cancellationToken);
        await DeleteCheckpointedFactsAsync(connection, transaction, manifest, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task InsertFactAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string saveId,
        WorldFact fact,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO facts(save_id, sequence, type_id, schema_version, payload)
            VALUES($save_id, $sequence, $type_id, $schema_version, $payload);
            """;
        command.Parameters.AddWithValue("$save_id", saveId);
        command.Parameters.AddWithValue("$sequence", fact.Sequence);
        command.Parameters.AddWithValue("$type_id", fact.TypeId);
        command.Parameters.AddWithValue("$schema_version", fact.SchemaVersion);
        command.Parameters.AddWithValue("$payload", fact.Payload.AsSpan().ToArray());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateLastSequenceAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string saveId,
        long sequence,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE saves SET last_fact_sequence = $sequence WHERE save_id = $save_id;";
        command.Parameters.AddWithValue("$sequence", sequence);
        command.Parameters.AddWithValue("$save_id", saveId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> GetLastFactSequenceAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string saveId,
        CancellationToken cancellationToken)
    {
        return await TryGetLastFactSequenceAsync(connection, transaction, saveId, cancellationToken)
            ?? throw new KeyNotFoundException($"Save '{saveId}' does not exist.");
    }

    private static async Task<long?> TryGetLastFactSequenceAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string saveId,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT last_fact_sequence FROM saves WHERE save_id = $save_id;";
        command.Parameters.AddWithValue("$save_id", saveId);
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null ? null : Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static async Task UpsertSaveAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        SaveManifest manifest,
        WorldSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO saves(save_id, save_format_version, world_schema_version,
                              snapshot_sequence, last_fact_sequence, snapshot_payload)
            VALUES($save_id, $format, $schema, $snapshot, $last, $payload)
            ON CONFLICT(save_id) DO UPDATE SET
                save_format_version = excluded.save_format_version,
                world_schema_version = excluded.world_schema_version,
                snapshot_sequence = excluded.snapshot_sequence,
                last_fact_sequence = excluded.last_fact_sequence,
                snapshot_payload = excluded.snapshot_payload;
            """;
        command.Parameters.AddWithValue("$save_id", manifest.SaveId);
        command.Parameters.AddWithValue("$format", manifest.SaveFormatVersion);
        command.Parameters.AddWithValue("$schema", manifest.WorldSchemaVersion);
        command.Parameters.AddWithValue("$snapshot", manifest.SnapshotSequence);
        command.Parameters.AddWithValue("$last", manifest.LastFactSequence);
        command.Parameters.AddWithValue("$payload", snapshot.Payload.AsSpan().ToArray());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ReplacePackHashesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        SaveManifest manifest,
        CancellationToken cancellationToken)
    {
        await using (SqliteCommand delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM save_packs WHERE save_id = $save_id;";
            delete.Parameters.AddWithValue("$save_id", manifest.SaveId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach ((string packId, string hash) in manifest.PackHashes)
        {
            await using SqliteCommand insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO save_packs(save_id, pack_id, pack_hash) VALUES($save_id, $pack_id, $hash);";
            insert.Parameters.AddWithValue("$save_id", manifest.SaveId);
            insert.Parameters.AddWithValue("$pack_id", packId);
            insert.Parameters.AddWithValue("$hash", hash);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task DeleteCheckpointedFactsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        SaveManifest manifest,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM facts WHERE save_id = $save_id AND sequence <= $sequence;";
        command.Parameters.AddWithValue("$save_id", manifest.SaveId);
        command.Parameters.AddWithValue("$sequence", manifest.SnapshotSequence);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
