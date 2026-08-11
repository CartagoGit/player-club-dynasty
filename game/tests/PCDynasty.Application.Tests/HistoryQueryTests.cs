using PCDynasty.Application.History;
using PCDynasty.History.Events;
using Xunit;

namespace PCDynasty.Application.Tests;

public sealed class HistoryQueryTests
{
    [Fact]
    public void MapsCareerHistoryIntoAnImmutableStorageAgnosticView()
    {
        HistoricalEvent transfer = Event(2, 100, "history:transfer", "person:42", "club:7");
        HistoricalEvent title = Event(5, 240, "history:title", "person:42", "competition:league");

        HistoryView view = HistoryQuery.ExecuteCareer("person:42", [title, transfer]);

        Assert.Equal("person:42", view.SubjectId);
        Assert.Equal(HistorySubjectKind.Career, view.SubjectKind);
        Assert.Equal([2L, 5L], view.Entries.Select(entry => entry.Sequence));
        Assert.Equal("history:transfer", view.Entries[0].TypeId);
        Assert.Equal("club:7", view.Entries[0].SubjectIds[1]);
        Assert.All(
            typeof(HistoryView).GetProperties()
                .Concat(typeof(HistoryEntryView).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void MapsCompetitionHistoryWithoutExposingDomainOrInfrastructureTypes()
    {
        HistoricalEvent edition = Event(
            8,
            220,
            "history:competition-edition",
            "competition:league");

        HistoryView view = HistoryQuery.ExecuteCompetition("competition:league", [edition]);

        Assert.Equal(HistorySubjectKind.Competition, view.SubjectKind);
        Assert.Equal(8, Assert.Single(view.Entries).Sequence);
        Type[] propertyTypes = typeof(HistoryView).GetProperties()
            .Concat(typeof(HistoryEntryView).GetProperties())
            .Select(property => property.PropertyType)
            .ToArray();
        string[] references = typeof(HistoryView).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(typeof(HistoricalEvent), propertyTypes);
        Assert.DoesNotContain(references, name =>
            name.Contains("Persistence", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Godot", StringComparison.Ordinal));
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
