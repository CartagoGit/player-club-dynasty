namespace PCDynasty.Core.Contests;

public sealed class ContestEvent
{
    public ContestEvent(
        int sequence,
        int tick,
        string kindId,
        string? actorId,
        int magnitude)
    {
        if (sequence < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence),
                sequence,
                "An event sequence cannot be negative.");
        }

        if (tick < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tick),
                tick,
                "An event tick cannot be negative.");
        }

        Sequence = sequence;
        Tick = tick;
        KindId = ContestValue.NormalizeIdentifier(kindId, nameof(kindId));
        ActorId = actorId is null
            ? null
            : ContestValue.NormalizeIdentifier(actorId, nameof(actorId));
        Magnitude = magnitude;
    }

    public int Sequence { get; }

    public int Tick { get; }

    public string KindId { get; }

    public string? ActorId { get; }

    public int Magnitude { get; }
}
