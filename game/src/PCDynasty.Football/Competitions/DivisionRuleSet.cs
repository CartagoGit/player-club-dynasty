using PCDynasty.Rules.Versioning;

namespace PCDynasty.Football.Competitions;

public sealed class DivisionRuleSet
{
    private DivisionRuleSet(
        EffectiveRuleSet rules,
        DateOnly effectiveOn,
        FootballScoringRules scoring,
        int participantCount,
        int promotionPlaces,
        int relegationPlaces)
    {
        Rules = rules;
        EffectiveOn = effectiveOn;
        Scoring = scoring;
        ParticipantCount = participantCount;
        PromotionPlaces = promotionPlaces;
        RelegationPlaces = relegationPlaces;
    }

    public EffectiveRuleSet Rules { get; }
    public string RuleSetId => Rules.RuleSetId;
    public int Version => Rules.Version;
    public DateOnly EffectiveOn { get; }
    public FootballScoringRules Scoring { get; }
    public int ParticipantCount { get; }
    public int PromotionPlaces { get; }
    public int RelegationPlaces { get; }

    public static DivisionRuleSet Resolve(
        EffectiveRuleSetCatalogue catalogue,
        string ruleSetId,
        int version,
        DateOnly effectiveOn,
        FootballScoringRules scoring,
        int participantCount,
        int promotionPlaces,
        int relegationPlaces)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(scoring);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(participantCount);
        ArgumentOutOfRangeException.ThrowIfNegative(promotionPlaces);
        ArgumentOutOfRangeException.ThrowIfNegative(relegationPlaces);
        if ((long)promotionPlaces + relegationPlaces >= participantCount)
        {
            throw new ArgumentException(
                "Promotion and relegation places must leave at least one participant in the division.");
        }

        EffectiveRuleSet rules = catalogue.Resolve(ruleSetId, version, effectiveOn);
        return new DivisionRuleSet(
            rules,
            effectiveOn,
            scoring,
            participantCount,
            promotionPlaces,
            relegationPlaces);
    }
}
