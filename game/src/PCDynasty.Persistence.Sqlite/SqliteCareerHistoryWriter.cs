using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerHistoryWriter
{
    internal static void SaveMatch(SqliteConnection connection, HistoryEntry entry)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO match_history
                (competition_display_name, home_club_id, away_club_id,
                 home_display_name, away_display_name,
                 home_goals, away_goals, seed, played_at_utc)
            VALUES
                ($competition, $homeId, $awayId,
                 $homeName, $awayName,
                 $homeGoals, $awayGoals, $seed, $playedAt);
            """;
        command.Parameters.AddWithValue("$competition", entry.CompetitionDisplayName);
        command.Parameters.AddWithValue("$homeId", entry.HomeClubId);
        command.Parameters.AddWithValue("$awayId", entry.AwayClubId);
        command.Parameters.AddWithValue("$homeName", entry.HomeDisplayName);
        command.Parameters.AddWithValue("$awayName", entry.AwayDisplayName);
        command.Parameters.AddWithValue("$homeGoals", entry.HomeGoals);
        command.Parameters.AddWithValue("$awayGoals", entry.AwayGoals);
        command.Parameters.AddWithValue("$seed", entry.Seed);
        command.Parameters.AddWithValue("$playedAt", FormatDate(entry.PlayedAtUtc));
        command.ExecuteNonQuery();
    }

    internal static void SaveSeason(SqliteConnection connection, SeasonHistoryEntry entry)
    {
        using SqliteTransaction transaction = connection.BeginTransaction();
        long seasonId = InsertSeason(connection, transaction, entry);
        for (int position = 0; position < entry.FinalStandings.Length; position++)
        {
            InsertStanding(connection, transaction, seasonId, position + 1, entry.FinalStandings[position]);
        }

        transaction.Commit();
    }

    internal static void Clear(SqliteConnection connection)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM season_standing; DELETE FROM season_history; DELETE FROM match_history;";
        command.ExecuteNonQuery();
    }

    private static long InsertSeason(
        SqliteConnection connection,
        SqliteTransaction transaction,
        SeasonHistoryEntry entry)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO season_history
                (competition_display_name, managed_club_id, managed_club_name,
                 managed_position, managed_points,
                 managed_goals_for, managed_goals_against,
                 total_clubs, total_fixtures, played_at_utc)
            VALUES
                ($competition, $managedId, $managedName,
                 $position, $points,
                 $gf, $ga,
                 $totalClubs, $totalFixtures, $playedAt);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$competition", entry.CompetitionDisplayName);
        command.Parameters.AddWithValue("$managedId", entry.ManagedClubId);
        command.Parameters.AddWithValue("$managedName", entry.ManagedClubDisplayName);
        command.Parameters.AddWithValue("$position", entry.ManagedPosition);
        command.Parameters.AddWithValue("$points", entry.ManagedPoints);
        command.Parameters.AddWithValue("$gf", entry.ManagedGoalsFor);
        command.Parameters.AddWithValue("$ga", entry.ManagedGoalsAgainst);
        command.Parameters.AddWithValue("$totalClubs", entry.TotalClubs);
        command.Parameters.AddWithValue("$totalFixtures", entry.TotalFixtures);
        command.Parameters.AddWithValue("$playedAt", FormatDate(entry.PlayedAtUtc));
        return (long)(command.ExecuteScalar() ?? 0L);
    }

    private static void InsertStanding(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long seasonId,
        int position,
        StandingRow standing)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO season_standing
                (season_id, position, club_external_id, club_display_name,
                 points, played, wins, draws, losses, goals_for, goals_against)
            VALUES
                ($seasonId, $position, $clubId, $clubName,
                 $points, $played, $wins, $draws, $losses, $gf, $ga);
            """;
        command.Parameters.AddWithValue("$seasonId", seasonId);
        command.Parameters.AddWithValue("$position", position);
        command.Parameters.AddWithValue("$clubId", standing.ClubExternalId);
        command.Parameters.AddWithValue("$clubName", standing.DisplayName);
        command.Parameters.AddWithValue("$points", standing.Points);
        command.Parameters.AddWithValue("$played", standing.Played);
        command.Parameters.AddWithValue("$wins", standing.Wins);
        command.Parameters.AddWithValue("$draws", standing.Draws);
        command.Parameters.AddWithValue("$losses", standing.Losses);
        command.Parameters.AddWithValue("$gf", standing.GoalsFor);
        command.Parameters.AddWithValue("$ga", standing.GoalsAgainst);
        command.ExecuteNonQuery();
    }

    private static string FormatDate(DateTime value) =>
        value.ToString("O", CultureInfo.InvariantCulture);
}
