using System.Collections.Immutable;
using PCDynasty.History.Events;

namespace PCDynasty.History.Projections;

public static class CompetitionHistoryProjection
{
    public static CompetitionHistory Project(string competitionId, IEnumerable<HistoricalEvent> events)
    {
        string normalizedCompetitionId = HistoryValue.NormalizeIdentifier(
            competitionId,
            nameof(competitionId));
        ImmutableArray<HistoricalEvent> timeline = HistoryProjection.SelectTimeline(
            normalizedCompetitionId,
            events);

        return new CompetitionHistory(
            normalizedCompetitionId,
            timeline,
            HistoryProjection.OfType(timeline, HistoricalEventTypes.CompetitionEdition),
            HistoryProjection.OfType(timeline, HistoricalEventTypes.Title),
            HistoryProjection.OfType(timeline, HistoricalEventTypes.Record));
    }
}

public sealed class CompetitionHistory
{
    internal CompetitionHistory(
        string competitionId,
        ImmutableArray<HistoricalEvent> timeline,
        ImmutableArray<HistoricalEvent> editions,
        ImmutableArray<HistoricalEvent> titles,
        ImmutableArray<HistoricalEvent> records)
    {
        CompetitionId = competitionId;
        Timeline = timeline;
        Editions = editions;
        Titles = titles;
        Records = records;
    }

    public string CompetitionId { get; }
    public ImmutableArray<HistoricalEvent> Timeline { get; }
    public ImmutableArray<HistoricalEvent> Editions { get; }
    public ImmutableArray<HistoricalEvent> Titles { get; }
    public ImmutableArray<HistoricalEvent> Records { get; }
}
