using System.Collections.Immutable;
using PCDynasty.Core.Contests;

namespace PCDynasty.Application.Matches;

public static class MatchTimelineQuery
{
    public static MatchTimelineView Execute(IEnumerable<ContestEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        ImmutableArray<ContestEvent> copy = events.ToImmutableArray();
        ValidateOrdering(copy);

        var items = ImmutableArray.CreateBuilder<MatchTimelineItemView>(copy.Length);
        foreach (ContestEvent contestEvent in copy)
        {
            items.Add(new MatchTimelineItemView(
                contestEvent.Sequence,
                contestEvent.Tick,
                contestEvent.KindId,
                contestEvent.ActorId,
                contestEvent.Magnitude));
        }

        return new MatchTimelineView(items.ToImmutable());
    }

    private static void ValidateOrdering(ImmutableArray<ContestEvent> events)
    {
        int previousTick = -1;
        for (int index = 0; index < events.Length; index++)
        {
            ContestEvent? contestEvent = events[index];
            if (contestEvent is null)
            {
                throw new ArgumentException("A timeline cannot contain a null event.", nameof(events));
            }

            if (contestEvent.Sequence != index || contestEvent.Tick < previousTick)
            {
                throw new ArgumentException(
                    "Timeline events must have contiguous sequences and nondecreasing ticks.",
                    nameof(events));
            }

            previousTick = contestEvent.Tick;
        }
    }
}

public sealed class MatchTimelineView
{
    internal MatchTimelineView(ImmutableArray<MatchTimelineItemView> items)
    {
        Items = items;
    }

    public ImmutableArray<MatchTimelineItemView> Items { get; }
}

public sealed class MatchTimelineItemView
{
    internal MatchTimelineItemView(
        int sequence,
        int tick,
        string eventKey,
        string? actorId,
        int magnitude)
    {
        Sequence = sequence;
        Tick = tick;
        EventKey = eventKey;
        ActorId = actorId;
        Magnitude = magnitude;
    }

    public int Sequence { get; }

    public int Tick { get; }

    public string EventKey { get; }

    public string? ActorId { get; }

    public int Magnitude { get; }
}
