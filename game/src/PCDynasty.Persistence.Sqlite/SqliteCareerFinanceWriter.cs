using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerFinanceWriter
{
    internal static void Save(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CareerFinanceState state)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = """
            INSERT INTO career_finance
                (club_external_id, season_year, currency, cash, revenue, costs, debt,
                 transfer_budget, wage_budget, updated_at_utc)
            VALUES
                ($clubId, $year, $currency, $cash, $revenue, $costs, $debt,
                 $transferBudget, $wageBudget, $updatedAt)
            ON CONFLICT(club_external_id) DO UPDATE SET
                season_year = excluded.season_year,
                currency = excluded.currency,
                cash = excluded.cash,
                revenue = excluded.revenue,
                costs = excluded.costs,
                debt = excluded.debt,
                transfer_budget = excluded.transfer_budget,
                wage_budget = excluded.wage_budget,
                updated_at_utc = excluded.updated_at_utc;
            """;
        command.Parameters.AddWithValue("$clubId", state.ClubExternalId);
        command.Parameters.AddWithValue("$year", state.SeasonYear);
        command.Parameters.AddWithValue("$currency", state.Currency);
        command.Parameters.AddWithValue("$cash", state.Cash);
        command.Parameters.AddWithValue("$revenue", state.Revenue);
        command.Parameters.AddWithValue("$costs", state.Costs);
        command.Parameters.AddWithValue("$debt", state.Debt);
        command.Parameters.AddWithValue("$transferBudget", state.TransferBudget);
        command.Parameters.AddWithValue("$wageBudget", state.WageBudget);
        command.Parameters.AddWithValue(
            "$updatedAt",
            state.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.ExecuteNonQuery();
    }

    internal static void Clear(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = "DELETE FROM career_finance;";
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
