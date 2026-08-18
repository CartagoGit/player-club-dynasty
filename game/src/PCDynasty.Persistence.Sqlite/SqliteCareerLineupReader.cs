using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerLineupReader
{
    internal static bool TryLoad(
        SqliteConnection connection,
        string clubExternalId,
        out LineupPlan? plan)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT contest_id, submitted_at_utc
            FROM career_lineup
            WHERE club_external_id = $clubId;
            """;
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            plan = null;
            return false;
        }

        string contestId = reader.GetString(0);
        DateTimeOffset submittedAtUtc = DateTimeOffset.Parse(
            reader.GetString(1),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
        reader.Close();
        plan = new LineupPlan(
            clubExternalId,
            contestId,
            submittedAtUtc,
            LoadAssignments(connection, clubExternalId),
            LoadOptions(connection, clubExternalId));
        return true;
    }

    private static List<LineupAssignment> LoadAssignments(
        SqliteConnection connection,
        string clubExternalId)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT participant_id, assignment_id, selection_order
            FROM career_lineup_assignment
            WHERE club_external_id = $clubId
            ORDER BY selection_order;
            """;
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        using SqliteDataReader reader = command.ExecuteReader();
        var assignments = new List<LineupAssignment>();
        while (reader.Read())
        {
            assignments.Add(new LineupAssignment(reader.GetString(0), reader.GetString(1), reader.GetInt32(2)));
        }

        return assignments;
    }

    private static List<KeyValuePair<string, string>> LoadOptions(
        SqliteConnection connection,
        string clubExternalId)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT option_id, option_value
            FROM career_lineup_option
            WHERE club_external_id = $clubId
            ORDER BY option_id;
            """;
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        using SqliteDataReader reader = command.ExecuteReader();
        var options = new List<KeyValuePair<string, string>>();
        while (reader.Read())
        {
            options.Add(KeyValuePair.Create(reader.GetString(0), reader.GetString(1)));
        }

        return options;
    }
}
