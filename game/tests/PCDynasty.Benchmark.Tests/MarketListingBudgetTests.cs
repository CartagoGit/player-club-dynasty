using System.Diagnostics;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Benchmarks;
using PCDynasty.Contracts.Careers;
using Xunit;
using Xunit.Abstractions;

namespace PCDynasty.Benchmarks.Tests;

/// <summary>
/// The market is the screen the player reported as slow, and it is the one
/// that asks the world for the most: every player of every club it does not
/// own, valued and sorted. Opening it re-ran that walk each time.
///
/// The first listing is allowed to be expensive — it is a full scan of the
/// world and there is no honest way to make it free. What must not happen is
/// paying it again on every render, so the budget here is on the *second*
/// call, which is what navigating back to the market actually costs.
/// </summary>
public sealed class MarketListingBudgetTests(ITestOutputHelper output, TargetScaleWorld world)
    : IClassFixture<TargetScaleWorld>
{
    private static readonly TimeSpan RepeatBudget = TimeSpan.FromMilliseconds(16);

    private readonly TargetScaleWorld _world = world;

    [Fact]
    public void ReopeningTheMarketDoesNotRewalkTheWorld()
    {
        CareerTransferMarketService market = CreateMarket();
        var managed = new ManagedClubContext(WorldLoadBenchmarks.SelectedClubId);

        long coldStarted = Stopwatch.GetTimestamp();
        int firstCount = market.ListCandidates(managed).Count;
        TimeSpan cold = Elapsed(coldStarted);

        TimeSpan warm = MeasureMedian(() => market.ListCandidates(managed).Count);

        output.WriteLine(
            $"market listing: first {cold.TotalMilliseconds:F1} ms, reopen {warm.TotalMilliseconds:F3} ms " +
            $"({firstCount} candidates, budget {RepeatBudget.TotalMilliseconds} ms)");

        Assert.True(firstCount > 0, "The target-scale world should offer candidates.");
        Assert.True(
            warm <= RepeatBudget,
            $"Reopening the market took {warm.TotalMilliseconds:F3} ms; budget is {RepeatBudget.TotalMilliseconds} ms.");
    }

    [Fact]
    public void TheSquadProjectionIsCachedAlongsideTheCandidates()
    {
        CareerTransferMarketService market = CreateMarket();
        var managed = new ManagedClubContext(WorldLoadBenchmarks.SelectedClubId);

        market.ListCandidates(managed);
        TimeSpan warm = MeasureMedian(() => market.ListManagedSquad(managed).Count);

        output.WriteLine($"managed squad: {warm.TotalMilliseconds:F3} ms (budget {RepeatBudget.TotalMilliseconds} ms)");
        Assert.True(
            warm <= RepeatBudget,
            $"Listing the managed squad took {warm.TotalMilliseconds:F3} ms; budget is {RepeatBudget.TotalMilliseconds} ms.");
    }

    [Fact]
    public void ListingTheSameWorldTwiceReturnsTheSameProjection()
    {
        // The projections are pure functions of an immutable snapshot, so a
        // second call must be the same instance, not an equal copy. Anything
        // else means the world is being re-projected behind the screen.
        IReadOnlyList<ClubRow> first = _world.Service.ListClubs();
        IReadOnlyList<ClubRow> second = _world.Service.ListClubs();
        Assert.Same(first, second);

        IReadOnlyList<PlayerRow> squadFirst = _world.Service.ListPlayers(WorldLoadBenchmarks.SelectedClubId);
        IReadOnlyList<PlayerRow> squadSecond = _world.Service.ListPlayers(WorldLoadBenchmarks.SelectedClubId);
        Assert.Same(squadFirst, squadSecond);
    }

    private CareerTransferMarketService CreateMarket() =>
        new(_world.Service, new NoTransfers(), new NoFinances(), TimeProvider.System);

    private static TimeSpan Elapsed(long started) =>
        TimeSpan.FromSeconds((double)(Stopwatch.GetTimestamp() - started) / Stopwatch.Frequency);

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

    /// <summary>A career that has signed nobody: the listing cost is the world's alone.</summary>
    private sealed class NoTransfers : ICareerTransferStore
    {
        public IReadOnlyList<CareerTransferRecord> ListForClub(string clubExternalId) => [];

        public bool TrySettle(
            CareerTransferRecord transfer,
            CareerFinanceState expected,
            CareerFinanceState updated) => false;

        public void Clear()
        {
        }
    }

    private sealed class NoFinances : ICareerFinanceStore
    {
        public bool TryLoad(string clubExternalId, out CareerFinanceState? state)
        {
            state = null;
            return false;
        }

        public void Save(CareerFinanceState state)
        {
        }

        public void Clear()
        {
        }
    }
}
