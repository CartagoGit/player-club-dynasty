using PCDynasty.World.Commitments;
using PCDynasty.World.Materialization;
using PCDynasty.World.State;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class EntityMaterializerTests
{
    [Fact]
    public void MaterializePromotesOwnershipAndPreservesCommittedMacroHistory()
    {
        ClubSeasonState sourceState = State("club:7");
        var checkpoint = new WorldCheckpoint(
            schemaVersion: 2,
            sequence: 500,
            new WorldDate(2040, 1, 1),
            [sourceState, State("club:8")]);
        HistoricalCommitment commitment = Commitment("club:7", committedAtTick: 200);
        HistoricalCommitment otherCommitment = Commitment("club:8", committedAtTick: 220);
        var ledger = new MaterializationLedger(
            [new FidelityOwnership("club:7", SimulationFidelity.Macro, 0, null)],
            [commitment, otherCommitment],
            []);
        var boundary = new FidelityPromotionBoundary(
            "club:7",
            SimulationFidelity.Macro,
            SimulationFidelity.Detailed,
            atTick: 500);

        MaterializationResult result = EntityMaterializer.Materialize(
            checkpoint,
            ledger,
            boundary);

        Assert.Same(checkpoint, result.SourceCheckpoint);
        Assert.Same(sourceState, Assert.Single(result.SourceStates));
        Assert.Same(commitment, Assert.Single(result.ConfirmedCommitments));
        Assert.Same(commitment, result.Ledger.Commitments[0]);
        Assert.Same(otherCommitment, result.Ledger.Commitments[1]);
        Assert.Same(boundary, Assert.Single(result.Ledger.PromotionBoundaries));
        Assert.Null(Assert.Single(ledger.Ownership).UntilTickExclusive);
        Assert.Equal(
            SimulationFidelity.Detailed,
            result.Ledger.Ownership.Single(ownership =>
                ownership.SubjectId == "club:7" &&
                ownership.UntilTickExclusive is null).Fidelity);
    }

    [Fact]
    public void MaterializeRejectsMissingStateAndFutureKnowledge()
    {
        var checkpoint = new WorldCheckpoint(
            schemaVersion: 1,
            sequence: 100,
            new WorldDate(2030, 1, 1),
            [State("club:8")]);
        var ledger = new MaterializationLedger(
            [new FidelityOwnership("club:7", SimulationFidelity.Macro, 0, null)],
            [Commitment("club:7", committedAtTick: 101)],
            []);
        var boundary = new FidelityPromotionBoundary(
            "club:7",
            SimulationFidelity.Macro,
            SimulationFidelity.Detailed,
            atTick: 100);

        Assert.Throws<InvalidOperationException>(() => EntityMaterializer.Materialize(
            checkpoint,
            ledger,
            boundary));

        var checkpointWithState = new WorldCheckpoint(
            schemaVersion: 1,
            sequence: 100,
            new WorldDate(2030, 1, 1),
            [State("club:7")]);
        Assert.Throws<InvalidOperationException>(() => EntityMaterializer.Materialize(
            checkpointWithState,
            ledger,
            boundary));
    }

    private static ClubSeasonState State(string clubId) =>
        new(
            clubId,
            "season:2040",
            "competition:open",
            [KeyValuePair.Create("rating", 80L)]);

    private static HistoricalCommitment Commitment(string subjectId, long committedAtTick) =>
        new(
            $"commitment:{subjectId}",
            subjectId,
            "outcome:budget",
            committedAtTick,
            effectiveAtTick: committedAtTick + 10,
            SimulationFidelity.Macro,
            [KeyValuePair.Create("amount", "1000")]);
}
