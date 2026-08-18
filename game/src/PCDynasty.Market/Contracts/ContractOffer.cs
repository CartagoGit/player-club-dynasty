using System.Collections.Immutable;
using PCDynasty.Market.Valuation;

namespace PCDynasty.Market.Contracts;

public sealed class ContractOffer
{
    public ContractOffer(
        string offerId,
        string offeringClubId,
        string subjectId,
        int startsOnDay,
        int endsOnDay,
        ContractPrice guaranteedValue,
        MoneyAmount periodicCompensation,
        IEnumerable<KeyValuePair<string, string>> clauses)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startsOnDay);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(endsOnDay, startsOnDay);
        string guaranteedCurrency = MarketValue.ValidCurrency(
            guaranteedValue.Amount,
            nameof(guaranteedValue));
        string compensationCurrency = MarketValue.ValidCurrency(
            periodicCompensation,
            nameof(periodicCompensation));
        if (!string.Equals(
                guaranteedCurrency,
                compensationCurrency,
                StringComparison.Ordinal))
        {
            throw new ArgumentException("Contract values must use one currency.");
        }

        ArgumentNullException.ThrowIfNull(clauses);
        var copiedClauses = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in clauses)
        {
            string normalizedKey = MarketValue.Required(key, nameof(clauses));
            ArgumentNullException.ThrowIfNull(value);
            if (!copiedClauses.TryAdd(normalizedKey, value))
            {
                throw new ArgumentException("Clause identifiers must be unique.", nameof(clauses));
            }
        }

        OfferId = MarketValue.Required(offerId, nameof(offerId));
        OfferingClubId = MarketValue.Required(offeringClubId, nameof(offeringClubId));
        SubjectId = MarketValue.Required(subjectId, nameof(subjectId));
        StartsOnDay = startsOnDay;
        EndsOnDay = endsOnDay;
        GuaranteedValue = guaranteedValue;
        PeriodicCompensation = periodicCompensation;
        Clauses = copiedClauses.ToImmutable();
    }

    public string OfferId { get; }
    public string OfferingClubId { get; }
    public string SubjectId { get; }
    public int StartsOnDay { get; }
    public int EndsOnDay { get; }
    public ContractPrice GuaranteedValue { get; }
    public MoneyAmount PeriodicCompensation { get; }
    public ImmutableDictionary<string, string> Clauses { get; }
}
