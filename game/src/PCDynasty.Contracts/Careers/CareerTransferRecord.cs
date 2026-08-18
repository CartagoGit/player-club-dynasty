namespace PCDynasty.Contracts.Careers;

public sealed record CareerTransferRecord
{
    public CareerTransferRecord(
        string transferId,
        string buyerClubExternalId,
        string sellerClubExternalId,
        string playerExternalId,
        long amount,
        string currency,
        DateTimeOffset completedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        TransferId = CareerContractValue.Required(transferId, nameof(transferId));
        BuyerClubExternalId = CareerContractValue.Required(
            buyerClubExternalId,
            nameof(buyerClubExternalId));
        SellerClubExternalId = CareerContractValue.Required(
            sellerClubExternalId,
            nameof(sellerClubExternalId));
        if (string.Equals(BuyerClubExternalId, SellerClubExternalId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Buyer and seller clubs must be different.", nameof(sellerClubExternalId));
        }

        PlayerExternalId = CareerContractValue.Required(playerExternalId, nameof(playerExternalId));
        Currency = CareerContractValue.Required(currency, nameof(currency)).ToUpperInvariant();
        if (Currency.Length != 3 || Currency.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currency));
        }

        Amount = amount;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
    }

    public string TransferId { get; }

    public string BuyerClubExternalId { get; }

    public string SellerClubExternalId { get; }

    public string PlayerExternalId { get; }

    public long Amount { get; }

    public string Currency { get; }

    public DateTimeOffset CompletedAtUtc { get; }
}
