using System.Diagnostics;
using PCDynasty.Application.Workspace;
using PCDynasty.Benchmarks;
using PCDynasty.Presentation.Core;
using Xunit;
using Xunit.Abstractions;

namespace PCDynasty.Benchmarks.Tests;

/// <summary>
/// Fast gate for the indexed world: the full BenchmarkDotNet run is too slow
/// for every pull request, but a regression from an indexed lookup back to a
/// scan — the defect audit a00001 measured — must fail immediately.
/// </summary>
public sealed class WorldQueryBudgetTests(ITestOutputHelper output, TargetScaleWorld world)
    : IClassFixture<TargetScaleWorld>
{
    private static readonly TimeSpan QueryBudget = TimeSpan.FromMilliseconds(16);

    // The fixture is shared by the class, so the 160,000-person world is built
    // once rather than once per test.
    private readonly TargetScaleWorld _world = world;

    [Fact]
    public void SquadQueryOverOneHundredSixtyThousandPeopleStaysInteractive()
    {
        TimeSpan elapsed = MeasureMedian(() => _world.Service.ListPlayers(WorldLoadBenchmarks.SelectedClubId).Count);

        output.WriteLine($"squad query: {elapsed.TotalMilliseconds:F3} ms (budget {QueryBudget.TotalMilliseconds} ms)");
        Assert.True(
            elapsed <= QueryBudget,
            $"Squad query took {elapsed.TotalMilliseconds:F3} ms; budget is {QueryBudget.TotalMilliseconds} ms.");
    }

    [Fact]
    public void ClubLookupOverSixThousandFourHundredClubsStaysInteractive()
    {
        TimeSpan elapsed = MeasureMedian(() => _world.Service.FindClub(WorldLoadBenchmarks.SelectedClubId) is null ? 0 : 1);

        output.WriteLine($"club lookup: {elapsed.TotalMilliseconds:F3} ms (budget {QueryBudget.TotalMilliseconds} ms)");
        Assert.True(
            elapsed <= QueryBudget,
            $"Club lookup took {elapsed.TotalMilliseconds:F3} ms; budget is {QueryBudget.TotalMilliseconds} ms.");
    }

    [Fact]
    public void TheTargetScaleWorldHoldsTheExpectedPopulation()
    {
        Assert.Equal(WorldLoadBenchmarks.ClubCount, _world.Service.ListClubs().Count);
        Assert.Equal(WorldLoadBenchmarks.PlayersPerClub, _world.Service.ListPlayers(WorldLoadBenchmarks.SelectedClubId).Count);
    }

    private static TimeSpan MeasureMedian(Func<int> query)
    {
        GC.KeepAlive(query());
        var samples = new long[5];
        for (int index = 0; index < samples.Length; index++)
        {
            long started = Stopwatch.GetTimestamp();
            GC.KeepAlive(query());
            samples[index] = Stopwatch.GetTimestamp() - started;
        }

        Array.Sort(samples);
        return TimeSpan.FromSeconds((double)samples[samples.Length / 2] / Stopwatch.Frequency);
    }
}

/// <summary>Builds the target-scale world once for the whole test class.</summary>
public sealed class TargetScaleWorld
{
    public WorldService Service { get; } = WorldLoadBenchmarks.CreateLoadedWorld();
}
