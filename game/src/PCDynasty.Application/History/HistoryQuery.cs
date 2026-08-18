using System.Collections.Immutable;
using PCDynasty.History.Events;
using PCDynasty.History.Projections;

namespace PCDynasty.Application.History;

public static class HistoryQuery
{
    public static HistoryView ExecuteCareer(
        string subjectId,
        IEnumerable<HistoricalEvent> events)
    {
        CareerHistory history = CareerHistoryProjection.Project(subjectId, events);
        return Map(history.SubjectId, HistorySubjectKind.Career, history.Timeline);
    }

    public static HistoryView ExecuteCompetition(
        string competitionId,
        IEnumerable<HistoricalEvent> events)
    {
        CompetitionHistory history = CompetitionHistoryProjection.Project(competitionId, events);
        return Map(history.CompetitionId, HistorySubjectKind.Competition, history.Timeline);
    }

    private static HistoryView Map(
        string subjectId,
        HistorySubjectKind subjectKind,
        IEnumerable<HistoricalEvent> timeline)
    {
        ImmutableArray<HistoryEntryView> entries = timeline
            .Select(historicalEvent => new HistoryEntryView(
                historicalEvent.Sequence,
                historicalEvent.OccurredOnDay,
                historicalEvent.TypeId,
                historicalEvent.SubjectIds,
                historicalEvent.Values))
            .ToImmutableArray();
        return new HistoryView(subjectId, subjectKind, entries);
    }
}
