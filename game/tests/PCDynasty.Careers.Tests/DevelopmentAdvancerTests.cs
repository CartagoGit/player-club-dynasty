using System.Diagnostics;
using PCDynasty.Careers.Development;
using Xunit;

namespace PCDynasty.Careers.Tests;

public sealed class DevelopmentAdvancerTests
{
    [Fact]
    public void AdvancesElapsedTimeDeterministicallyOnDemand()
    {
        var profile = new DevelopmentProfile(
            talentFactor: 820,
            workFactor: 760,
            opportunityFactor: 640,
            new BiologicalProfile(6_200, 9_100, 11_000, 720, 680));
        var initial = new DevelopmentState(
            asOfDay: 7_000,
            capability: 510,
            new CareerWear(asOfDay: 7_000, accumulatedExposure: 12_000, durableWear: 30));

        DevelopmentState first = DevelopmentAdvancer.Advance(
            profile,
            initial,
            targetDay: 7_365,
            exposurePerDay: 420);
        DevelopmentState replay = DevelopmentAdvancer.Advance(
            profile,
            initial,
            targetDay: 7_365,
            exposurePerDay: 420);

        Assert.Equal(Fingerprint(first), Fingerprint(replay));
        Assert.Equal(7_365, first.AsOfDay);
        Assert.True(first.Capability > initial.Capability);
        Assert.True(first.Wear.DurableWear > initial.Wear.DurableWear);
        Assert.Equal(
            12_000 + (365L * 420),
            first.Wear.AccumulatedExposure);
    }

    [Fact]
    public void HugeElapsedIntervalsUseConstantTimeAggregateCalculation()
    {
        var profile = new DevelopmentProfile(
            700,
            700,
            700,
            new BiologicalProfile(6_000, 9_000, 11_000, 800, 800));
        var initial = new DevelopmentState(
            5_000,
            400,
            new CareerWear(5_000, 0, 0));
        var stopwatch = Stopwatch.StartNew();

        DevelopmentState result = DevelopmentAdvancer.Advance(
            profile,
            initial,
            targetDay: 3_655_000,
            exposurePerDay: 100);

        stopwatch.Stop();
        Assert.Equal(3_655_000, result.AsOfDay);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DevelopmentAdvancer.Advance(
            profile,
            initial,
            targetDay: 4_999,
            exposurePerDay: 100));
    }

    private static string Fingerprint(DevelopmentState state) => string.Join(
        '|',
        state.AsOfDay,
        state.Capability,
        state.Wear.AccumulatedExposure,
        state.Wear.DurableWear);
}
