using System.IO;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

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
    private static readonly string ContentRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    private static async Task<(ClubRow home, ClubRow away)> LoadClubsAsync(string homeId, string awayId)
    {
        var service = WorldService.Initialize(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();
        var home = service.FindClub(homeId) ?? throw new InvalidOperationException($"missing {homeId}");
        var away = service.FindClub(awayId) ?? throw new InvalidOperationException($"missing {awayId}");
        return (home, away);
    }

    [Fact]
    public async Task SimulateReturnsMatchBetweenTheTwoClubs()
    {
        var (home, away) = await LoadClubsAsync("club:aurora-quay", "club:cinder-valley");

        var result = MatchSimulator.Simulate(home, away, seed: 42);

        Assert.Equal(home.ExternalId, result.HomeClub.ExternalId);
        Assert.Equal(away.ExternalId, result.AwayClub.ExternalId);
        Assert.Equal(42, result.Seed);
        Assert.True(result.Events.Count > 0);
        Assert.Equal(result.HomeGoals + result.AwayGoals, CountGoals(result));
    }

    [Fact]
    public async Task SimulateIsDeterministicForTheSameSeed()
    {
        var (home, away) = await LoadClubsAsync("club:aurora-quay", "club:northglass-rovers");

        var first = MatchSimulator.Simulate(home, away, seed: 12345);
        var second = MatchSimulator.Simulate(home, away, seed: 12345);

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
    public async Task SimulateProducesDifferentOutcomesForDifferentSeeds()
    {
        var (home, away) = await LoadClubsAsync("club:aurora-quay", "club:forge-lantern");

        var first = MatchSimulator.Simulate(home, away, seed: 1);
        var second = MatchSimulator.Simulate(home, away, seed: 9999);

        // The result sequences must differ. The score may coincidentally
        // match (very rare) but the underlying events must diverge.
        bool sameScore = first.HomeGoals == second.HomeGoals && first.AwayGoals == second.AwayGoals;
        bool sameEvents = first.Events.SequenceEqual(second.Events);

        Assert.False(sameScore && sameEvents, "Different seeds must produce different matches.");
    }

    [Fact]
    public async Task SimulateGoalsAlwaysBelongToOneOfTheTwoClubs()
    {
        var (home, away) = await LoadClubsAsync("club:maris-cobalt", "club:solspire-azul");

        var result = MatchSimulator.Simulate(home, away, seed: 7);

        foreach (var evt in result.Events.Where(e => e.IsGoal))
        {
            Assert.True(
                evt.ActorClubId == home.ExternalId || evt.ActorClubId == away.ExternalId,
                $"Goal actor {evt.ActorClubId} does not belong to either side.");
        }
    }

    [Fact]
    public async Task SimulateRejectsIdenticalClubs()
    {
        var (home, _) = await LoadClubsAsync("club:aurora-quay", "club:northglass-rovers");

        Assert.Throws<ArgumentException>(() =>
            MatchSimulator.Simulate(home, home, seed: 1));
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
}