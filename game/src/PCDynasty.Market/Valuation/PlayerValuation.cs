namespace PCDynasty.Market.Valuation;

public readonly record struct MoneyAmount
{
    public MoneyAmount(long minorUnits, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minorUnits);
        MinorUnits = minorUnits;
        Currency = MarketValue.Required(currency, nameof(currency)).ToUpperInvariant();
    }

    public long MinorUnits { get; }
    public string Currency { get; }
}

public readonly record struct MarketPrice(MoneyAmount Amount);
public readonly record struct InternalPrice(MoneyAmount Amount);
public readonly record struct AskingPrice(MoneyAmount Amount);
public readonly record struct TransferPrice(MoneyAmount Amount);
public readonly record struct ContractPrice(MoneyAmount Amount);

public sealed class PlayerValuation
{
    public PlayerValuation(
        string subjectId,
        int valuedOnDay,
        MarketPrice market,
        InternalPrice internalPrice,
        AskingPrice asking,
        TransferPrice transfer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valuedOnDay);
        string currency = MarketValue.ValidCurrency(market.Amount, nameof(market));
        if (new[]
            {
                MarketValue.ValidCurrency(internalPrice.Amount, nameof(internalPrice)),
                MarketValue.ValidCurrency(asking.Amount, nameof(asking)),
                MarketValue.ValidCurrency(transfer.Amount, nameof(transfer)),
            }.Any(candidate => !string.Equals(candidate, currency, StringComparison.Ordinal)))
        {
            throw new ArgumentException("Every valuation dimension must use the same currency.");
        }

        SubjectId = MarketValue.Required(subjectId, nameof(subjectId));
        ValuedOnDay = valuedOnDay;
        Market = market;
        Internal = internalPrice;
        Asking = asking;
        Transfer = transfer;
    }

    public string SubjectId { get; }
    public int ValuedOnDay { get; }
    public MarketPrice Market { get; }
    public InternalPrice Internal { get; }
    public AskingPrice Asking { get; }
    public TransferPrice Transfer { get; }
}

internal static class MarketValue
{
    public static string ValidCurrency(MoneyAmount amount, string parameterName) =>
        string.IsNullOrWhiteSpace(amount.Currency)
            ? throw new ArgumentException("A valid money amount is required.", parameterName)
            : amount.Currency;

    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
