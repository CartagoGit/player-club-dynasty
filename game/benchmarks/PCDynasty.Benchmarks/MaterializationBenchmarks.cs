using BenchmarkDotNet.Attributes;
using System.Globalization;
using PCDynasty.World.Commitments;
using PCDynasty.World.Materialization;
using PCDynasty.World.State;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class MaterializationBenchmarks
{
    private const string SelectedClubId = "benchmark:club:0042";

    private readonly WorldCheckpoint checkpoint = CreateCheckpoint();
    private readonly MaterializationLedger ledger = CreateLedger();
    private readonly FidelityPromotionBoundary boundary = new(
        SelectedClubId,
        SimulationFidelity.Macro,
        SimulationFidelity.Detailed,
        atTick: 1_000);

    [Benchmark]
    public long MaterializeClub()
    {
        MaterializationResult result = EntityMaterializer.Materialize(
            checkpoint,
            ledger,
            boundary);
        return result.SourceStates.Length +
               result.ConfirmedCommitments.Length +
               result.Ledger.Ownership.Length;
    }

    private static WorldCheckpoint CreateCheckpoint()
    {
        return new WorldCheckpoint(
            schemaVersion: 1,
            sequence: 1_000,
            new WorldDate(2026, 7, 1),
            Enumerable.Range(0, 512).Select(index => new ClubSeasonState(
                $"benchmark:club:{index:D4}",
                "benchmark:season:2026",
                $"benchmark:competition:{index % 16:D2}",
                [KeyValuePair.Create("rating", 400L + index)])));
    }

    private static MaterializationLedger CreateLedger()
    {
        HistoricalCommitment[] commitments = Enumerable.Range(0, 1_000)
            .Select(index => new HistoricalCommitment(
                $"benchmark:commitment:{index:D4}",
                index % 33 == 0
                    ? SelectedClubId
                    : $"benchmark:club:{index % 512:D4}",
                $"benchmark:outcome:{index:D4}",
                committedAtTick: index,
                effectiveAtTick: index,
                SimulationFidelity.Macro,
                [KeyValuePair.Create(
                    "value",
                    index.ToString(CultureInfo.InvariantCulture))]))
            .ToArray();
        return new MaterializationLedger(
            [new FidelityOwnership(SelectedClubId, SimulationFidelity.Macro, 0, null)],
            commitments,
            []);
    }
}
