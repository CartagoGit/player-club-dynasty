using BenchmarkDotNet.Attributes;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class ManagerInteractionBenchmarks
{
    private const int ClubCount = 20;
    private static readonly ClubRow[] Clubs = CreateClubs();
    private readonly CompetitionSimulationService simulation = new(
        new FixedTimeProvider(new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero)));

    [Benchmark]
    public long PlayManagerMatch()
    {
        MatchExecution execution = simulation.PlayMatch("Benchmark League", Clubs[0], Clubs[1]);
        return execution.Result.Seed
            + execution.Result.HomeGoals
            + execution.Result.AwayGoals
            + execution.Result.Events.Count;
    }

    [Benchmark]
    public long SimulateManagerSeason()
    {
        SeasonExecution execution = simulation.PlaySeason(
            "competition:benchmark-league",
            "Benchmark League",
            Clubs[0],
            Clubs);
        return execution.Result.Fixtures.Length
            + execution.SeasonHistory.ManagedPoints
            + execution.MatchHistory.Sum(match => match.HomeGoals + match.AwayGoals);
    }

    private static ClubRow[] CreateClubs() => Enumerable
        .Range(0, ClubCount)
        .Select(index => new ClubRow(
            $"club:benchmark:{index:D2}",
            $"Benchmark Club {index:D2}",
            $"Club {index:D2}",
            $"City {index:D2}",
            "BCH",
            (1900 + index).ToString(System.Globalization.CultureInfo.InvariantCulture),
            Prestige: 45 + ((index * 7) % 50),
            Fanbase: 40 + ((index * 11) % 55),
            SquadSize: 22))
        .ToArray();

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
