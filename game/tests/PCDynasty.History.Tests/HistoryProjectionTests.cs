using PCDynasty.History.Events;
using PCDynasty.History.Projections;
using Xunit;

namespace PCDynasty.History.Tests;

public sealed class HistoryProjectionTests
{
    [Fact]
    public void ReconstructsCareerTimelineIndependentlyOfInputOrder()
    {
        HistoricalEvent transfer = Event(2, 100, HistoricalEventTypes.Transfer, "person:42", "club:7");
        HistoricalEvent title = Event(5, 240, HistoricalEventTypes.Title, "person:42", "competition:league");
        HistoricalEvent record = Event(9, 300, HistoricalEventTypes.Record, "person:42");
        HistoricalEvent irrelevant = Event(3, 120, HistoricalEventTypes.Transfer, "person:99");
        CareerHistory first = CareerHistoryProjection.Project(
            "person:42",
            [record, irrelevant, title, transfer]);
        CareerHistory second = CareerHistoryProjection.Project(
            "person:42",
            [transfer, title, irrelevant, record]);

        Assert.Equal([2L, 5L, 9L], first.Timeline.Select(item => item.Sequence));
        Assert.Equal(
            first.Timeline.Select(item => item.Sequence),
            second.Timeline.Select(item => item.Sequence));
        Assert.Same(transfer, Assert.Single(first.Transfers));
        Assert.Same(title, Assert.Single(first.Titles));
        Assert.Same(record, Assert.Single(first.Records));
    }

    [Fact]
    public void ReconstructsCompetitionEditionsTitlesAndRecords()
    {
        HistoricalEvent edition = Event(
            8,
            220,
            HistoricalEventTypes.CompetitionEdition,
            "competition:league");
        HistoricalEvent title = Event(
            10,
            240,
            HistoricalEventTypes.Title,
            "club:7",
            "competition:league");
        HistoricalEvent record = Event(
            12,
            250,
            HistoricalEventTypes.Record,
            "competition:league",
            "person:42");

        CompetitionHistory history = CompetitionHistoryProjection.Project(
            "competition:league",
            [record, title, edition]);

        Assert.Equal([8L, 10L, 12L], history.Timeline.Select(item => item.Sequence));
        Assert.Same(edition, Assert.Single(history.Editions));
        Assert.Same(title, Assert.Single(history.Titles));
        Assert.Same(record, Assert.Single(history.Records));
    }

    [Fact]
    public void RejectsAmbiguousDuplicateSequencesInAProjection()
    {
        HistoricalEvent first = Event(4, 100, HistoricalEventTypes.Transfer, "person:42");
        HistoricalEvent duplicate = Event(4, 101, HistoricalEventTypes.Title, "person:42");

        Assert.Throws<ArgumentException>(() => CareerHistoryProjection.Project(
            "person:42",
            [first, duplicate]));
    }

    private static HistoricalEvent Event(
        long sequence,
        int occurredOnDay,
        string typeId,
        params string[] subjects) =>
        new(
            sequence,
            occurredOnDay,
            typeId,
            schemaVersion: 1,
            subjects,
            new Dictionary<string, string>());
}
