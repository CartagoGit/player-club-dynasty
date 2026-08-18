using Microsoft.Data.Sqlite;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerHistorySchema
{
    internal static void EnsureCreated(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS match_history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                competition_display_name TEXT NOT NULL,
                home_club_id TEXT NOT NULL,
                away_club_id TEXT NOT NULL,
                home_display_name TEXT NOT NULL,
                away_display_name TEXT NOT NULL,
                home_goals INTEGER NOT NULL,
                away_goals INTEGER NOT NULL,
                seed INTEGER NOT NULL,
                played_at_utc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS season_history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                competition_display_name TEXT NOT NULL,
                managed_club_id TEXT NOT NULL,
                managed_club_name TEXT NOT NULL,
                managed_position INTEGER NOT NULL,
                managed_points INTEGER NOT NULL,
                managed_goals_for INTEGER NOT NULL,
                managed_goals_against INTEGER NOT NULL,
                total_clubs INTEGER NOT NULL,
                total_fixtures INTEGER NOT NULL,
                played_at_utc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS season_standing (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                season_id INTEGER NOT NULL REFERENCES season_history(id) ON DELETE CASCADE,
                position INTEGER NOT NULL,
                club_external_id TEXT NOT NULL,
                club_display_name TEXT NOT NULL,
                points INTEGER NOT NULL,
                played INTEGER NOT NULL,
                wins INTEGER NOT NULL,
                draws INTEGER NOT NULL,
                losses INTEGER NOT NULL,
                goals_for INTEGER NOT NULL,
                goals_against INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
