using PCDynasty.Economy.Ledgers;
using Xunit;

namespace PCDynasty.Economy.Tests;

public sealed class ClubLedgerTests
{
    [Fact]
    public void DoubleEntryPostingConservesMoneyAndPreservesOwnership()
    {
        var ledger = new ClubLedger(
            "club:7",
            "EUR",
            [
                new LedgerAccount("cash", "club:7", LedgerAccountKind.Cash),
                new LedgerAccount("revenue", "club:7", LedgerAccountKind.Revenue),
                new LedgerAccount("cost", "club:7", LedgerAccountKind.Cost),
                new LedgerAccount("debt", "club:7", LedgerAccountKind.Debt),
                new LedgerAccount("budget", "club:7", LedgerAccountKind.Budget),
            ],
            []);

        ClubLedger posted = ledger
            .Post(new LedgerTransaction("tx:revenue", 1, "cash", "revenue", 1_000, "EUR"))
            .Post(new LedgerTransaction("tx:cost", 2, "cost", "cash", 200, "EUR"))
            .Post(new LedgerTransaction("tx:debt", 3, "cash", "debt", 500, "EUR"))
            .Post(new LedgerTransaction("tx:budget", 4, "budget", "cash", 100, "EUR"));
        AnnualClubFinancialState state = posted.Snapshot(year: 2040);

        Assert.Empty(ledger.Transactions);
        Assert.Equal(0, posted.Accounts.Sum(account => posted.Balance(account.AccountId)));
        Assert.Equal(1_000, state.Revenue);
        Assert.Equal(200, state.Cost);
        Assert.Equal(500, state.Debt);
        Assert.Equal(100, state.Budget);
        Assert.Equal(1_200, state.Cash);
    }

    [Fact]
    public void RejectsCrossOwnerAccountsAndUnbalancedPostingShapes()
    {
        Assert.Throws<ArgumentException>(() => new ClubLedger(
            "club:7",
            "EUR",
            [new LedgerAccount("cash", "club:8", LedgerAccountKind.Cash)],
            []));
        var ledger = new ClubLedger(
            "club:7",
            "EUR",
            [
                new LedgerAccount("cash", "club:7", LedgerAccountKind.Cash),
                new LedgerAccount("revenue", "club:7", LedgerAccountKind.Revenue),
            ],
            []);

        Assert.Throws<ArgumentException>(() => ledger.Post(
            new LedgerTransaction("tx:same", 1, "cash", "cash", 100, "EUR")));
        Assert.Throws<ArgumentException>(() => ledger.Post(
            new LedgerTransaction("tx:currency", 1, "cash", "revenue", 100, "USD")));
    }
}
