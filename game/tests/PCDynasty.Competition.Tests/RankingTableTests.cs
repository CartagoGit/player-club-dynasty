using PCDynasty.Competition.Outcomes;
using PCDynasty.Competition.Rankings;
using Xunit;

namespace PCDynasty.Competition.Tests;

public sealed class RankingTableTests
{
    [Fact]
    public void OrdersOpaqueCriteriaWithoutKnowingSportScoring()
    {
        var table = new RankingTable(
            [
                new RankingCandidate("participant:b", [24, 7, 12]),
                new RankingCandidate("participant:a", [27, 3, 18]),
                new RankingCandidate("participant:c", [24, 9, 10]),
            ],
            [
                RankingCriterionDirection.Descending,
                RankingCriterionDirection.Descending,
                RankingCriterionDirection.Ascending,
            ]);

        Assert.Equal(
            ["participant:a", "participant:c", "participant:b"],
            table.Entries.Select(entry => entry.ParticipantId));
        Assert.Equal([1, 2, 3], table.Entries.Select(entry => entry.Rank));
    }

    [Fact]
    public void RejectsIncompleteCriteriaAndUnresolvedTies()
    {
        Assert.Throws<ArgumentException>(() => new RankingTable(
            [new RankingCandidate("participant:a", [24])],
            [RankingCriterionDirection.Descending, RankingCriterionDirection.Ascending]));

        Assert.Throws<ArgumentException>(() => new RankingTable(
            [
                new RankingCandidate("participant:a", [24, 7]),
                new RankingCandidate("participant:b", [24, 7]),
            ],
            [RankingCriterionDirection.Descending, RankingCriterionDirection.Descending]));
    }

    [Fact]
    public void ExpressesQualificationAsOpaqueOutcomes()
    {
        var outcome = new QualificationOutcome(
            "participant:a",
            "outcome:advance",
            "stage:next");

        Assert.Equal("participant:a", outcome.ParticipantId);
        Assert.Equal("outcome:advance", outcome.OutcomeId);
        Assert.Equal("stage:next", outcome.DestinationStageId);

        string[] publicMemberNames = typeof(RankingTable).Assembly
            .GetExportedTypes()
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain(publicMemberNames, name =>
            name.Contains("Goal", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Point", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Win", StringComparison.OrdinalIgnoreCase));
    }
}
