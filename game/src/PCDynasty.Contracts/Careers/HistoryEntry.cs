namespace PCDynasty.Contracts.Careers;

public sealed record HistoryEntry
{
    public HistoryEntry(
        string CompetitionDisplayName,
        string HomeClubId,
        string AwayClubId,
        string HomeDisplayName,
        string AwayDisplayName,
        int HomeGoals,
        int AwayGoals,
        long Seed,
        DateTime PlayedAtUtc)
    {
        this.CompetitionDisplayName = CareerContractValue.Required(CompetitionDisplayName, nameof(CompetitionDisplayName));
        this.HomeClubId = CareerContractValue.Required(HomeClubId, nameof(HomeClubId));
        this.AwayClubId = CareerContractValue.Required(AwayClubId, nameof(AwayClubId));
        if (string.Equals(this.HomeClubId, this.AwayClubId, StringComparison.Ordinal))
        {
            throw new ArgumentException("A match requires two distinct clubs.", nameof(AwayClubId));
        }

        this.HomeDisplayName = CareerContractValue.Required(HomeDisplayName, nameof(HomeDisplayName));
        this.AwayDisplayName = CareerContractValue.Required(AwayDisplayName, nameof(AwayDisplayName));
        ArgumentOutOfRangeException.ThrowIfNegative(HomeGoals);
        ArgumentOutOfRangeException.ThrowIfNegative(AwayGoals);
        this.HomeGoals = HomeGoals;
        this.AwayGoals = AwayGoals;
        this.Seed = Seed;
        this.PlayedAtUtc = PlayedAtUtc.ToUniversalTime();
    }

    public string CompetitionDisplayName { get; }
    public string HomeClubId { get; }
    public string AwayClubId { get; }
    public string HomeDisplayName { get; }
    public string AwayDisplayName { get; }
    public int HomeGoals { get; }
    public int AwayGoals { get; }
    public long Seed { get; }
    public DateTime PlayedAtUtc { get; }
}
