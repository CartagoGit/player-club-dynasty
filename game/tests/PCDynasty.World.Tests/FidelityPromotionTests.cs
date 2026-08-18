using PCDynasty.World.Commitments;
using PCDynasty.World.Materialization;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class FidelityPromotionTests
{
    [Fact]
    public void PromotesOwnershipWithoutMutatingConfirmedHistory()
    {
        HistoricalCommitment commitment = Commitment("club:7");
        var original = new MaterializationLedger(
            [new FidelityOwnership("contest:9", SimulationFidelity.Macro, 0, null)],
            [commitment],
            []);
        var boundary = new FidelityPromotionBoundary(
            "contest:9",
            SimulationFidelity.Macro,
            SimulationFidelity.Detailed,
            10_000);

        MaterializationLedger promoted = FidelityPromotion.Apply(original, boundary);

        FidelityOwnership originalOwnership = Assert.Single(original.Ownership);
        Assert.Null(originalOwnership.UntilTickExclusive);
        Assert.Collection(
            promoted.Ownership,
            ownership =>
            {
                Assert.Equal(SimulationFidelity.Macro, ownership.Fidelity);
                Assert.Equal(10_000, ownership.UntilTickExclusive);
            },
            ownership =>
            {
                Assert.Equal(SimulationFidelity.Detailed, ownership.Fidelity);
                Assert.Equal(10_000, ownership.FromTickInclusive);
                Assert.Null(ownership.UntilTickExclusive);
            });
        Assert.Same(commitment, Assert.Single(promoted.Commitments));
        Assert.Same(boundary, Assert.Single(promoted.PromotionBoundaries));
    }

    [Fact]
    public void RejectsAttemptsToRewriteAConfirmedCommitment()
    {
        HistoricalCommitment confirmed = Commitment("club:7");
        var ledger = new MaterializationLedger(
            [new FidelityOwnership("contest:9", SimulationFidelity.Macro, 0, null)],
            [confirmed],
            []);
        HistoricalCommitment rewritten = Commitment("club:8");

        Assert.Throws<InvalidOperationException>(() => ledger.RecordCommitments([rewritten]));
        MaterializationLedger retried = ledger.RecordCommitments([Commitment("club:7")]);
        Assert.Same(confirmed, Assert.Single(retried.Commitments));
    }

    [Fact]
    public void RejectsPromotionOutsideCurrentOwnership()
    {
        var ledger = new MaterializationLedger(
            [new FidelityOwnership("contest:9", SimulationFidelity.Macro, 0, 500)],
            [],
            []);
        var boundary = new FidelityPromotionBoundary(
            "contest:9",
            SimulationFidelity.Macro,
            SimulationFidelity.Detailed,
            1_000);

        Assert.Throws<InvalidOperationException>(() => FidelityPromotion.Apply(ledger, boundary));
    }

    private static HistoricalCommitment Commitment(string winner) =>
        new(
            "commitment:42",
            "contest:9",
            "outcome:completed",
            committedAtTick: 100,
            effectiveAtTick: 120,
            SimulationFidelity.Macro,
            new Dictionary<string, string> { ["winner"] = winner });
}
