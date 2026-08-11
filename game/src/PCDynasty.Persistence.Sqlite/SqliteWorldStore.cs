using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using PCDynasty.Persistence.Abstractions;

namespace PCDynasty.Persistence.Sqlite;

public sealed class WorldStoreConcurrencyException : InvalidOperationException
{
    public WorldStoreConcurrencyException(string message)
        : base(message)
    {
    }
}

public sealed class SqliteWorldStore : IWorldStore
{
    private const string SchemaSql = """
        PRAGMA foreign_keys = ON;
        CREATE TABLE IF NOT EXISTS saves (
            save_id TEXT PRIMARY KEY,
            save_format_version INTEGER NOT NULL,
            world_schema_version INTEGER NOT NULL,
            snapshot_sequence INTEGER NOT NULL,
            last_fact_sequence INTEGER NOT NULL,
            snapshot_payload BLOB NOT NULL
        );
        CREATE TABLE IF NOT EXISTS save_packs (
            save_id TEXT NOT NULL,
            pack_id TEXT NOT NULL,
            pack_hash TEXT NOT NULL,
            PRIMARY KEY (save_id, pack_id),
            FOREIGN KEY (save_id) REFERENCES saves(save_id) ON DELETE CASCADE
        );
        CREATE TABLE IF NOT EXISTS facts (
            save_id TEXT NOT NULL,
            sequence INTEGER NOT NULL,
            type_id TEXT NOT NULL,
            schema_version INTEGER NOT NULL,
            payload BLOB NOT NULL,
            PRIMARY KEY (save_id, sequence),
            FOREIGN KEY (save_id) REFERENCES saves(save_id) ON DELETE CASCADE
        );
        """;

    private readonly string connectionString;

    public SqliteWorldStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(databasePath),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        }.ToString();
    }

    public async ValueTask<WorldLoadResult?> LoadAsync(
        string saveId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveId);
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand saveCommand = connection.CreateCommand();
        saveCommand.CommandText = """
            SELECT save_format_version, world_schema_version, snapshot_sequence,
                   last_fact_sequence, snapshot_payload
            FROM saves WHERE save_id = $save_id;
            """;
        saveCommand.Parameters.AddWithValue("$save_id", saveId);
        await using SqliteDataReader saveReader = await saveCommand.ExecuteReaderAsync(cancellationToken);
        if (!await saveReader.ReadAsync(cancellationToken))
        {
            return null;
        }

        int formatVersion = saveReader.GetInt32(0);
        int schemaVersion = saveReader.GetInt32(1);
        long snapshotSequence = saveReader.GetInt64(2);
        long lastFactSequence = saveReader.GetInt64(3);
        byte[] snapshotPayload = (byte[])saveReader.GetValue(4);
        await saveReader.DisposeAsync();

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

    public async ValueTask AppendAsync(
        string saveId,
        long expectedLastFactSequence,
        ImmutableArray<WorldFact> facts,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveId);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedLastFactSequence);
        if (facts.IsEmpty)
        {
            return;
        }

        long expectedSequence = expectedLastFactSequence + 1;
        foreach (WorldFact fact in facts)
        {
            ArgumentNullException.ThrowIfNull(fact);
            if (fact.Sequence != expectedSequence++)
            {
                throw new ArgumentException("Appended facts must be contiguous.", nameof(facts));
            }
        }

        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = connection.BeginTransaction();
        long storedSequence = await GetLastFactSequenceAsync(connection, transaction, saveId, cancellationToken);
        if (storedSequence != expectedLastFactSequence)
        {
            throw new WorldStoreConcurrencyException(
                $"Expected fact sequence {expectedLastFactSequence}, but the save is at {storedSequence}.");
        }

        foreach (WorldFact fact in facts)
        {
            await using SqliteCommand insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO facts(save_id, sequence, type_id, schema_version, payload)
                VALUES($save_id, $sequence, $type_id, $schema_version, $payload);
                """;
            insert.Parameters.AddWithValue("$save_id", saveId);
            insert.Parameters.AddWithValue("$sequence", fact.Sequence);
            insert.Parameters.AddWithValue("$type_id", fact.TypeId);
            insert.Parameters.AddWithValue("$schema_version", fact.SchemaVersion);
            insert.Parameters.AddWithValue("$payload", fact.Payload.AsSpan().ToArray());
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using SqliteCommand update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = "UPDATE saves SET last_fact_sequence = $sequence WHERE save_id = $save_id;";
        update.Parameters.AddWithValue("$sequence", facts[^1].Sequence);
        update.Parameters.AddWithValue("$save_id", saveId);
        await update.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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

        await using SqliteConnection connection = await OpenAsync(cancellationToken);
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

    private async ValueTask<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqliteCommand schema = connection.CreateCommand();
        schema.CommandText = SchemaSql;
        await schema.ExecuteNonQueryAsync(cancellationToken);
        return connection;
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
        return value is null ? null : Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
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
