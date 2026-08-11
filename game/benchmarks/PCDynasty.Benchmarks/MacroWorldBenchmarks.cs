using BenchmarkDotNet.Attributes;
using PCDynasty.World.Seasons;
using PCDynasty.World.State;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class MacroWorldBenchmarks
{
    public const int ClubCount = 512;

    private readonly WorldCheckpoint checkpoint;
    private readonly MacroSeasonTransition transition;

    public MacroWorldBenchmarks()
    {
        ClubSeasonState[] currentStates = CreateStates("season:2025", pointsOffset: 0);
        ClubSeasonState[] nextStates = CreateStates("season:2026", pointsOffset: 7);
        checkpoint = new WorldCheckpoint(
            schemaVersion: 1,
            sequence: 10_000,
            new WorldDate(2026, 6, 30),
            currentStates);
        transition = new MacroSeasonTransition(
            worldTick: 10_001,
            new WorldDate(2026, 7, 1),
            nextStates);
    }

    [Benchmark]
    public long AdvanceMacroSeason()
    {
        MacroSeasonResult result = MacroSeasonScheduler.Advance(checkpoint, transition);
        return result.Checkpoint.Sequence + result.Facts.Length;
    }

    private static ClubSeasonState[] CreateStates(string seasonId, int pointsOffset)
    {
        return Enumerable.Range(0, ClubCount)
            .Select(index => new ClubSeasonState(
                $"benchmark:club:{index:D4}",
                seasonId,
                $"benchmark:competition:{index % 16:D2}",
                new Dictionary<string, long>
                {
                    ["played"] = 38,
                    ["points"] = (index + pointsOffset) % 100,
                    ["reputation"] = 400 + ((index * 31) % 500),
                }))
            .ToArray();
    }
}
