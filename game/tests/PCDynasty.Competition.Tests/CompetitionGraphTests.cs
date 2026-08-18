using PCDynasty.Competition.Stages;
using Xunit;

namespace PCDynasty.Competition.Tests;

public sealed class CompetitionGraphTests
{
    [Fact]
    public void BuildsAReachableSportNeutralStageGraph()
    {
        var graph = new CompetitionGraph(
            "stage:qualification",
            [
                new CompetitionStageDefinition("stage:qualification", 8),
                new CompetitionStageDefinition("stage:final", 2),
            ],
            [new CompetitionTransition("stage:qualification", "stage:final", "outcome:advance")]);

        Assert.Equal("stage:qualification", graph.EntryStageId);
        Assert.Equal(2, graph.Stages.Length);
        Assert.Single(graph.Transitions);
        Assert.All(
            typeof(CompetitionGraph).GetProperties()
                .Concat(typeof(CompetitionStageDefinition).GetProperties())
                .Concat(typeof(CompetitionTransition).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void RejectsUnknownOrUnreachableStages()
    {
        Assert.Throws<ArgumentException>(() => new CompetitionGraph(
            "stage:start",
            [new CompetitionStageDefinition("stage:start", 2)],
            [new CompetitionTransition("stage:start", "stage:missing", "outcome:advance")]));

        Assert.Throws<ArgumentException>(() => new CompetitionGraph(
            "stage:start",
            [
                new CompetitionStageDefinition("stage:start", 2),
                new CompetitionStageDefinition("stage:orphan", 2),
            ],
            []));
    }

    [Fact]
    public void RejectsCyclesAndDuplicateStageIdentifiers()
    {
        Assert.Throws<ArgumentException>(() => new CompetitionGraph(
            "stage:a",
            [
                new CompetitionStageDefinition("stage:a", 2),
                new CompetitionStageDefinition("stage:b", 2),
            ],
            [
                new CompetitionTransition("stage:a", "stage:b", "outcome:advance"),
                new CompetitionTransition("stage:b", "stage:a", "outcome:return"),
            ]));

        Assert.Throws<ArgumentException>(() => new CompetitionGraph(
            "stage:a",
            [
                new CompetitionStageDefinition("stage:a", 2),
                new CompetitionStageDefinition("stage:a", 4),
            ],
            []));
    }
}
