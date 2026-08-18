using System.Collections.Immutable;
using PCDynasty.History.Events;

namespace PCDynasty.History.Projections;

public static class HistoricalEventTypes
{
    public const string Transfer = "history:transfer";
    public const string Title = "history:title";
    public const string Record = "history:record";
    public const string CompetitionEdition = "history:competition-edition";
}

public static class CareerHistoryProjection
{
    public static CareerHistory Project(string subjectId, IEnumerable<HistoricalEvent> events)
    {
        string normalizedSubjectId = HistoryValue.NormalizeIdentifier(subjectId, nameof(subjectId));
        ImmutableArray<HistoricalEvent> timeline = HistoryProjection.SelectTimeline(
            normalizedSubjectId,
            events);

        return new CareerHistory(
            normalizedSubjectId,
            timeline,
            HistoryProjection.OfType(timeline, HistoricalEventTypes.Transfer),
            HistoryProjection.OfType(timeline, HistoricalEventTypes.Title),
            HistoryProjection.OfType(timeline, HistoricalEventTypes.Record));
    }
}

public sealed class CareerHistory
{
    internal CareerHistory(
        string subjectId,
        ImmutableArray<HistoricalEvent> timeline,
        ImmutableArray<HistoricalEvent> transfers,
        ImmutableArray<HistoricalEvent> titles,
        ImmutableArray<HistoricalEvent> records)
    {
        SubjectId = subjectId;
        Timeline = timeline;
        Transfers = transfers;
        Titles = titles;
        Records = records;
    }

    public string SubjectId { get; }
    public ImmutableArray<HistoricalEvent> Timeline { get; }
    public ImmutableArray<HistoricalEvent> Transfers { get; }
    public ImmutableArray<HistoricalEvent> Titles { get; }
    public ImmutableArray<HistoricalEvent> Records { get; }
}

internal static class HistoryProjection
{
    public static ImmutableArray<HistoricalEvent> SelectTimeline(
        string subjectId,
        IEnumerable<HistoricalEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        HistoricalEvent[] timeline = events
            .Select(historicalEvent =>
            {
                ArgumentNullException.ThrowIfNull(historicalEvent);
                return historicalEvent;
            })
            .Where(historicalEvent => historicalEvent.SubjectIds.Contains(
                subjectId,
                StringComparer.Ordinal))
            .OrderBy(historicalEvent => historicalEvent.Sequence)
            .ToArray();

        for (int index = 1; index < timeline.Length; index++)
        {
            if (timeline[index - 1].Sequence == timeline[index].Sequence)
            {
                throw new ArgumentException(
                    $"Historical sequence {timeline[index].Sequence} is ambiguous.",
                    nameof(events));
            }
        }

        return timeline.ToImmutableArray();
    }

    public static ImmutableArray<HistoricalEvent> OfType(
        ImmutableArray<HistoricalEvent> timeline,
        string typeId) =>
        timeline
            .Where(historicalEvent => string.Equals(
                historicalEvent.TypeId,
                typeId,
                StringComparison.Ordinal))
            .ToImmutableArray();
}
