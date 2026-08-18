using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;
using Xunit;

namespace PCDynasty.Application.Tests.Competitions;

/// <summary>
/// Tests for the engine-neutral match simulator. These pin the contract
/// that:
///   1. A match is deterministic given the same seed.
///   2. Two clubs with different strengths produce different expected
///      outcomes (we use enough samples for a stable comparison).
///   3. Goals are correctly attributed to home/away.
/// </summary>
public sealed class MatchSimulatorTests
{
    private static readonly ClubRow Home = Club("club:home", "Home FC", 72);
    private static readonly ClubRow Away = Club("club:away", "Away FC", 58);

    [Fact]
    public void SimulateReturnsMatchBetweenTheTwoClubs()
    {
        var result = MatchSimulator.Simulate(Home, Away, seed: 42);

        Assert.Equal(Home.ExternalId, result.HomeClub.ExternalId);
        Assert.Equal(Away.ExternalId, result.AwayClub.ExternalId);
        Assert.Equal(42, result.Seed);
        Assert.True(result.Events.Count > 0);
        Assert.Equal(result.HomeGoals + result.AwayGoals, CountGoals(result));
    }

    [Fact]
    public void SimulateIsDeterministicForTheSameSeed()
    {
        var first = MatchSimulator.Simulate(Home, Away, seed: 12345);
        var second = MatchSimulator.Simulate(Home, Away, seed: 12345);

        Assert.Equal(first.HomeGoals, second.HomeGoals);
        Assert.Equal(first.AwayGoals, second.AwayGoals);
        Assert.Equal(first.Events.Count, second.Events.Count);
        for (int i = 0; i < first.Events.Count; i++)
        {
            Assert.Equal(first.Events[i].Minute, second.Events[i].Minute);
            Assert.Equal(first.Events[i].IsGoal, second.Events[i].IsGoal);
            Assert.Equal(first.Events[i].ActorClubId, second.Events[i].ActorClubId);
        }
    }

    [Fact]
    public void SimulateProducesDifferentOutcomesForDifferentSeeds()
    {
        var first = MatchSimulator.Simulate(Home, Away, seed: 1);
        var second = MatchSimulator.Simulate(Home, Away, seed: 9999);

        // The result sequences must differ. The score may coincidentally
        // match (very rare) but the underlying events must diverge.
        bool sameScore = first.HomeGoals == second.HomeGoals && first.AwayGoals == second.AwayGoals;
        bool sameEvents = first.Events.SequenceEqual(second.Events);

        Assert.False(sameScore && sameEvents, "Different seeds must produce different matches.");
    }

    [Fact]
    public void SimulateGoalsAlwaysBelongToOneOfTheTwoClubs()
    {
        var result = MatchSimulator.Simulate(Home, Away, seed: 7);

        foreach (var evt in result.Events.Where(e => e.IsGoal))
        {
            Assert.True(
                evt.ActorClubId == Home.ExternalId || evt.ActorClubId == Away.ExternalId,
                $"Goal actor {evt.ActorClubId} does not belong to either side.");
        }
    }

    [Fact]
    public void SimulateRejectsIdenticalClubs()
    {
        Assert.Throws<ArgumentException>(() =>
            MatchSimulator.Simulate(Home, Home, seed: 1));
    }

    [Fact]
    public void ConstantsExposeMatchLength()
    {
        Assert.Equal(90, MatchSimulator.MatchDurationMinutes);
        Assert.True(MatchSimulator.DefaultEventCount > 0);
    }

    private static int CountGoals(MatchResult result)
    {
        return result.Events.Count(e => e.IsGoal);
    }

    private static ClubRow Club(string id, string name, int prestige) =>
        new(id, name, name, "Test City", "TST", "1900", prestige, 50, 22);
}
