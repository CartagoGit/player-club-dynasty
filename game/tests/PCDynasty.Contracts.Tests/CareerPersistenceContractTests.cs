using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class CareerPersistenceContractTests
{
    [Fact]
    public void MatchContractRejectsInvalidIdentityAndScores()
    {
        Assert.Throws<ArgumentException>(() => Match(homeClubId: " "));
        Assert.Throws<ArgumentException>(() => Match(awayClubId: "club:home"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Match(homeGoals: -1));
    }

    [Fact]
    public void ProfileContractNormalizesUtcAndRejectsReversedTimeline()
    {
        var started = new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.FromHours(2));
        var profile = new CareerProfile(" club:test ", " Test Club ", started, started.AddHours(1));

        Assert.Equal("club:test", profile.ClubExternalId);
        Assert.Equal("Test Club", profile.ClubDisplayName);
        Assert.Equal(TimeSpan.Zero, profile.StartedAtUtc.Offset);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CareerProfile("club:test", "Test", started, started.AddMinutes(-1)));
    }

    [Fact]
    public void PersistencePortsAreOwnedByTheContractsAssembly()
    {
        Assert.Equal(typeof(CareerProfile).Assembly, typeof(ICareerProfileStore).Assembly);
        Assert.Equal(typeof(HistoryEntry).Assembly, typeof(ICareerHistoryStore).Assembly);
        Assert.Equal(typeof(SeasonHistoryEntry).Assembly, typeof(StandingRow).Assembly);
    }

    [Fact]
    public void SeasonContractsRejectImpossibleStandings()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Standing(points: -1));
        Assert.Throws<ArgumentException>(() => Standing(played: 2, wins: 1, draws: 0, losses: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeasonHistoryEntry(
            CompetitionDisplayName: "Test Cup",
            ManagedClubId: "club:test",
            ManagedClubDisplayName: "Test Club",
            ManagedPosition: 0,
            ManagedPoints: 0,
            ManagedGoalsFor: 0,
            ManagedGoalsAgainst: 0,
            TotalClubs: 0,
            TotalFixtures: 0,
            PlayedAtUtc: DateTime.UtcNow,
            FinalStandings: []));
    }

    private static HistoryEntry Match(
        string homeClubId = "club:home",
        string awayClubId = "club:away",
        int homeGoals = 1) =>
        new(
            CompetitionDisplayName: "Test Cup",
            HomeClubId: homeClubId,
            AwayClubId: awayClubId,
            HomeDisplayName: "Home",
            AwayDisplayName: "Away",
            HomeGoals: homeGoals,
            AwayGoals: 0,
            Seed: 1,
            PlayedAtUtc: DateTime.UtcNow);

    private static StandingRow Standing(
        int points = 1,
        int played = 1,
        int wins = 0,
        int draws = 1,
        int losses = 0) =>
        new(
            ClubExternalId: "club:test",
            DisplayName: "Test Club",
            ShortName: "Test",
            Points: points,
            Played: played,
            Wins: wins,
            Draws: draws,
            Losses: losses,
            GoalsFor: 1,
            GoalsAgainst: 1);
}
