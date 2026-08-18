namespace PCDynasty.Application.Finances;

public sealed record ClubFinanceOverview(
    string ClubExternalId,
    int SeasonYear,
    string Currency,
    long Cash,
    long Revenue,
    long Costs,
    long OperatingResult,
    long Debt,
    long TransferBudget,
    long WageBudget,
    DateTimeOffset UpdatedAtUtc);
