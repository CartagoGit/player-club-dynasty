using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class ParallelReplayTests
{
    [Fact]
    public void SerialAndParallelRunsProduceIdenticalCanonicalFacts()
    {
        GoldenReplayFixture fixture = GoldenReplayFixture.Load();
        string expected = string.Join('\n', CanonicalReplay.Execute(fixture));
        string[] serial = Enumerable.Range(0, 256)
            .Select(_ => string.Join('\n', CanonicalReplay.Execute(fixture)))
            .ToArray();
        string[] parallel = Enumerable.Range(0, 256)
            .AsParallel()
            .Select(_ => string.Join('\n', CanonicalReplay.Execute(fixture)))
            .ToArray();

        Assert.All(serial, replay => Assert.Equal(expected, replay));
        Assert.All(parallel, replay => Assert.Equal(expected, replay));
    }
}
