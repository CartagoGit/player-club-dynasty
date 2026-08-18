using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerHistoryReader
{
    internal static IReadOnlyList<HistoryEntry> LoadMatches(SqliteConnection connection)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT competition_display_name, home_club_id, away_club_id,
                   home_display_name, away_display_name,
                   home_goals, away_goals, seed, played_at_utc
            FROM match_history
            ORDER BY played_at_utc ASC;
            """;
        using SqliteDataReader reader = command.ExecuteReader();
        var result = ImmutableArray.CreateBuilder<HistoryEntry>();
        while (reader.Read())
        {
            result.Add(new HistoryEntry(
                CompetitionDisplayName: reader.GetString(0),
                HomeClubId: reader.GetString(1),
                AwayClubId: reader.GetString(2),
                HomeDisplayName: reader.GetString(3),
                AwayDisplayName: reader.GetString(4),
                HomeGoals: reader.GetInt32(5),
                AwayGoals: reader.GetInt32(6),
                Seed: reader.GetInt64(7),
                PlayedAtUtc: ParseDate(reader.GetString(8))));
        }

        return result.ToImmutable();
    }

    internal static IReadOnlyList<SeasonHistoryEntry> LoadSeasons(SqliteConnection connection)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, competition_display_name, managed_club_id, managed_club_name,
                   managed_position, managed_points,
                   managed_goals_for, managed_goals_against,
                   total_clubs, total_fixtures, played_at_utc
            FROM season_history
            ORDER BY played_at_utc ASC;
            """;
        using SqliteDataReader reader = command.ExecuteReader();
        var result = ImmutableArray.CreateBuilder<SeasonHistoryEntry>();
        while (reader.Read())
        {
            long seasonId = reader.GetInt64(0);
            result.Add(new SeasonHistoryEntry(
                CompetitionDisplayName: reader.GetString(1),
                ManagedClubId: reader.GetString(2),
                ManagedClubDisplayName: reader.GetString(3),
                ManagedPosition: reader.GetInt32(4),
                ManagedPoints: reader.GetInt32(5),
                ManagedGoalsFor: reader.GetInt32(6),
                ManagedGoalsAgainst: reader.GetInt32(7),
                TotalClubs: reader.GetInt32(8),
                TotalFixtures: reader.GetInt32(9),
                PlayedAtUtc: ParseDate(reader.GetString(10)),
                FinalStandings: LoadStandings(connection, seasonId)));
        }

        return result.ToImmutable();
    }

    private static ImmutableArray<StandingRow> LoadStandings(
        SqliteConnection connection,
        long seasonId)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT club_external_id, club_display_name,
                   points, played, wins, draws, losses, goals_for, goals_against
            FROM season_standing
            WHERE season_id = $seasonId
            ORDER BY position ASC;
            """;
        command.Parameters.AddWithValue("$seasonId", seasonId);
        using SqliteDataReader reader = command.ExecuteReader();
        var rows = ImmutableArray.CreateBuilder<StandingRow>();
        while (reader.Read())
        {
            rows.Add(new StandingRow(
                ClubExternalId: reader.GetString(0),
                DisplayName: reader.GetString(1),
                ShortName: reader.GetString(1),
                Points: reader.GetInt32(2),
                Played: reader.GetInt32(3),
                Wins: reader.GetInt32(4),
                Draws: reader.GetInt32(5),
                Losses: reader.GetInt32(6),
                GoalsFor: reader.GetInt32(7),
                GoalsAgainst: reader.GetInt32(8)));
        }

        return rows.ToImmutable();
    }

    private static DateTime ParseDate(string value) =>
        DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
