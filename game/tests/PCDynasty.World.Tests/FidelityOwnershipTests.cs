using PCDynasty.World.Commitments;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class FidelityOwnershipTests
{
    [Fact]
    public void RecordsImmutableConfirmedOutcomes()
    {
        var values = new Dictionary<string, string>
        {
            ["winner"] = "club:7",
            ["score"] = "2-1",
        };
        var commitment = new HistoricalCommitment(
            "commitment:42",
            "contest:9",
            "outcome:completed",
            committedAtTick: 10_200,
            effectiveAtTick: 10_240,
            SimulationFidelity.Macro,
            values);
        values["winner"] = "club:8";

        Assert.Equal("commitment:42", commitment.CommitmentId);
        Assert.Equal("contest:9", commitment.SubjectId);
        Assert.Equal("outcome:completed", commitment.OutcomeId);
        Assert.Equal(10_200, commitment.CommittedAtTick);
        Assert.Equal(10_240, commitment.EffectiveAtTick);
        Assert.Equal(SimulationFidelity.Macro, commitment.OriginFidelity);
        Assert.Equal("club:7", commitment.Values["winner"]);
    }

    [Fact]
    public void DefinesHalfOpenOwnershipIntervalsAndPromotionBoundary()
    {
        var macro = new FidelityOwnership(
            "contest:9",
            SimulationFidelity.Macro,
            fromTickInclusive: 0,
            untilTickExclusive: 10_000);
        var boundary = new FidelityPromotionBoundary(
            "contest:9",
            SimulationFidelity.Macro,
            SimulationFidelity.Detailed,
            atTick: 10_000);

        Assert.True(macro.Owns(9_999));
        Assert.False(macro.Owns(10_000));
        Assert.Equal(10_000, boundary.AtTick);
        Assert.Equal(SimulationFidelity.Detailed, boundary.ToFidelity);
    }

    [Fact]
    public void RejectsEmptyIntervalsDemotionsAndRetroactiveCommitments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FidelityOwnership(
            "contest:9",
            SimulationFidelity.Macro,
            10,
            10));
        Assert.Throws<ArgumentException>(() => new FidelityPromotionBoundary(
            "contest:9",
            SimulationFidelity.Detailed,
            SimulationFidelity.Macro,
            20));
        Assert.Throws<ArgumentException>(() => new HistoricalCommitment(
            "commitment:42",
            "contest:9",
            "outcome:completed",
            committedAtTick: 100,
            effectiveAtTick: 99,
            SimulationFidelity.Macro,
            new Dictionary<string, string>()));
    }
}
