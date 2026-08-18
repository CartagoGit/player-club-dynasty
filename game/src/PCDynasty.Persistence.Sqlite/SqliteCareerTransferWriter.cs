using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Persistence.Sqlite;

internal static class SqliteCareerTransferWriter
{
    internal static bool TryInsert(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CareerTransferRecord transfer)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = """
            INSERT INTO career_transfers
                (transfer_id, buyer_club_external_id, seller_club_external_id,
                 player_external_id, amount, currency, completed_at_utc)
            VALUES
                ($transferId, $buyerClubId, $sellerClubId,
                 $playerId, $amount, $currency, $completedAt)
            ON CONFLICT DO NOTHING;
            """;
        command.Parameters.AddWithValue("$transferId", transfer.TransferId);
        command.Parameters.AddWithValue("$buyerClubId", transfer.BuyerClubExternalId);
        command.Parameters.AddWithValue("$sellerClubId", transfer.SellerClubExternalId);
        command.Parameters.AddWithValue("$playerId", transfer.PlayerExternalId);
        command.Parameters.AddWithValue("$amount", transfer.Amount);
        command.Parameters.AddWithValue("$currency", transfer.Currency);
        command.Parameters.AddWithValue(
            "$completedAt",
            transfer.CompletedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        return command.ExecuteNonQuery() == 1;
    }

    internal static void Clear(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using SqliteCommand command = CreateCommand(connection, transaction);
        command.CommandText = "DELETE FROM career_transfers;";
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
