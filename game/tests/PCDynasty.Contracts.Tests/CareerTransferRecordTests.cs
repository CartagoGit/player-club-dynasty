using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class CareerTransferRecordTests
{
    [Fact]
    public void NormalizesExternalValuesAndTimestamp()
    {
        var completed = new DateTimeOffset(2026, 8, 18, 14, 0, 0, TimeSpan.FromHours(2));

        var transfer = new CareerTransferRecord(
            " transfer:1 ",
            " club:buyer ",
            " club:seller ",
            " person:1 ",
            2_500_000,
            " eur ",
            completed);

        Assert.Equal("transfer:1", transfer.TransferId);
        Assert.Equal("club:buyer", transfer.BuyerClubExternalId);
        Assert.Equal("club:seller", transfer.SellerClubExternalId);
        Assert.Equal("person:1", transfer.PlayerExternalId);
        Assert.Equal("EUR", transfer.Currency);
        Assert.Equal(TimeSpan.Zero, transfer.CompletedAtUtc.Offset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveAmount(long amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Transfer(amount: amount));
    }

    [Fact]
    public void RejectsSameBuyerAndSeller()
    {
        Assert.Throws<ArgumentException>(() => new CareerTransferRecord(
            "transfer:1",
            "club:same",
            "club:same",
            "person:1",
            1,
            "EUR",
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RejectsInvalidCurrency()
    {
        Assert.Throws<ArgumentException>(() => Transfer(currency: "EU"));
        Assert.Throws<ArgumentException>(() => Transfer(currency: "1$!"));
    }

    private static CareerTransferRecord Transfer(long amount = 1, string currency = "EUR") =>
        new(
            "transfer:1",
            "club:buyer",
            "club:seller",
            "person:1",
            amount,
            currency,
            DateTimeOffset.UtcNow);
}
