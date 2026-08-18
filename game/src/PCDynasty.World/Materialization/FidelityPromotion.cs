using PCDynasty.World.Commitments;

namespace PCDynasty.World.Materialization;

public static class FidelityPromotion
{
    public static MaterializationLedger Apply(
        MaterializationLedger ledger,
        FidelityPromotionBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(boundary);
        return ledger.ApplyPromotion(boundary);
    }
}
