using PCDynasty.Application.Competitions;
using PCDynasty.Competition.Outcomes;
using PCDynasty.Competition.Rankings;
using Xunit;

namespace PCDynasty.Application.Tests;

public sealed class SeasonTableQueryTests
{
    [Fact]
    public void MapsCompetitionStateIntoImmutableReadModels()
    {
        var table = new RankingTable(
            [
                new RankingCandidate("participant:a", [27, 18]),
                new RankingCandidate("participant:b", [24, 12]),
            ],
            [RankingCriterionDirection.Descending, RankingCriterionDirection.Descending]);
        QualificationOutcome[] outcomes =
        [
            new QualificationOutcome("participant:a", "football:promoted", null),
            new QualificationOutcome("participant:b", "football:retained", null),
        ];

        SeasonTableView view = SeasonTableQuery.Execute(table, outcomes);

        Assert.Equal(2, view.Entries.Length);
        Assert.Equal(1, view.Entries[0].Rank);
        Assert.Equal("participant:a", view.Entries[0].ParticipantId);
        Assert.Equal([27L, 18L], view.Entries[0].Criteria.AsSpan().ToArray());
        Assert.Equal("football:promoted", view.Entries[0].OutcomeId);
        Assert.All(
            typeof(SeasonTableView).GetProperties()
                .Concat(typeof(SeasonTableEntryView).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void RejectsMissingOrDuplicateOutcomes()
    {
        var table = new RankingTable(
            [new RankingCandidate("participant:a", [27])],
            [RankingCriterionDirection.Descending]);

        Assert.Throws<ArgumentException>(() => SeasonTableQuery.Execute(table, []));
        Assert.Throws<ArgumentException>(() => SeasonTableQuery.Execute(
            table,
            [
                new QualificationOutcome("participant:a", "outcome:first", null),
                new QualificationOutcome("participant:a", "outcome:second", null),
            ]));
    }

    [Fact]
    public void ApplicationDoesNotExposeCompetitionEntitiesOrReferenceFootball()
    {
        Type[] viewPropertyTypes = typeof(SeasonTableView).GetProperties()
            .Concat(typeof(SeasonTableEntryView).GetProperties())
            .Select(property => property.PropertyType)
            .ToArray();
        string[] references = typeof(SeasonTableView).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(typeof(RankingEntry), viewPropertyTypes);
        Assert.DoesNotContain(typeof(QualificationOutcome), viewPropertyTypes);
        Assert.DoesNotContain("PCDynasty.Football", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Godot", StringComparison.Ordinal));
    }
}
