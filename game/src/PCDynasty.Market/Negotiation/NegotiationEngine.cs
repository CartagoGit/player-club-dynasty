using PCDynasty.Market.Valuation;

namespace PCDynasty.Market.Negotiation;

public sealed class MarketBid
{
    public MarketBid(
        string bidId,
        string buyerClubId,
        PlayerValuation valuation,
        TransferPrice offeredPrice,
        int needPriority)
    {
        if (needPriority is < 0 or > 1_000)
        {
            throw new ArgumentOutOfRangeException(nameof(needPriority));
        }

        BidId = NegotiationValue.Required(bidId, nameof(bidId));
        BuyerClubId = NegotiationValue.Required(buyerClubId, nameof(buyerClubId));
        Valuation = valuation ?? throw new ArgumentNullException(nameof(valuation));
        if (string.IsNullOrWhiteSpace(offeredPrice.Amount.Currency))
        {
            throw new ArgumentException("A valid offered price is required.", nameof(offeredPrice));
        }

        OfferedPrice = offeredPrice;
        NeedPriority = needPriority;
    }

    public string BidId { get; }
    public string BuyerClubId { get; }
    public PlayerValuation Valuation { get; }
    public TransferPrice OfferedPrice { get; }
    public int NeedPriority { get; }
}

public sealed record NegotiationResolution(
    string BidId,
    string BuyerClubId,
    string SubjectId,
    bool Accepted,
    TransferPrice? AgreedPrice,
    string Reason);

public static class NegotiationEngine
{
    public static NegotiationResolution Evaluate(
        MarketBid bid,
        MoneyAmount availableBudget)
    {
        ArgumentNullException.ThrowIfNull(bid);
        MoneyAmount offered = bid.OfferedPrice.Amount;
        MoneyAmount asking = bid.Valuation.Asking.Amount;
        if (string.IsNullOrWhiteSpace(availableBudget.Currency) ||
            !string.Equals(offered.Currency, asking.Currency, StringComparison.Ordinal) ||
            !string.Equals(offered.Currency, availableBudget.Currency, StringComparison.Ordinal))
        {
            return Rejected(bid, "currency-mismatch");
        }

        if (offered.MinorUnits < asking.MinorUnits)
        {
            return Rejected(bid, "below-asking-price");
        }

        if (offered.MinorUnits > availableBudget.MinorUnits)
        {
            return Rejected(bid, "budget-exceeded");
        }

        return new NegotiationResolution(
            bid.BidId,
            bid.BuyerClubId,
            bid.Valuation.SubjectId,
            Accepted: true,
            bid.OfferedPrice,
            "accepted");
    }

    internal static NegotiationResolution Rejected(MarketBid bid, string reason) =>
        new(
            bid.BidId,
            bid.BuyerClubId,
            bid.Valuation.SubjectId,
            Accepted: false,
            AgreedPrice: null,
            reason);
}

internal static class NegotiationValue
{
    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
