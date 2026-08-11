using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// SQLite-backed persistence for the session-level career history:
/// every played match and every completed season. The store is opened
/// lazily on the first save and survives process restarts.
/// </summary>
/// <remarks>
/// Pure-local: the on-disk SQLite lives in <c>saves/career.sqlite</c> by
/// default so users can find and back up their saves easily. Phase 5
/// swaps this for the engine-neutral <c>IWorldStore</c> pipeline.
/// </remarks>
public sealed class CareerStore
{
    private readonly string _databasePath;
    private readonly object _gate = new();
    private bool _initialised;

    public CareerStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        _databasePath = databasePath;
    }

    public string DatabasePath => _databasePath;

    public void SaveMatch(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_gate)
        {
            EnsureSchema();
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO match_history
                    (competition_display_name, home_club_id, away_club_id,
                     home_display_name, away_display_name,
                     home_goals, away_goals, seed, played_at_utc)
                VALUES
                    ($competition, $homeId, $awayId,
                     $homeName, $awayName,
                     $homeGoals, $awayGoals, $seed, $playedAt);";
            command.Parameters.AddWithValue("$competition", entry.CompetitionDisplayName);
            command.Parameters.AddWithValue("$homeId", entry.HomeClubId);
            command.Parameters.AddWithValue("$awayId", entry.AwayClubId);
            command.Parameters.AddWithValue("$homeName", entry.HomeDisplayName);
            command.Parameters.AddWithValue("$awayName", entry.AwayDisplayName);
            command.Parameters.AddWithValue("$homeGoals", entry.HomeGoals);
            command.Parameters.AddWithValue("$awayGoals", entry.AwayGoals);
            command.Parameters.AddWithValue("$seed", entry.Seed);
            command.Parameters.AddWithValue("$playedAt", entry.PlayedAtUtc.ToString("O", CultureInfo.InvariantCulture));
            command.ExecuteNonQuery();
        }
    }

    public void SaveSeason(SeasonHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_gate)
        {
            EnsureSchema();
            using var connection = Open();
            using var transaction = connection.BeginTransaction();

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
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
                    SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$competition", entry.CompetitionDisplayName);
                command.Parameters.AddWithValue("$managedId", entry.ManagedClubId);
                command.Parameters.AddWithValue("$managedName", entry.ManagedClubDisplayName);
                command.Parameters.AddWithValue("$position", entry.ManagedPosition);
                command.Parameters.AddWithValue("$points", entry.ManagedPoints);
                command.Parameters.AddWithValue("$gf", entry.ManagedGoalsFor);
                command.Parameters.AddWithValue("$ga", entry.ManagedGoalsAgainst);
                command.Parameters.AddWithValue("$totalClubs", entry.TotalClubs);
                command.Parameters.AddWithValue("$totalFixtures", entry.TotalFixtures);
                command.Parameters.AddWithValue("$playedAt", entry.PlayedAtUtc.ToString("O", CultureInfo.InvariantCulture));
                long seasonId = (long)(command.ExecuteScalar() ?? 0L);

                foreach (var standing in entry.FinalStandings)
                {
                    using var standingCommand = connection.CreateCommand();
                    standingCommand.Transaction = transaction;
                    standingCommand.CommandText = @"
                        INSERT INTO season_standing
                            (season_id, position, club_external_id, club_display_name,
                             points, played, wins, draws, losses, goals_for, goals_against)
                        VALUES
                            ($seasonId, $position, $clubId, $clubName,
                             $points, $played, $wins, $draws, $losses, $gf, $ga);";
                    standingCommand.Parameters.AddWithValue("$seasonId", seasonId);
                    standingCommand.Parameters.AddWithValue("$position", entry.FinalStandings.IndexOf(standing) + 1);
                    standingCommand.Parameters.AddWithValue("$clubId", standing.ClubExternalId);
                    standingCommand.Parameters.AddWithValue("$clubName", standing.DisplayName);
                    standingCommand.Parameters.AddWithValue("$points", standing.Points);
                    standingCommand.Parameters.AddWithValue("$played", standing.Played);
                    standingCommand.Parameters.AddWithValue("$wins", standing.Wins);
                    standingCommand.Parameters.AddWithValue("$draws", standing.Draws);
                    standingCommand.Parameters.AddWithValue("$losses", standing.Losses);
                    standingCommand.Parameters.AddWithValue("$gf", standing.GoalsFor);
                    standingCommand.Parameters.AddWithValue("$ga", standing.GoalsAgainst);
                    standingCommand.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }
    }

    public IReadOnlyList<HistoryEntry> LoadMatches()
    {
        lock (_gate)
        {
            EnsureSchema();
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT competition_display_name, home_club_id, away_club_id,
                       home_display_name, away_display_name,
                       home_goals, away_goals, seed, played_at_utc
                FROM match_history
                ORDER BY played_at_utc ASC;";
            using var reader = command.ExecuteReader();
            var result = new List<HistoryEntry>();
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
                        PlayedAtUtc: DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));
                }
            return result.ToImmutableArray();
        }
    }

    public IReadOnlyList<SeasonHistoryEntry> LoadSeasons()
    {
        lock (_gate)
        {
            EnsureSchema();
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, competition_display_name, managed_club_id, managed_club_name,
                       managed_position, managed_points,
                       managed_goals_for, managed_goals_against,
                       total_clubs, total_fixtures, played_at_utc
                FROM season_history
                ORDER BY played_at_utc ASC;";
            using var reader = command.ExecuteReader();
            var result = new List<SeasonHistoryEntry>();
            while (reader.Read())
                {
                    long seasonId = reader.GetInt64(0);
                    var standings = LoadStandingsForSeason(connection, seasonId);
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
                        PlayedAtUtc: DateTime.Parse(reader.GetString(10), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        FinalStandings: standings));
                }
            return result.ToImmutableArray();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            EnsureSchema();
            using var connection = Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM season_standing; DELETE FROM season_history; DELETE FROM match_history;";
            command.ExecuteNonQuery();
        }
    }

    private static ImmutableArray<StandingRow> LoadStandingsForSeason(SqliteConnection connection, long seasonId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT club_external_id, club_display_name,
                   points, played, wins, draws, losses, goals_for, goals_against
            FROM season_standing
            WHERE season_id = $seasonId
            ORDER BY position ASC;";
        command.Parameters.AddWithValue("$seasonId", seasonId);

        using var reader = command.ExecuteReader();
        var rows = new List<StandingRow>();
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
        return rows.ToImmutableArray();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();
        return connection;
    }

    private void EnsureSchema()
    {
        if (_initialised)
        {
            return;
        }

        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
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
            );";
        command.ExecuteNonQuery();
        _initialised = true;
    }
}