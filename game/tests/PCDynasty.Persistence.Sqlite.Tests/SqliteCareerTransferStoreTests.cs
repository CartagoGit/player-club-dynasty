using Microsoft.Data.Sqlite;
using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteCareerTransferStoreTests : IDisposable
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
    private readonly string _temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        $"pcdynasty-transfers-{Guid.NewGuid():N}");

    [Fact]
    public void MissingExternalDatabaseReturnsAnImmutableEmptySnapshot()
    {
        var store = Store();

        IReadOnlyList<CareerTransferRecord> records = store.ListForClub("club:buyer");

        Assert.Empty(records);
        Assert.Equal(DatabasePath, store.DatabasePath);
        Assert.IsAssignableFrom<System.Collections.Immutable.IImmutableList<CareerTransferRecord>>(records);
    }

    [Fact]
    public void SettlementRoundTripsTransferAndFinanceAcrossReopen()
    {
        CareerFinanceState expected = Finance();
        CareerFinanceState updated = Debit(expected, 2_000_000);
        FinanceStore().Save(expected);

        bool settled = Store().TrySettle(Transfer("transfer:1", "person:1", 2_000_000), expected, updated);

        Assert.True(settled);
        CareerTransferRecord loaded = Assert.Single(Store().ListForClub("club:buyer"));
        Assert.Equal("person:1", loaded.PlayerExternalId);
        Assert.Equal(2_000_000, loaded.Amount);
        Assert.True(FinanceStore().TryLoad("club:buyer", out CareerFinanceState? loadedFinance));
        Assert.Equal(updated, loadedFinance);
    }

    [Fact]
    public void StaleFinanceSnapshotRollsBackTransferAndPreservesConcurrentState()
    {
        CareerFinanceState expected = Finance();
        CareerFinanceState concurrent = Debit(expected, 1);
        FinanceStore().Save(concurrent);

        bool settled = Store().TrySettle(
            Transfer("transfer:1", "person:1", 2_000_000),
            expected,
            Debit(expected, 2_000_000));

        Assert.False(settled);
        Assert.Empty(Store().ListForClub("club:buyer"));
        Assert.True(FinanceStore().TryLoad("club:buyer", out CareerFinanceState? loadedFinance));
        Assert.Equal(concurrent, loadedFinance);
    }

    [Fact]
    public void DuplicatePlayerRollsBackSecondFinanceUpdate()
    {
        CareerFinanceState initial = Finance();
        CareerFinanceState afterFirst = Debit(initial, 1_000_000);
        CareerFinanceState afterSecond = Debit(afterFirst, 500_000);
        FinanceStore().Save(initial);
        Assert.True(Store().TrySettle(
            Transfer("transfer:1", "person:1", 1_000_000),
            initial,
            afterFirst));

        bool duplicate = Store().TrySettle(
            Transfer("transfer:2", "person:1", 500_000),
            afterFirst,
            afterSecond);

        Assert.False(duplicate);
        Assert.Single(Store().ListForClub("club:buyer"));
        Assert.True(FinanceStore().TryLoad("club:buyer", out CareerFinanceState? loadedFinance));
        Assert.Equal(afterFirst, loadedFinance);
    }

    [Fact]
    public void InvalidFinanceTransitionIsRejectedBeforeMutation()
    {
        CareerFinanceState initial = Finance();
        FinanceStore().Save(initial);

        Assert.Throws<ArgumentException>(() => Store().TrySettle(
            Transfer("transfer:1", "person:1", 1_000_000),
            initial,
            initial));

        Assert.Empty(Store().ListForClub("club:buyer"));
        Assert.True(FinanceStore().TryLoad("club:buyer", out CareerFinanceState? loadedFinance));
        Assert.Equal(initial, loadedFinance);
    }

    [Fact]
    public void ClearRemovesOnlyTransfersAndPreservesFinanceAndExternalTables()
    {
        CareerFinanceState initial = Finance();
        CareerFinanceState updated = Debit(initial, 1_000_000);
        FinanceStore().Save(initial);
        Assert.True(Store().TrySettle(
            Transfer("transfer:1", "person:1", 1_000_000),
            initial,
            updated));
        CreateSentinelTable();

        Store().Clear();

        Assert.Empty(Store().ListForClub("club:buyer"));
        Assert.True(FinanceStore().TryLoad("club:buyer", out CareerFinanceState? loadedFinance));
        Assert.Equal(updated, loadedFinance);
        Assert.Equal("preserved", ReadSentinel());
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private string DatabasePath => Path.Combine(_temporaryDirectory, "external-career.sqlite");

    private SqliteCareerTransferStore Store() => new(DatabasePath);

    private SqliteCareerFinanceStore FinanceStore() => new(DatabasePath);

    private static CareerFinanceState Finance() =>
        new("club:buyer", 2026, "EUR", 10_000_000, 20_000_000, 10_000_000, 0, 8_000_000, 4_000_000, Now);

    private static CareerFinanceState Debit(CareerFinanceState state, long amount) =>
        new(
            state.ClubExternalId,
            state.SeasonYear,
            state.Currency,
            state.Cash - amount,
            state.Revenue,
            state.Costs,
            state.Debt,
            state.TransferBudget - amount,
            state.WageBudget,
            state.UpdatedAtUtc.AddTicks(1));

    private static CareerTransferRecord Transfer(string id, string playerId, long amount) =>
        new(id, "club:buyer", "club:seller", playerId, amount, "EUR", Now.AddTicks(1));

    private void CreateSentinelTable()
    {
        using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE career_sentinel (value TEXT NOT NULL);
            INSERT INTO career_sentinel VALUES ('preserved');
            """;
        command.ExecuteNonQuery();
    }

    private string ReadSentinel()
    {
        using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM career_sentinel;";
        return (string)command.ExecuteScalar()!;
    }
}
