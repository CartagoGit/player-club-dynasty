using System.Collections.Immutable;

namespace PCDynasty.Contracts.Careers;

public sealed record SeasonHistoryEntry
{
    public SeasonHistoryEntry(
        string CompetitionDisplayName,
        string ManagedClubId,
        string ManagedClubDisplayName,
        int ManagedPosition,
        int ManagedPoints,
        int ManagedGoalsFor,
        int ManagedGoalsAgainst,
        int TotalClubs,
        int TotalFixtures,
        DateTime PlayedAtUtc,
        ImmutableArray<StandingRow> FinalStandings)
    {
        this.CompetitionDisplayName = CareerContractValue.Required(
            CompetitionDisplayName,
            nameof(CompetitionDisplayName));
        this.ManagedClubId = CareerContractValue.Required(ManagedClubId, nameof(ManagedClubId));
        this.ManagedClubDisplayName = CareerContractValue.Required(
            ManagedClubDisplayName,
            nameof(ManagedClubDisplayName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ManagedPosition);
        ArgumentOutOfRangeException.ThrowIfNegative(ManagedPoints);
        ArgumentOutOfRangeException.ThrowIfNegative(ManagedGoalsFor);
        ArgumentOutOfRangeException.ThrowIfNegative(ManagedGoalsAgainst);
        ArgumentOutOfRangeException.ThrowIfNegative(TotalClubs);
        ArgumentOutOfRangeException.ThrowIfNegative(TotalFixtures);
        if (FinalStandings.IsDefault)
        {
            throw new ArgumentException("Final standings must be initialized.", nameof(FinalStandings));
        }

        this.ManagedPosition = ManagedPosition;
        this.ManagedPoints = ManagedPoints;
        this.ManagedGoalsFor = ManagedGoalsFor;
        this.ManagedGoalsAgainst = ManagedGoalsAgainst;
        this.TotalClubs = TotalClubs;
        this.TotalFixtures = TotalFixtures;
        this.PlayedAtUtc = PlayedAtUtc.ToUniversalTime();
        this.FinalStandings = FinalStandings;
    }

    public string CompetitionDisplayName { get; }
    public string ManagedClubId { get; }
    public string ManagedClubDisplayName { get; }
    public int ManagedPosition { get; }
    public int ManagedPoints { get; }
    public int ManagedGoalsFor { get; }
    public int ManagedGoalsAgainst { get; }
    public int TotalClubs { get; }
    public int TotalFixtures { get; }
    public DateTime PlayedAtUtc { get; }
    public ImmutableArray<StandingRow> FinalStandings { get; }
}

public sealed record StandingRow
{
    public StandingRow(
        string ClubExternalId,
        string DisplayName,
        string ShortName,
        int Points,
        int Played,
        int Wins,
        int Draws,
        int Losses,
        int GoalsFor,
        int GoalsAgainst)
    {
        this.ClubExternalId = CareerContractValue.Required(ClubExternalId, nameof(ClubExternalId));
        this.DisplayName = CareerContractValue.Required(DisplayName, nameof(DisplayName));
        this.ShortName = CareerContractValue.Required(ShortName, nameof(ShortName));
        ArgumentOutOfRangeException.ThrowIfNegative(Points);
        ArgumentOutOfRangeException.ThrowIfNegative(Played);
        ArgumentOutOfRangeException.ThrowIfNegative(Wins);
        ArgumentOutOfRangeException.ThrowIfNegative(Draws);
        ArgumentOutOfRangeException.ThrowIfNegative(Losses);
        ArgumentOutOfRangeException.ThrowIfNegative(GoalsFor);
        ArgumentOutOfRangeException.ThrowIfNegative(GoalsAgainst);
        if (Played != Wins + Draws + Losses)
        {
            throw new ArgumentException("Played matches must equal wins, draws and losses.", nameof(Played));
        }

        this.Points = Points;
        this.Played = Played;
        this.Wins = Wins;
        this.Draws = Draws;
        this.Losses = Losses;
        this.GoalsFor = GoalsFor;
        this.GoalsAgainst = GoalsAgainst;
    }

    public string ClubExternalId { get; }
    public string DisplayName { get; }
    public string ShortName { get; }
    public int Points { get; init; }
    public int Played { get; init; }
    public int Wins { get; init; }
    public int Draws { get; init; }
    public int Losses { get; init; }
    public int GoalsFor { get; init; }
    public int GoalsAgainst { get; init; }
    public int GoalDifference => GoalsFor - GoalsAgainst;
}
