using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerTransferReader
{
    internal static IReadOnlyList<CareerTransferRecord> ListForClub(
        SqliteConnection connection,
        string clubExternalId)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT transfer_id, seller_club_external_id, player_external_id,
                   amount, currency, completed_at_utc
            FROM career_transfers
            WHERE buyer_club_external_id = $clubId
            ORDER BY completed_at_utc, transfer_id;
            """;
        command.Parameters.AddWithValue("$clubId", clubExternalId);
        using SqliteDataReader reader = command.ExecuteReader();
        var records = ImmutableArray.CreateBuilder<CareerTransferRecord>();
        while (reader.Read())
        {
            records.Add(new CareerTransferRecord(
                reader.GetString(0),
                clubExternalId,
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt64(3),
                reader.GetString(4),
                DateTimeOffset.Parse(
                    reader.GetString(5),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)));
        }

        return records.ToImmutable();
    }
}
