using PCDynasty.History.Events;
using Xunit;

namespace PCDynasty.History.Tests;

public sealed class HistoricalEventTests
{
    [Fact]
    public void RecordsCompactVersionedFactsWithDefensiveCopies()
    {
        string[] subjects = ["person:42", "club:7"];
        var values = new Dictionary<string, string>
        {
            ["from"] = "club:3",
            ["fee"] = "1500000",
        };

        var historicalEvent = new HistoricalEvent(
            sequence: 18,
            occurredOnDay: 4_802,
            typeId: "history:transfer",
            schemaVersion: 2,
            subjects,
            values);
        subjects[0] = "changed";
        values["fee"] = "0";

        Assert.Equal(18, historicalEvent.Sequence);
        Assert.Equal(4_802, historicalEvent.OccurredOnDay);
        Assert.Equal("history:transfer", historicalEvent.TypeId);
        Assert.Equal(2, historicalEvent.SchemaVersion);
        Assert.Collection(
            historicalEvent.SubjectIds,
            subjectId => Assert.Equal("person:42", subjectId),
            subjectId => Assert.Equal("club:7", subjectId));
        Assert.Equal("1500000", historicalEvent.Values["fee"]);
    }

    [Fact]
    public void RejectsInvalidSequencesVersionsAndDuplicateSubjects()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HistoricalEvent(
            0,
            20,
            "history:title",
            1,
            ["club:7"],
            new Dictionary<string, string>()));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HistoricalEvent(
            1,
            -1,
            "history:title",
            1,
            ["club:7"],
            new Dictionary<string, string>()));
        Assert.Throws<ArgumentException>(() => new HistoricalEvent(
            1,
            20,
            "history:title",
            1,
            ["club:7", "club:7"],
            new Dictionary<string, string>()));
    }

    [Fact]
    public void HistoryContractDoesNotDependOnStorageOrPresentation()
    {
        string[] references = typeof(HistoricalEvent).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name =>
            name.Contains("Persistence", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Godot", StringComparison.OrdinalIgnoreCase));
    }
}
