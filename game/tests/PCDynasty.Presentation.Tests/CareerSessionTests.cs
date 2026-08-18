using System.Collections.Immutable;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class CareerSessionTests
{
    [Fact]
    public void IndependentCareersDoNotShareMatchHistory()
    {
        var firstCareer = new CareerSession();
        var secondCareer = new CareerSession();

        firstCareer.Record(NewMatch(seed: 17));

        Assert.Single(firstCareer.MatchSnapshot());
        Assert.Empty(secondCareer.MatchSnapshot());
    }

    [Fact]
    public void IndependentCareersDoNotShareSeasonHistory()
    {
        var firstCareer = new CareerSession();
        var secondCareer = new CareerSession();

        firstCareer.Record(NewSeason("club:first"));

        Assert.Single(firstCareer.SeasonsFor("club:first"));
        Assert.Empty(secondCareer.SeasonsFor("club:first"));
    }

    [Fact]
    public void SessionExposesStableSnapshots()
    {
        var career = new CareerSession();
        career.Record(NewMatch(seed: 1));

        IReadOnlyList<HistoryEntry> snapshot = career.MatchSnapshot();
        career.Record(NewMatch(seed: 2));

        Assert.Single(snapshot);
        Assert.Equal(2, career.MatchSnapshot().Count);
    }

    private static HistoryEntry NewMatch(long seed) => new(
        CompetitionDisplayName: "Test Cup",
        HomeDisplayName: "Home",
        AwayDisplayName: "Away",
        HomeClubId: "club:home",
        AwayClubId: "club:away",
        HomeGoals: 1,
        AwayGoals: 0,
        Seed: seed,
        PlayedAtUtc: DateTime.UtcNow);

    private static SeasonHistoryEntry NewSeason(string clubId) => new(
        CompetitionDisplayName: "Test Cup",
        ManagedClubId: clubId,
        ManagedClubDisplayName: "Managed Club",
        ManagedPosition: 1,
        ManagedPoints: 9,
        ManagedGoalsFor: 6,
        ManagedGoalsAgainst: 1,
        TotalClubs: 4,
        TotalFixtures: 6,
        PlayedAtUtc: DateTime.UtcNow,
        FinalStandings: ImmutableArray<StandingRow>.Empty);
}
