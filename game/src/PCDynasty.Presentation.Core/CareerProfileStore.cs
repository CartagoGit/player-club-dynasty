using System.Globalization;
using Microsoft.Data.Sqlite;

namespace PCDynasty.Presentation.Core;

public sealed record CareerProfile
{
    public CareerProfile(
        string clubExternalId,
        string clubDisplayName,
        DateTimeOffset startedAtUtc,
        DateTimeOffset lastPlayedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(clubExternalId))
        {
            throw new ArgumentException("Club identity cannot be empty.", nameof(clubExternalId));
        }

        if (string.IsNullOrWhiteSpace(clubDisplayName))
        {
            throw new ArgumentException("Club display name cannot be empty.", nameof(clubDisplayName));
        }

        if (lastPlayedAtUtc < startedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastPlayedAtUtc),
                "Last-played time cannot precede career start time.");
        }

        ClubExternalId = clubExternalId;
        ClubDisplayName = clubDisplayName;
        StartedAtUtc = startedAtUtc;
        LastPlayedAtUtc = lastPlayedAtUtc;
    }

    public string ClubExternalId { get; }

    public string ClubDisplayName { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset LastPlayedAtUtc { get; }
}

/// <summary>
/// Stores the single active career identity in a caller-selected SQLite file.
/// </summary>
public sealed class CareerProfileStore
{
    private const int ActiveProfileId = 1;
    private readonly object _gate = new();
    private bool _schemaInitialized;

    public CareerProfileStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public void Save(CareerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        lock (_gate)
        {
            EnsureParentDirectory();
            using var connection = Open();
            EnsureSchema(connection);
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO career_profile
                    (singleton_id, club_external_id, club_display_name, started_at_utc, last_played_at_utc)
                VALUES
                    ($id, $clubId, $clubName, $startedAt, $lastPlayedAt)
                ON CONFLICT(singleton_id) DO UPDATE SET
                    club_external_id = excluded.club_external_id,
                    club_display_name = excluded.club_display_name,
                    started_at_utc = excluded.started_at_utc,
                    last_played_at_utc = excluded.last_played_at_utc;
                """;
            command.Parameters.AddWithValue("$id", ActiveProfileId);
            command.Parameters.AddWithValue("$clubId", profile.ClubExternalId);
            command.Parameters.AddWithValue("$clubName", profile.ClubDisplayName);
            command.Parameters.AddWithValue("$startedAt", Format(profile.StartedAtUtc));
            command.Parameters.AddWithValue("$lastPlayedAt", Format(profile.LastPlayedAtUtc));
            command.ExecuteNonQuery();
        }
    }

    public bool TryLoad(out CareerProfile? profile)
    {
        lock (_gate)
        {
            if (!File.Exists(DatabasePath))
            {
                profile = null;
                return false;
            }

            using var connection = Open();
            EnsureSchema(connection);
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT club_external_id, club_display_name, started_at_utc, last_played_at_utc
                FROM career_profile
                WHERE singleton_id = $id;
                """;
            command.Parameters.AddWithValue("$id", ActiveProfileId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                profile = null;
                return false;
            }

            profile = new CareerProfile(
                reader.GetString(0),
                reader.GetString(1),
                Parse(reader.GetString(2)),
                Parse(reader.GetString(3)));
            return true;
        }
    }

    private static string Format(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset Parse(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private void EnsureParentDirectory()
    {
        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void EnsureSchema(SqliteConnection connection)
    {
        if (_schemaInitialized)
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS career_profile (
                singleton_id INTEGER PRIMARY KEY CHECK (singleton_id = 1),
                club_external_id TEXT NOT NULL,
                club_display_name TEXT NOT NULL,
                started_at_utc TEXT NOT NULL,
                last_played_at_utc TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
        _schemaInitialized = true;
    }

    private SqliteConnection Open()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
        }.ToString();
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}
