using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerTransferSchema
{
    internal static void EnsureCreated(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS career_transfers (
                transfer_id TEXT PRIMARY KEY,
                buyer_club_external_id TEXT NOT NULL,
                seller_club_external_id TEXT NOT NULL,
                player_external_id TEXT NOT NULL UNIQUE,
                amount INTEGER NOT NULL,
                currency TEXT NOT NULL,
                completed_at_utc TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_career_transfers_buyer
                ON career_transfers (buyer_club_external_id, completed_at_utc, transfer_id);
            """;
        command.ExecuteNonQuery();
    }
}
