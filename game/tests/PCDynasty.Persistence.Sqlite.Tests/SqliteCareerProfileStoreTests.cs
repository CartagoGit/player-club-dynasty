using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteCareerProfileStoreTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        "pcd-profile-tests",
        Guid.NewGuid().ToString("N"),
        "career.sqlite");

    public void Dispose()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void MissingDatabaseReturnsAnExplicitEmptyResult()
    {
        var store = new SqliteCareerProfileStore(_databasePath);

        var found = store.TryLoad(out var profile);

        Assert.False(found);
        Assert.Null(profile);
    }

    [Fact]
    public void SavedProfileRoundTripsThroughAnExternalDatabase()
    {
        var expected = new CareerProfile(
            clubExternalId: "club:aurora-quay",
            clubDisplayName: "Aurora Quay Athletic",
            startedAtUtc: new DateTimeOffset(2026, 8, 18, 8, 30, 0, TimeSpan.Zero),
            lastPlayedAtUtc: new DateTimeOffset(2026, 8, 18, 9, 45, 0, TimeSpan.Zero));

        new SqliteCareerProfileStore(_databasePath).Save(expected);
        var reopenedStore = new SqliteCareerProfileStore(_databasePath);

        Assert.True(reopenedStore.TryLoad(out var actual));
        Assert.Equal(expected, actual);
        Assert.Equal(_databasePath, reopenedStore.DatabasePath);
    }

    [Fact]
    public void SavingAgainReplacesTheSingleActiveProfile()
    {
        var store = new SqliteCareerProfileStore(_databasePath);
        store.Save(NewProfile("club:first", "First Club"));

        var expected = NewProfile("club:second", "Second Club");
        store.Save(expected);

        Assert.True(store.TryLoad(out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RejectsInvalidPathsAndProfileIdentity()
    {
        Assert.Throws<ArgumentException>(() => new SqliteCareerProfileStore(" "));
        Assert.Throws<ArgumentException>(() => NewProfile(" ", "Club"));
        Assert.Throws<ArgumentException>(() => NewProfile("club:test", " "));
    }

    private static CareerProfile NewProfile(string clubExternalId, string clubDisplayName) =>
        new(
            clubExternalId,
            clubDisplayName,
            new DateTimeOffset(2026, 8, 18, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 18, 9, 45, 0, TimeSpan.Zero));
}
