namespace PCDynasty.Contracts.Simulation;

public readonly record struct EventSequence : IComparable<EventSequence>
{
    public EventSequence(long worldTick, int phase, string emitterId, int localSequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(worldTick);
        ArgumentOutOfRangeException.ThrowIfNegative(phase);
        ArgumentOutOfRangeException.ThrowIfNegative(localSequence);

        WorldTick = worldTick;
        Phase = phase;
        EmitterId = ContractValue.NormalizeIdentifier(emitterId, nameof(emitterId));
        LocalSequence = localSequence;
    }

    public long WorldTick { get; }
    public int Phase { get; }
    public string EmitterId { get; }
    public int LocalSequence { get; }

    public static bool operator <(EventSequence left, EventSequence right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(EventSequence left, EventSequence right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(EventSequence left, EventSequence right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(EventSequence left, EventSequence right) =>
        left.CompareTo(right) >= 0;

    public int CompareTo(EventSequence other)
    {
        int comparison = WorldTick.CompareTo(other.WorldTick);
        if (comparison != 0)
        {
            return comparison;
        }

        comparison = Phase.CompareTo(other.Phase);
        if (comparison != 0)
        {
            return comparison;
        }

        comparison = StringComparer.Ordinal.Compare(EmitterId, other.EmitterId);
        return comparison != 0
            ? comparison
            : LocalSequence.CompareTo(other.LocalSequence);
    }
}
