using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerLineupWriter
{
    internal static void Clear(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        foreach (string table in new[]
        {
            "career_lineup_assignment",
            "career_lineup_option",
            "career_lineup",
        })
        {
            using SqliteCommand command = CreateCommand(connection, transaction);
            command.CommandText = $"DELETE FROM {table};";
            command.ExecuteNonQuery();
        }
    }

    internal static void Save(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LineupPlan plan)
    {
        UpsertPlan(connection, transaction, plan);
        ReplaceAssignments(connection, transaction, plan);
        ReplaceOptions(connection, transaction, plan);
    }

    private static void UpsertPlan(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LineupPlan plan)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = """
            INSERT INTO career_lineup (club_external_id, contest_id, submitted_at_utc)
            VALUES ($clubId, $contestId, $submittedAt)
            ON CONFLICT(club_external_id) DO UPDATE SET
                contest_id = excluded.contest_id,
                submitted_at_utc = excluded.submitted_at_utc;
            """;
        command.Parameters.AddWithValue("$clubId", plan.ClubExternalId);
        command.Parameters.AddWithValue("$contestId", plan.ContestId);
        command.Parameters.AddWithValue(
            "$submittedAt",
            plan.SubmittedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.ExecuteNonQuery();
    }

    private static void ReplaceAssignments(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LineupPlan plan)
    {
        DeleteAssignments(connection, transaction, plan.ClubExternalId);
        foreach (LineupAssignment assignment in plan.Assignments)
        {
            using SqliteCommand command = CreateCommand(connection, transaction);
            command.CommandText = """
                INSERT INTO career_lineup_assignment
                    (club_external_id, participant_id, assignment_id, selection_order)
                VALUES ($clubId, $participantId, $assignmentId, $selectionOrder);
                """;
            command.Parameters.AddWithValue("$clubId", plan.ClubExternalId);
            command.Parameters.AddWithValue("$participantId", assignment.ParticipantId);
            command.Parameters.AddWithValue("$assignmentId", assignment.AssignmentId);
            command.Parameters.AddWithValue("$selectionOrder", assignment.Order);
            command.ExecuteNonQuery();
        }
    }

    private static void ReplaceOptions(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LineupPlan plan)
    {
        DeleteOptions(connection, transaction, plan.ClubExternalId);
        foreach ((string key, string value) in plan.Options)
        {
            using SqliteCommand command = CreateCommand(connection, transaction);
            command.CommandText = """
                INSERT INTO career_lineup_option (club_external_id, option_id, option_value)
                VALUES ($clubId, $optionId, $optionValue);
                """;
            command.Parameters.AddWithValue("$clubId", plan.ClubExternalId);
            command.Parameters.AddWithValue("$optionId", key);
            command.Parameters.AddWithValue("$optionValue", value);
            command.ExecuteNonQuery();
        }
    }

    private static void DeleteAssignments(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string clubExternalId)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = "DELETE FROM career_lineup_assignment WHERE club_external_id = $clubId;";
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        command.ExecuteNonQuery();
    }

    private static void DeleteOptions(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string clubExternalId)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = "DELETE FROM career_lineup_option WHERE club_external_id = $clubId;";
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        command.ExecuteNonQuery();
    }

    private static SqliteCommand CreateCommand(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        return command;
    }
}
