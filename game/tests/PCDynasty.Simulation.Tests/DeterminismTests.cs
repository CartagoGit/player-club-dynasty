using PCDynasty.Simulation.Determinism;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class DeterminismTests
{
    [Fact]
    public void DerivesStableIsolatedSeedsFromCanonicalScopes()
    {
        ulong first = SeedDeriver.Derive(1_234, "season:5", "contest:9");
        ulong repeated = SeedDeriver.Derive(1_234, "season:5", "contest:9");
        ulong differentContest = SeedDeriver.Derive(1_234, "season:5", "contest:10");
        ulong differentBoundary = SeedDeriver.Derive(1_234, "season:5|contest:9");

        Assert.Equal(first, repeated);
        Assert.NotEqual(first, differentContest);
        Assert.NotEqual(first, differentBoundary);
        Assert.Equal(11_926_236_389_362_648_463UL, first);
    }

    [Fact]
    public void UsesCheckedFixedPointArithmeticWithoutFloatingPoint()
    {
        DeterministicNumber oneThird = DeterministicNumber.FromRatio(1, 3);
        DeterministicNumber oneAndHalf = DeterministicNumber.FromRatio(3, 2);

        Assert.Equal(333_333, oneThird.RawValue);
        Assert.Equal(999_999, (oneThird * DeterministicNumber.FromInt32(3)).RawValue);
        Assert.Equal(3_000_000, (oneAndHalf * DeterministicNumber.FromInt32(2)).RawValue);
        Assert.Equal(750_000, (oneAndHalf / DeterministicNumber.FromInt32(2)).RawValue);
        Assert.Equal(
            DeterministicNumber.FromInt32(2),
            oneAndHalf + DeterministicNumber.FromRatio(1, 2));
    }

    [Fact]
    public void ProducesTheSameNumbersInParallel()
    {
        long[] results = Enumerable.Range(0, 1_000)
            .AsParallel()
            .Select(index =>
                (DeterministicNumber.FromRatio(index, 7) *
                 DeterministicNumber.FromRatio(11, 13)).RawValue)
            .ToArray();
        long[] repeated = Enumerable.Range(0, 1_000)
            .AsParallel()
            .Select(index =>
                (DeterministicNumber.FromRatio(index, 7) *
                 DeterministicNumber.FromRatio(11, 13)).RawValue)
            .ToArray();

        Assert.Equal(results, repeated);
    }
}
