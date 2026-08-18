using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteCareerFinanceStoreTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        $"pcdynasty-finance-{Guid.NewGuid():N}");

    [Fact]
    public void MissingExternalDatabaseReturnsNoFinanceState()
    {
        var store = Store();

        Assert.False(store.TryLoad("club:test", out CareerFinanceState? state));
        Assert.Null(state);
    }

    [Fact]
    public void SaveReopenAndReplaceRoundTripLatestValues()
    {
        var store = Store();
        store.Save(State(cash: 1_000, year: 2026));
        store.Save(State(cash: 2_500, year: 2027));

        var reopened = Store();

        Assert.True(reopened.TryLoad("club:test", out CareerFinanceState? loaded));
        Assert.NotNull(loaded);
        Assert.Equal(2_500, loaded.Cash);
        Assert.Equal(2027, loaded.SeasonYear);
        Assert.Equal(FixedTimestamp, loaded.UpdatedAtUtc);
        Assert.Equal(Path.Combine(_temporaryDirectory, "external-career.sqlite"), reopened.DatabasePath);
    }

    [Fact]
    public void ClearRemovesOnlyFinanceRowsAndPreservesOtherCareerTables()
    {
        var store = Store();
        store.Save(State(cash: 1_000, year: 2026));
        CreateSentinelCareerTable(store.DatabasePath);

        store.Clear();

        Assert.False(Store().TryLoad("club:test", out CareerFinanceState? state));
        Assert.Null(state);
        Assert.Equal("preserved", ReadSentinel(store.DatabasePath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private SqliteCareerFinanceStore Store() =>
        new(Path.Combine(_temporaryDirectory, "external-career.sqlite"));

    private static readonly DateTimeOffset FixedTimestamp =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    private static CareerFinanceState State(long cash, int year) =>
        new("club:test", year, "EUR", cash, 10_000, 4_000, 500, 2_000, 1_000, FixedTimestamp);

    private static void CreateSentinelCareerTable(string databasePath)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE career_sentinel (value TEXT NOT NULL); INSERT INTO career_sentinel VALUES ('preserved');";
        command.ExecuteNonQuery();
    }

    private static string ReadSentinel(string databasePath)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM career_sentinel;";
        return (string)command.ExecuteScalar()!;
    }
}
