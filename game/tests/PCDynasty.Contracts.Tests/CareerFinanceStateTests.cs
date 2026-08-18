using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class CareerFinanceStateTests
{
    [Fact]
    public void NormalizesCurrencyAndTimestampWithoutMutableMembers()
    {
        var state = new CareerFinanceState(
            "club:test",
            2026,
            "eur",
            10_000,
            8_000,
            3_000,
            2_000,
            4_000,
            1_000,
            new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.FromHours(2)));

        Assert.Equal("EUR", state.Currency);
        Assert.Equal(TimeSpan.Zero, state.UpdatedAtUtc.Offset);
        Assert.DoesNotContain(
            typeof(CareerFinanceState).GetProperties(),
            property => property.SetMethod?.IsPublic == true);
    }

    [Fact]
    public void RejectsInvalidExternalValues()
    {
        Assert.Throws<ArgumentException>(() => Create(clubId: " "));
        Assert.Throws<ArgumentException>(() => Create(currency: "EU"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(debt: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(transferBudget: -1));
    }

    private static CareerFinanceState Create(
        string clubId = "club:test",
        string currency = "EUR",
        long debt = 0,
        long transferBudget = 1) =>
        new(clubId, 2026, currency, 1, 1, 1, debt, transferBudget, 1, DateTimeOffset.UnixEpoch);
}
