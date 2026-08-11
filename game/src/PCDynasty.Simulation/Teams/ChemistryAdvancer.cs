using System.Collections.Immutable;

namespace PCDynasty.Simulation.Teams;

public sealed class TeamChemistryEvent
{
    public TeamChemistryEvent(
        long sequence,
        DateOnly occurredOn,
        ChemistryEventKind kind,
        int magnitude,
        string sourceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);

        ArgumentOutOfRangeException.ThrowIfEqual(
            occurredOn,
            DateOnly.MinValue,
            nameof(occurredOn));
        if (magnitude is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(magnitude));
        }

        ArgumentNullException.ThrowIfNull(sourceId);
        string normalizedSourceId = sourceId.Trim();
        if (normalizedSourceId.Length == 0)
        {
            throw new ArgumentException("An event source is required.", nameof(sourceId));
        }

        Sequence = sequence;
        OccurredOn = occurredOn;
        Kind = kind;
        Magnitude = magnitude;
        SourceId = normalizedSourceId;
    }

    public long Sequence { get; }

    public DateOnly OccurredOn { get; }

    public ChemistryEventKind Kind { get; }

    public int Magnitude { get; }

    public string SourceId { get; }
}

public static class ChemistryAdvancer
{
    public static TeamChemistry Advance(
        TeamChemistry current,
        IEnumerable<TeamChemistryEvent> events)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(events);

        ImmutableArray<TeamChemistryEvent> orderedEvents = events
            .OrderBy(item => item.Sequence)
            .ToImmutableArray();
        if (orderedEvents.IsEmpty)
        {
            return current;
        }

        if (orderedEvents.Any(item => item.Sequence <= current.LastAppliedSequence) ||
            orderedEvents.Select(item => item.Sequence).Distinct().Count() != orderedEvents.Length)
        {
            throw new ArgumentException(
                "Chemistry events must have unique sequences after the current state.",
                nameof(events));
        }

        int cohesion = current.Cohesion;
        int trust = current.Trust;
        int familiarity = current.Familiarity;
        var factors = ImmutableArray.CreateBuilder<ChemistryFactor>(orderedEvents.Length);

        foreach (TeamChemistryEvent teamEvent in orderedEvents)
        {
            ChemistryChange requested = ResolveChange(teamEvent);
            int nextCohesion = Clamp(cohesion + requested.Cohesion);
            int nextTrust = Clamp(trust + requested.Trust);
            int nextFamiliarity = Clamp(familiarity + requested.Familiarity);
            int cohesionDelta = nextCohesion - cohesion;
            int trustDelta = nextTrust - trust;
            int familiarityDelta = nextFamiliarity - familiarity;

            factors.Add(new ChemistryFactor(
                teamEvent.Sequence,
                teamEvent.Kind,
                teamEvent.SourceId,
                cohesionDelta,
                trustDelta,
                familiarityDelta,
                requested.Explanation));
            cohesion = nextCohesion;
            trust = nextTrust;
            familiarity = nextFamiliarity;
        }

        return current.WithAdvance(
            cohesion,
            trust,
            familiarity,
            orderedEvents[^1].Sequence,
            factors.ToImmutable());
    }

    private static ChemistryChange ResolveChange(TeamChemistryEvent teamEvent)
    {
        int magnitude = teamEvent.Magnitude;
        int half = (int)Math.Ceiling(magnitude / 2d);
        int quarter = (int)Math.Ceiling(magnitude / 4d);
        return teamEvent.Kind switch
        {
            ChemistryEventKind.SharedSuccess => new(
                magnitude,
                half,
                quarter,
                "Shared success reinforces cohesion, trust and familiarity."),
            ChemistryEventKind.SharedAdversity => new(
                half,
                half,
                quarter,
                "Facing adversity together can reinforce mutual trust."),
            ChemistryEventKind.RosterContinuity => new(
                half,
                quarter,
                magnitude,
                "Roster continuity primarily increases shared familiarity."),
            ChemistryEventKind.InternalConflict => new(
                -half,
                -magnitude,
                -quarter,
                "Internal conflict damages trust and cohesion."),
            ChemistryEventKind.LeadershipChange => new(
                -quarter,
                -half,
                -half,
                "Leadership change temporarily disrupts trust and familiarity."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(teamEvent),
                teamEvent.Kind,
                "Unknown chemistry event kind."),
        };
    }

    private static int Clamp(int value) => Math.Clamp(value, 0, 100);

    private readonly record struct ChemistryChange(
        int Cohesion,
        int Trust,
        int Familiarity,
        string Explanation);
}
