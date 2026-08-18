using System.Collections.Immutable;
using PCDynasty.Market.Negotiation;
using PCDynasty.Market.Valuation;

namespace PCDynasty.Market.Rounds;

public sealed class MarketRoundResult
{
    public MarketRoundResult(
        IEnumerable<NegotiationResolution> resolutions,
        IEnumerable<KeyValuePair<string, MoneyAmount>> remainingBudgets)
    {
        ArgumentNullException.ThrowIfNull(resolutions);
        ArgumentNullException.ThrowIfNull(remainingBudgets);
        Resolutions = resolutions.OrderBy(
            resolution => resolution.BidId,
            StringComparer.Ordinal).ToImmutableArray();
        RemainingBudgets = remainingBudgets.ToImmutableSortedDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
    }

    public ImmutableArray<NegotiationResolution> Resolutions { get; }
    public ImmutableSortedDictionary<string, MoneyAmount> RemainingBudgets { get; }
}

public static class MarketRoundResolver
{
    public static MarketRoundResult Resolve(
        IEnumerable<MarketBid> bids,
        IEnumerable<KeyValuePair<string, MoneyAmount>> budgets)
    {
        ArgumentNullException.ThrowIfNull(bids);
        ArgumentNullException.ThrowIfNull(budgets);
        MarketBid[] orderedBids = bids.Select(bid =>
        {
            ArgumentNullException.ThrowIfNull(bid);
            return bid;
        }).OrderByDescending(bid => bid.NeedPriority)
          .ThenByDescending(bid => bid.OfferedPrice.Amount.MinorUnits)
          .ThenBy(bid => bid.BuyerClubId, StringComparer.Ordinal)
          .ThenBy(bid => bid.Valuation.SubjectId, StringComparer.Ordinal)
          .ThenBy(bid => bid.BidId, StringComparer.Ordinal)
          .ToArray();
        if (orderedBids.Select(bid => bid.BidId)
                .Distinct(StringComparer.Ordinal).Count() != orderedBids.Length)
        {
            throw new ArgumentException("Bid identifiers must be unique.", nameof(bids));
        }

        var remaining = new Dictionary<string, MoneyAmount>(StringComparer.Ordinal);
        foreach ((string buyerId, MoneyAmount budget) in budgets)
        {
            string normalizedBuyer = NegotiationValue.Required(buyerId, nameof(budgets));
            if (string.IsNullOrWhiteSpace(budget.Currency) || !remaining.TryAdd(normalizedBuyer, budget))
            {
                throw new ArgumentException("Budgets must have unique buyers and valid money.", nameof(budgets));
            }
        }

        var resolvedSubjects = new HashSet<string>(StringComparer.Ordinal);
        var resolutions = ImmutableArray.CreateBuilder<NegotiationResolution>(orderedBids.Length);
        foreach (MarketBid bid in orderedBids)
        {
            if (resolvedSubjects.Contains(bid.Valuation.SubjectId))
            {
                resolutions.Add(NegotiationEngine.Rejected(bid, "subject-resolved"));
                continue;
            }

            if (!remaining.TryGetValue(bid.BuyerClubId, out MoneyAmount budget))
            {
                resolutions.Add(NegotiationEngine.Rejected(bid, "budget-unavailable"));
                continue;
            }

            NegotiationResolution resolution = NegotiationEngine.Evaluate(bid, budget);
            resolutions.Add(resolution);
            if (!resolution.Accepted)
            {
                continue;
            }

            resolvedSubjects.Add(bid.Valuation.SubjectId);
            remaining[bid.BuyerClubId] = new MoneyAmount(
                budget.MinorUnits - bid.OfferedPrice.Amount.MinorUnits,
                budget.Currency);
        }

        return new MarketRoundResult(resolutions, remaining);
    }
}
