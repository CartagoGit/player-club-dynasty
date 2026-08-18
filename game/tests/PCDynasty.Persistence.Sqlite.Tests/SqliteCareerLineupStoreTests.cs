using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteCareerLineupStoreTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        $"pcdynasty-lineup-{Guid.NewGuid():N}");

    [Fact]
    public void MissingDatabaseReturnsNoPlan()
    {
        var store = Store();

        Assert.False(store.TryLoad("club:test", out LineupPlan? plan));
        Assert.Null(plan);
    }

    [Fact]
    public void SaveReopenAndReplaceRoundTripTheLatestClubPlan()
    {
        var store = Store();
        store.Save(Plan("contest:first", "balanced"));
        store.Save(Plan("contest:second", "attacking"));

        var reopened = Store();

        Assert.True(reopened.TryLoad("club:test", out LineupPlan? loaded));
        Assert.NotNull(loaded);
        Assert.Equal("contest:second", loaded.ContestId);
        Assert.Equal("attacking", loaded.Options["approach"]);
        Assert.Equal(["person:a", "person:b"], loaded.Assignments.Select(item => item.ParticipantId));
    }

    public void Dispose()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private SqliteCareerLineupStore Store() =>
        new(Path.Combine(temporaryDirectory, "external-career.sqlite"));

    private static LineupPlan Plan(string contestId, string approach) =>
        new(
            "club:test",
            contestId,
            new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero),
            [new LineupAssignment("person:a", "starter", 0), new LineupAssignment("person:b", "starter", 1)],
            [KeyValuePair.Create("approach", approach)]);
}
