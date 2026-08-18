using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;
using Xunit;

namespace PCDynasty.Application.Tests.Competitions;

public sealed class CompetitionSimulationServiceTests
{
    private static readonly ManagedClubContext Managed = new("club:home");

    private static readonly DateTimeOffset FixedNow = new(2026, 8, 18, 12, 30, 0, TimeSpan.Zero);
    private static readonly ClubRow Home = Club("club:home", "Home FC", 72);
    private static readonly ClubRow Away = Club("club:away", "Away FC", 58);
    private static readonly ClubRow Third = Club("club:third", "Third FC", 64);

    [Fact]
    public void PlayedMatchIncludesClockedPersistenceRecordAndUniqueSeed()
    {
        var service = Service();

        MatchExecution first = service.PlayMatch(Managed, "Test Cup", Home, Away);
        MatchExecution second = service.PlayMatch(Managed, "Test Cup", Home, Away);

        Assert.Equal(FixedNow.UtcDateTime, first.History.PlayedAtUtc);
        Assert.Equal(first.Result.Seed, first.History.Seed);
        Assert.NotEqual(first.Result.Seed, second.Result.Seed);
    }

    [Fact]
    public void ReplayUsesTheOriginalSeedAndProducesTheSameTimeline()
    {
        var service = Service();
        MatchResult original = service.PlayMatch(Managed, "Test Cup", Home, Away).Result;

        MatchResult replay = service.Replay(original);

        Assert.Equal(original.HomeGoals, replay.HomeGoals);
        Assert.Equal(original.AwayGoals, replay.AwayGoals);
        Assert.Equal(original.Seed, replay.Seed);
        Assert.Equal(original.Events, replay.Events);
    }

    [Fact]
    public void PlayedSeasonProjectsEveryFixtureAndManagedStanding()
    {
        var service = Service();

        SeasonExecution execution = service.PlaySeason(
            Managed,
            "competition:test",
            "Test Cup",
            Home,
            [Home, Away, Third]);

        Assert.Equal(execution.Result.Fixtures.Length, execution.MatchHistory.Length);
        Assert.All(execution.MatchHistory, match => Assert.Equal(FixedNow.UtcDateTime, match.PlayedAtUtc));
        Assert.All(execution.MatchHistory, match => Assert.NotEqual(0, match.Seed));
        Assert.Equal(Home.ExternalId, execution.SeasonHistory.ManagedClubId);
        Assert.Equal(execution.Result.Standings, execution.SeasonHistory.FinalStandings);
        Assert.InRange(execution.SeasonHistory.ManagedPosition, 1, 3);
    }

    [Fact]
    public void MatchAfterSeasonDoesNotReuseAFixtureSeed()
    {
        var service = Service();
        SeasonExecution season = service.PlaySeason(
            Managed,
            "competition:test",
            "Test Cup",
            Home,
            [Home, Away, Third]);

        MatchExecution nextMatch = service.PlayMatch(Managed, "Test Cup", Home, Away);

        Assert.DoesNotContain(nextMatch.Result.Seed, season.MatchHistory.Select(match => match.Seed));
        Assert.True(nextMatch.Result.Seed > season.MatchHistory.Max(match => match.Seed));
    }

    [Fact]
    public async Task ParallelExecutionsReserveDisjointSeedRanges()
    {
        var service = Service();
        Task<long[]>[] operations = Enumerable.Range(0, 12)
            .Select(index => Task.Run(() => index % 2 == 0
                ? service.PlaySeason(
            Managed,
                    $"competition:{index}",
                    "Test Cup",
                    Home,
                    [Home, Away, Third]).MatchHistory.Select(match => match.Seed).ToArray()
                : [service.PlayMatch(Managed, "Test Cup", Home, Away).Result.Seed]))
            .ToArray();

        long[][] ranges = await Task.WhenAll(operations);
        long[] seeds = ranges.SelectMany(range => range).ToArray();

        Assert.Equal(seeds.Length, seeds.Distinct().Count());
    }

    private static CompetitionSimulationService Service() => new(new FixedTimeProvider(FixedNow));

    private static ClubRow Club(string id, string name, int prestige) =>
        new(id, name, name, "Test City", "TST", "1900", prestige, 50, 22);

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
