using PCDynasty.Application.Matches;
using PCDynasty.Core.Contests;
using PCDynasty.Football.Attributes;
using PCDynasty.Football.Simulation;
using Xunit;

namespace PCDynasty.Football.Tests;

public sealed class FootballContestPackTests
{
    [Fact]
    public void MapsDeterministicGenericEventsIntoAReadOnlyFootballTimeline()
    {
        ContestSnapshot snapshot = CreateSnapshot("football:standard:v1", seed: 741L);

        IReadOnlyList<ContestEvent> first = FootballContestPack.Simulate(snapshot);
        IReadOnlyList<ContestEvent> second = FootballContestPack.Simulate(snapshot);
        MatchTimelineView timeline = MatchTimelineQuery.Execute(first);

        Assert.Equal(EventValues(first), EventValues(second));
        Assert.Equal(FootballContestPack.TimelineEventCount, first.Count);
        Assert.All(first, item => Assert.StartsWith("football:", item.KindId));
        Assert.Equal(first.Count, timeline.Items.Length);
        Assert.Equal(first[0].KindId, timeline.Items[0].EventKey);
        Assert.Equal(first[0].Tick, timeline.Items[0].Tick);
        Assert.All(
            typeof(MatchTimelineView).GetProperties()
                .Concat(typeof(MatchTimelineItemView).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void RejectsAnotherRulesetOrAnIncompleteFootballCatalogue()
    {
        Assert.Throws<ArgumentException>(() =>
            FootballContestPack.Simulate(CreateSnapshot("rules:test-sport:v1", seed: 741L)));

        var incomplete = new ContestSnapshot(
            "contest:2026:42",
            "football:standard:v1",
            seed: 741L,
            [
                new ContestParticipantSnapshot("participant:home", [70, 80], form: 65),
                new ContestParticipantSnapshot("participant:away", [60, 50], form: 70),
            ]);

        Assert.Throws<ArgumentException>(() => FootballContestPack.Simulate(incomplete));
    }

    [Fact]
    public void ProductionDependenciesDoNotReverseIntoApplication()
    {
        string[] footballReferences = typeof(FootballContestPack).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        string[] applicationReferences = typeof(MatchTimelineQuery).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("PCDynasty.Application", footballReferences);
        Assert.DoesNotContain("PCDynasty.Football", applicationReferences);
        Assert.DoesNotContain(footballReferences, name => name.StartsWith("Godot", StringComparison.Ordinal));
        Assert.DoesNotContain(applicationReferences, name => name.StartsWith("Godot", StringComparison.Ordinal));
    }

    private static ContestSnapshot CreateSnapshot(string rulesetId, long seed)
    {
        int[] homeAttributes = Enumerable.Repeat(72, FootballAttributeCatalog.Count).ToArray();
        int[] awayAttributes = Enumerable.Repeat(66, FootballAttributeCatalog.Count).ToArray();

        return new ContestSnapshot(
            "contest:2026:42",
            rulesetId,
            seed,
            [
                new ContestParticipantSnapshot("participant:home", homeAttributes, form: 74),
                new ContestParticipantSnapshot("participant:away", awayAttributes, form: 68),
            ]);
    }

    private static IEnumerable<(int Sequence, int Tick, string Kind, string? Actor, int Magnitude)>
        EventValues(IEnumerable<ContestEvent> events)
    {
        return events.Select(item =>
            (item.Sequence, item.Tick, item.KindId, item.ActorId, item.Magnitude));
    }
}
