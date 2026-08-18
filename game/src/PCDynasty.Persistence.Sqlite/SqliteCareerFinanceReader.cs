using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerFinanceReader
{
    internal static bool TryLoad(
        SqliteConnection connection,
        string clubExternalId,
        out CareerFinanceState? state)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT season_year, currency, cash, revenue, costs, debt,
                   transfer_budget, wage_budget, updated_at_utc
            FROM career_finance
            WHERE club_external_id = $clubId;
            """;
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            state = null;
            return false;
        }

        state = new CareerFinanceState(
            clubExternalId,
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            reader.GetInt64(4),
            reader.GetInt64(5),
            reader.GetInt64(6),
            reader.GetInt64(7),
            DateTimeOffset.Parse(
                reader.GetString(8),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind));
        return true;
    }
}
