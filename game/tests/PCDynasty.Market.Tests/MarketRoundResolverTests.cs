using PCDynasty.Market.Negotiation;
using PCDynasty.Market.Rounds;
using PCDynasty.Market.Valuation;
using Xunit;

namespace PCDynasty.Market.Tests;

public sealed class MarketRoundResolverTests
{
    [Fact]
    public void ResolvesNeedsAndBudgetsDeterministicallyRegardlessOfInputOrder()
    {
        PlayerValuation firstSubject = Valuation("subject:1", asking: 500);
        PlayerValuation secondSubject = Valuation("subject:2", asking: 600);
        MarketBid[] bids =
        [
            new("bid:1", "club:a", firstSubject, new TransferPrice(Eur(550)), needPriority: 900),
            new("bid:2", "club:b", firstSubject, new TransferPrice(Eur(600)), needPriority: 800),
            new("bid:3", "club:a", secondSubject, new TransferPrice(Eur(600)), needPriority: 700),
            new("bid:4", "club:b", secondSubject, new TransferPrice(Eur(650)), needPriority: 600),
        ];
        var budgets = new Dictionary<string, MoneyAmount>
        {
            ["club:a"] = Eur(900),
            ["club:b"] = Eur(700),
        };

        MarketRoundResult ordered = MarketRoundResolver.Resolve(bids, budgets);
        MarketRoundResult reversed = MarketRoundResolver.Resolve(bids.Reverse(), budgets);

        Assert.Equal(Fingerprint(ordered), Fingerprint(reversed));
        Assert.Equal(
            ["bid:1", "bid:4"],
            ordered.Resolutions.Where(result => result.Accepted).Select(result => result.BidId));
        Assert.Equal(350, ordered.RemainingBudgets["club:a"].MinorUnits);
        Assert.Equal(50, ordered.RemainingBudgets["club:b"].MinorUnits);
        Assert.Equal(900, budgets["club:a"].MinorUnits);
    }

    [Fact]
    public void NegotiationRejectsAskingPriceAndCurrencyViolations()
    {
        PlayerValuation valuation = Valuation("subject:1", asking: 500);

        Assert.False(NegotiationEngine.Evaluate(
            new MarketBid(
                "bid:low",
                "club:a",
                valuation,
                new TransferPrice(Eur(499)),
                needPriority: 500),
            Eur(1_000)).Accepted);
        Assert.False(NegotiationEngine.Evaluate(
            new MarketBid(
                "bid:currency",
                "club:a",
                valuation,
                new TransferPrice(new MoneyAmount(600, "USD")),
                needPriority: 500),
            Eur(1_000)).Accepted);
    }

    private static PlayerValuation Valuation(string subjectId, long asking) =>
        new(
            subjectId,
            valuedOnDay: 100,
            new MarketPrice(Eur(450)),
            new InternalPrice(Eur(475)),
            new AskingPrice(Eur(asking)),
            new TransferPrice(Eur(asking)));

    private static MoneyAmount Eur(long amount) => new(amount, "EUR");

    private static string Fingerprint(MarketRoundResult result) => string.Join(
        ';',
        result.Resolutions.Select(resolution => string.Join(
            '|',
            resolution.BidId,
            resolution.Accepted,
            resolution.Reason))) + "#" + string.Join(
            ';',
            result.RemainingBudgets.Select(pair => $"{pair.Key}:{pair.Value.MinorUnits}"));
}
