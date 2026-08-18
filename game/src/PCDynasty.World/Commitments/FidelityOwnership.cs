namespace PCDynasty.World.Commitments;

public enum SimulationFidelity
{
    Macro,
    Detailed,
    Materialized,
}

public sealed class FidelityOwnership
{
    public FidelityOwnership(
        string subjectId,
        SimulationFidelity fidelity,
        long fromTickInclusive,
        long? untilTickExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromTickInclusive);
        if (untilTickExclusive is not null && untilTickExclusive <= fromTickInclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(untilTickExclusive),
                "An ownership interval must be non-empty.");
        }

        if (!Enum.IsDefined(fidelity))
        {
            throw new ArgumentOutOfRangeException(nameof(fidelity));
        }

        SubjectId = WorldValue.NormalizeIdentifier(subjectId, nameof(subjectId));
        Fidelity = fidelity;
        FromTickInclusive = fromTickInclusive;
        UntilTickExclusive = untilTickExclusive;
    }

    public string SubjectId { get; }
    public SimulationFidelity Fidelity { get; }
    public long FromTickInclusive { get; }
    public long? UntilTickExclusive { get; }

    public bool Owns(long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        return tick >= FromTickInclusive &&
               (UntilTickExclusive is null || tick < UntilTickExclusive);
    }
}

public sealed class FidelityPromotionBoundary
{
    public FidelityPromotionBoundary(
        string subjectId,
        SimulationFidelity fromFidelity,
        SimulationFidelity toFidelity,
        long atTick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(atTick);
        if (!Enum.IsDefined(fromFidelity))
        {
            throw new ArgumentOutOfRangeException(nameof(fromFidelity));
        }

        if (!Enum.IsDefined(toFidelity))
        {
            throw new ArgumentOutOfRangeException(nameof(toFidelity));
        }

        if ((int)toFidelity != (int)fromFidelity + 1)
        {
            throw new ArgumentException("Fidelity promotion must advance exactly one level.");
        }

        SubjectId = WorldValue.NormalizeIdentifier(subjectId, nameof(subjectId));
        FromFidelity = fromFidelity;
        ToFidelity = toFidelity;
        AtTick = atTick;
    }

    public string SubjectId { get; }
    public SimulationFidelity FromFidelity { get; }
    public SimulationFidelity ToFidelity { get; }
    public long AtTick { get; }
}
