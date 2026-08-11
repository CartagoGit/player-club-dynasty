using BenchmarkDotNet.Attributes;
using PCDynasty.Careers.Development;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public sealed class DevelopmentBenchmarks
{
    public const int CareerCount = 50_000;

    private readonly DevelopmentProfile profile = new(
        talentFactor: 820,
        workFactor: 760,
        opportunityFactor: 640,
        new BiologicalProfile(
            maturationDay: 6_200,
            peakDay: 9_100,
            declineDay: 11_000,
            resilience: 720,
            recoveryCapacity: 680));

    private readonly DevelopmentState initialState = new(
        asOfDay: 7_000,
        capability: 510,
        new CareerWear(
            asOfDay: 7_000,
            accumulatedExposure: 12_000,
            durableWear: 30));

    [Benchmark(OperationsPerInvoke = CareerCount)]
    public long AdvanceDevelopmentBatch()
    {
        long checksum = 0;
        for (int index = 0; index < CareerCount; index++)
        {
            DevelopmentState advanced = DevelopmentAdvancer.Advance(
                profile,
                initialState,
                targetDay: 7_001 + (index % 365),
                exposurePerDay: 300 + (index % 121));
            checksum += advanced.Capability;
            checksum += advanced.Wear.DurableWear;
        }

        return checksum;
    }
}
