using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class MatchStatsTests
{
    [Fact]
    public void ComputeReturnsRowWithBothGoals()
    {
        var result = BuildResult(homeGoals: 2, awayGoals: 1);

        var stats = MatchStats.Compute(result);

        Assert.Equal(2, stats.HomeGoals);
        Assert.Equal(1, stats.AwayGoals);
    }

    [Fact]
    public void ComputeSplitsEventsBetweenHomeAndAway()
    {
        var result = BuildResult(
            homeGoals: 1,
            awayGoals: 1,
            extraEvents:
            [
                BuildEvent(isHome: true, kind: MatchEventKind.Shot),
                BuildEvent(isHome: false, kind: MatchEventKind.Shot),
                BuildEvent(isHome: true, kind: MatchEventKind.Save),
                BuildEvent(isHome: false, kind: MatchEventKind.Miss),
                BuildEvent(isHome: true, kind: MatchEventKind.Card),
            ]);

        var stats = MatchStats.Compute(result);

        Assert.Equal(2, stats.HomeShots); // 1 goal + 1 shot on target
        Assert.Equal(1, stats.HomeSaves);
        Assert.Equal(1, stats.HomeCards);
        Assert.Equal(2, stats.AwayShots); // 1 goal + 1 shot
        Assert.Equal(1, stats.AwayMisses);
    }

    [Fact]
    public void PossessionSumsToOneHundred()
    {
        var result = BuildResult(
            homeGoals: 0,
            awayGoals: 0,
            extraEvents:
            [
                BuildEvent(isHome: true, kind: MatchEventKind.Shot),
                BuildEvent(isHome: true, kind: MatchEventKind.Shot),
                BuildEvent(isHome: true, kind: MatchEventKind.Miss),
                BuildEvent(isHome: false, kind: MatchEventKind.Shot),
            ]);

        var stats = MatchStats.Compute(result);

        Assert.Equal(100, stats.HomePossession + stats.AwayPossession);
    }

    [Fact]
    public void EmptyMatchHasBalancedPossession()
    {
        var result = BuildResult(homeGoals: 0, awayGoals: 0);

        var stats = MatchStats.Compute(result);

        Assert.Equal(50, stats.HomePossession);
        Assert.Equal(50, stats.AwayPossession);
    }

    [Fact]
    public void ComputeRejectsNullResult()
    {
        Assert.Throws<ArgumentNullException>(() => MatchStats.Compute(null!));
    }

    private static MatchResult BuildResult(
        int homeGoals,
        int awayGoals,
        IReadOnlyList<MatchEvent>? extraEvents = null)
    {
        var home = new ClubRow("club:home", "Home FC", "Home", "Hometon", "AAA", "1900", 60, 50, 22);
        var away = new ClubRow("club:away", "Away FC", "Away", "Awayville", "BBB", "1900", 60, 50, 22);

        var events = new List<MatchEvent>();
        for (int i = 0; i < homeGoals; i++)
        {
            events.Add(BuildEvent(isHome: true, kind: MatchEventKind.Goal));
        }
        for (int i = 0; i < awayGoals; i++)
        {
            events.Add(BuildEvent(isHome: false, kind: MatchEventKind.Goal));
        }
        if (extraEvents is not null)
        {
            events.AddRange(extraEvents);
        }

        return new MatchResult(home, away, homeGoals, awayGoals, 1L, events);
    }

    private static MatchEvent BuildEvent(bool isHome, MatchEventKind kind) =>
        new(
            Sequence: 1,
            Minute: 30,
            ActorClubId: isHome ? "club:home" : "club:away",
            ActorShortName: isHome ? "Home" : "Away",
            IsHomeActor: isHome,
            IsGoal: kind == MatchEventKind.Goal,
            Magnitude: kind switch
            {
                MatchEventKind.Goal => 90,
                MatchEventKind.Save => 80,
                MatchEventKind.Shot => 60,
                MatchEventKind.Card => 40,
                _ => 10,
            },
            Description: "test");
}
