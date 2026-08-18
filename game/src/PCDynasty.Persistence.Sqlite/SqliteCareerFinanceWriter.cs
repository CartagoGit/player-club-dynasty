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
                ($valueClubId, $valueYear, $valueCurrency, $valueCash, $valueRevenue,
                 $valueCosts, $valueDebt, $valueTransferBudget, $valueWageBudget,
                 $valueUpdatedAt)
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
        AddStateParameters(command, "value", state);
        command.ExecuteNonQuery();
    }

    internal static bool TryReplaceExpected(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CareerFinanceState expected,
        CareerFinanceState updated)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(updated);
        if (!string.Equals(expected.ClubExternalId, updated.ClubExternalId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Expected and updated finance must belong to the same club.", nameof(updated));
        }

        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = """
            UPDATE career_finance
            SET season_year = $updatedYear,
                currency = $updatedCurrency,
                cash = $updatedCash,
                revenue = $updatedRevenue,
                costs = $updatedCosts,
                debt = $updatedDebt,
                transfer_budget = $updatedTransferBudget,
                wage_budget = $updatedWageBudget,
                updated_at_utc = $updatedUpdatedAt
            WHERE club_external_id = $expectedClubId
              AND season_year = $expectedYear
              AND currency = $expectedCurrency
              AND cash = $expectedCash
              AND revenue = $expectedRevenue
              AND costs = $expectedCosts
              AND debt = $expectedDebt
              AND transfer_budget = $expectedTransferBudget
              AND wage_budget = $expectedWageBudget
              AND updated_at_utc = $expectedUpdatedAt;
            """;
        AddStateParameters(command, "expected", expected);
        AddStateParameters(command, "updated", updated);
        return command.ExecuteNonQuery() == 1;
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

    private static void AddStateParameters(
        SqliteCommand command,
        string prefix,
        CareerFinanceState state)
    {
        command.Parameters.AddWithValue($"${prefix}ClubId", state.ClubExternalId);
        command.Parameters.AddWithValue($"${prefix}Year", state.SeasonYear);
        command.Parameters.AddWithValue($"${prefix}Currency", state.Currency);
        command.Parameters.AddWithValue($"${prefix}Cash", state.Cash);
        command.Parameters.AddWithValue($"${prefix}Revenue", state.Revenue);
        command.Parameters.AddWithValue($"${prefix}Costs", state.Costs);
        command.Parameters.AddWithValue($"${prefix}Debt", state.Debt);
        command.Parameters.AddWithValue($"${prefix}TransferBudget", state.TransferBudget);
        command.Parameters.AddWithValue($"${prefix}WageBudget", state.WageBudget);
        command.Parameters.AddWithValue(
            $"${prefix}UpdatedAt",
            state.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
    }
}
