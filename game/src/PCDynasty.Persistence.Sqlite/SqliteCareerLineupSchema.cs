using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerLineupSchema
{
    internal static void EnsureCreated(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS career_lineup (
                club_external_id TEXT PRIMARY KEY,
                contest_id TEXT NOT NULL,
                submitted_at_utc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS career_lineup_assignment (
                club_external_id TEXT NOT NULL,
                participant_id TEXT NOT NULL,
                assignment_id TEXT NOT NULL,
                selection_order INTEGER NOT NULL,
                PRIMARY KEY (club_external_id, participant_id),
                UNIQUE (club_external_id, selection_order)
            );
            CREATE TABLE IF NOT EXISTS career_lineup_option (
                club_external_id TEXT NOT NULL,
                option_id TEXT NOT NULL,
                option_value TEXT NOT NULL,
                PRIMARY KEY (club_external_id, option_id)
            );
            """;
        command.ExecuteNonQuery();
    }
}
