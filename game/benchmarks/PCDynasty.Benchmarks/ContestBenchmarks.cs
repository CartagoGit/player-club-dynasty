using BenchmarkDotNet.Attributes;
using PCDynasty.Core.Contests;
using PCDynasty.Simulation.Contests;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class ContestBenchmarks
{
    public const int ContestCount = 32;

    private static readonly ContestSimulationOptions Options = new(
        durationTicks: 90,
        eventCount: 256,
        kindId: "benchmark:contest-event",
        maximumMagnitude: 5);

    private readonly ContestSnapshot[] snapshots = CreateSnapshots();

    [Benchmark(OperationsPerInvoke = ContestCount)]
    public long SimulateContestBatch()
    {
        long checksum = 0;
        foreach (ContestSnapshot snapshot in snapshots)
        {
            ContestSimulationResult result = ContestSimulator.Run(snapshot, Options);
            checksum += result.Events.Length;
            checksum += result.Events[^1].Tick;
            checksum += result.Events[^1].Magnitude;
        }

        return checksum;
    }

    private static ContestSnapshot[] CreateSnapshots()
    {
        ContestParticipantSnapshot[] participants = Enumerable.Range(0, 22)
            .Select(index => new ContestParticipantSnapshot(
                $"benchmark:participant:{index:D2}",
                [
                    45 + ((index * 13) % 50),
                    40 + ((index * 17) % 55),
                    50 + ((index * 19) % 45),
                    35 + ((index * 23) % 60),
                ],
                form: 40 + ((index * 29) % 55)))
            .ToArray();

        return Enumerable.Range(0, ContestCount)
            .Select(index => new ContestSnapshot(
                $"benchmark:contest:{index:D2}",
                "benchmark:rules:v1",
                BenchmarkConfig.ReproducibleSeed + index,
                participants))
            .ToArray();
    }
}
