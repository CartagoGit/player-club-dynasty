using System.Collections.Immutable;
using PCDynasty.Competition.Outcomes;
using PCDynasty.Competition.Rankings;

namespace PCDynasty.Football.Competitions;

public static class PromotionRelegationResolver
{
    public static SeasonClosure Resolve(RankingTable table, DivisionRuleSet rules)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(rules);
        if (table.Entries.Length != rules.ParticipantCount)
        {
            throw new ArgumentException(
                "The ranking cardinality must match the effective division rules.",
                nameof(table));
        }

        ImmutableArray<QualificationOutcome> promoted = table.Entries
            .Take(rules.PromotionPlaces)
            .Select(entry => Outcome(entry, "football:promoted"))
            .ToImmutableArray();
        ImmutableArray<QualificationOutcome> relegated = table.Entries
            .TakeLast(rules.RelegationPlaces)
            .Select(entry => Outcome(entry, "football:relegated"))
            .ToImmutableArray();
        ImmutableArray<QualificationOutcome> remaining = table.Entries
            .Skip(rules.PromotionPlaces)
            .Take(rules.ParticipantCount - rules.PromotionPlaces - rules.RelegationPlaces)
            .Select(entry => Outcome(entry, "football:retained"))
            .ToImmutableArray();

        return new SeasonClosure(promoted, remaining, relegated);
    }

    private static QualificationOutcome Outcome(RankingEntry entry, string outcomeId)
    {
        return new QualificationOutcome(entry.ParticipantId, outcomeId, destinationStageId: null);
    }
}
