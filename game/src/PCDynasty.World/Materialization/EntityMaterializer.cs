using PCDynasty.World.Commitments;
using PCDynasty.World.State;

namespace PCDynasty.World.Materialization;

public static class EntityMaterializer
{
    public static MaterializationResult Materialize(
        WorldCheckpoint checkpoint,
        MaterializationLedger ledger,
        FidelityPromotionBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(boundary);
        if (boundary.AtTick != checkpoint.Sequence)
        {
            throw new InvalidOperationException(
                "Materialization must use the checkpoint captured at the promotion boundary.");
        }

        ClubSeasonState[] sourceStates = checkpoint.ClubSeasons
            .Where(state => string.Equals(
                state.ClubId,
                boundary.SubjectId,
                StringComparison.Ordinal))
            .ToArray();
        if (sourceStates.Length == 0)
        {
            throw new InvalidOperationException(
                $"Checkpoint contains no macro state for '{boundary.SubjectId}'.");
        }

        HistoricalCommitment[] subjectCommitments = ledger.Commitments
            .Where(commitment => string.Equals(
                commitment.SubjectId,
                boundary.SubjectId,
                StringComparison.Ordinal))
            .ToArray();
        if (subjectCommitments.Any(commitment =>
                commitment.CommittedAtTick > boundary.AtTick))
        {
            throw new InvalidOperationException(
                "Materialization cannot consume commitments confirmed after its boundary.");
        }

        MaterializationLedger promoted = FidelityPromotion.Apply(ledger, boundary);
        return new MaterializationResult(
            checkpoint,
            promoted,
            sourceStates,
            subjectCommitments);
    }
}
