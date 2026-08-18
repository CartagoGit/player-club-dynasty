namespace PCDynasty.Contracts.Careers;

public sealed record CareerFinanceState
{
    public CareerFinanceState(
        string clubExternalId,
        int seasonYear,
        string currency,
        long cash,
        long revenue,
        long costs,
        long debt,
        long transferBudget,
        long wageBudget,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seasonYear);
        ArgumentOutOfRangeException.ThrowIfNegative(revenue);
        ArgumentOutOfRangeException.ThrowIfNegative(costs);
        ArgumentOutOfRangeException.ThrowIfNegative(debt);
        ArgumentOutOfRangeException.ThrowIfNegative(transferBudget);
        ArgumentOutOfRangeException.ThrowIfNegative(wageBudget);
        ClubExternalId = CareerContractValue.Required(clubExternalId, nameof(clubExternalId));
        Currency = CareerContractValue.Required(currency, nameof(currency)).ToUpperInvariant();
        if (Currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currency));
        }

        SeasonYear = seasonYear;
        Cash = cash;
        Revenue = revenue;
        Costs = costs;
        Debt = debt;
        TransferBudget = transferBudget;
        WageBudget = wageBudget;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public string ClubExternalId { get; }
    public int SeasonYear { get; }
    public string Currency { get; }
    public long Cash { get; }
    public long Revenue { get; }
    public long Costs { get; }
    public long Debt { get; }
    public long TransferBudget { get; }
    public long WageBudget { get; }
    public DateTimeOffset UpdatedAtUtc { get; }
}
