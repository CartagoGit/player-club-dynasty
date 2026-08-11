using PCDynasty.Core.Performance;
using Xunit;

namespace PCDynasty.Core.Tests.Performance;

public sealed class CurrentPerformanceTests
{
    [Fact]
    public void DerivesCurrentScoreFromCapabilitiesAndForm()
    {
        var capabilities = new CapabilityProfile(
            technique: 80,
            physical: 60,
            mental: 40);
        var weights = new CurrentPerformanceWeights(
            technique: 4,
            form: 1,
            physical: 3,
            mental: 2);

        CurrentPerformance performance = CurrentPerformance.Derive(
            capabilities,
            form: 70,
            weights);

        Assert.Equal(65, performance.Score);
    }

    [Fact]
    public void AllowsAConsumerToIgnoreFactorsThroughWeights()
    {
        var capabilities = new CapabilityProfile(
            technique: 90,
            physical: 15,
            mental: 20);
        var techniqueOnly = new CurrentPerformanceWeights(
            technique: 1,
            form: 0,
            physical: 0,
            mental: 0);

        CurrentPerformance performance = CurrentPerformance.Derive(
            capabilities,
            form: 10,
            techniqueOnly);

        Assert.Equal(90, performance.Score);
    }

    [Fact]
    public void RejectsScoresOutsideTheCompiledRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapabilityProfile(-1, 50, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapabilityProfile(50, 101, 50));

        var capabilities = new CapabilityProfile(50, 50, 50);
        var weights = new CurrentPerformanceWeights(1, 1, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CurrentPerformance.Derive(capabilities, form: 101, weights));
    }

    [Fact]
    public void RejectsWeightsWithoutAContributingFactor()
    {
        Assert.Throws<ArgumentException>(() => new CurrentPerformanceWeights(0, 0, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CurrentPerformanceWeights(-1, 1, 1, 1));

        var capabilities = new CapabilityProfile(50, 50, 50);
        Assert.Throws<ArgumentException>(() =>
            CurrentPerformance.Derive(capabilities, form: 50, default));
    }
}
