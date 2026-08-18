using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerFinanceSchema
{
    internal static void EnsureCreated(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS career_finance (
                club_external_id TEXT PRIMARY KEY,
                season_year INTEGER NOT NULL,
                currency TEXT NOT NULL,
                cash INTEGER NOT NULL,
                revenue INTEGER NOT NULL,
                costs INTEGER NOT NULL,
                debt INTEGER NOT NULL,
                transfer_budget INTEGER NOT NULL,
                wage_budget INTEGER NOT NULL,
                updated_at_utc TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
