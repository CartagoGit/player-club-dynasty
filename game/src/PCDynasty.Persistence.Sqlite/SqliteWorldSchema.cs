using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteWorldSchema
{
    private const string CreateSql = """
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

    internal static async Task EnsureCreatedAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = CreateSql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
