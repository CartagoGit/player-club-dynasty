namespace PCDynasty.Economy.Ledgers;

public sealed record AnnualClubFinancialState(
    string ClubId,
    int Year,
    string Currency,
    long Cash,
    long Revenue,
    long Cost,
    long Debt,
    long Budget);
