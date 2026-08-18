using System.Collections.Immutable;

namespace PCDynasty.Rules.Versioning;

public sealed class EffectiveRuleSet
{
    public EffectiveRuleSet(
        string ruleSetId,
        int version,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        IEnumerable<string> ruleIds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentNullException.ThrowIfNull(ruleIds);

        if (effectiveUntil < effectiveFrom)
        {
            throw new ArgumentException(
                "The effective end date cannot precede the start date.",
                nameof(effectiveUntil));
        }

        ImmutableArray<string> ruleCopy = ruleIds
            .Select((ruleId, index) => RuleIdentifier.Normalize(ruleId, $"{nameof(ruleIds)}[{index}]"))
            .ToImmutableArray();
        if (ruleCopy.IsEmpty)
        {
            throw new ArgumentException("A rule set requires at least one rule.", nameof(ruleIds));
        }

        if (ruleCopy.Distinct(StringComparer.Ordinal).Count() != ruleCopy.Length)
        {
            throw new ArgumentException("Rule identifiers must be unique.", nameof(ruleIds));
        }

        RuleSetId = RuleIdentifier.Normalize(ruleSetId, nameof(ruleSetId));
        Version = version;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        RuleIds = ruleCopy;
    }

    public string RuleSetId { get; }
    public int Version { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly? EffectiveUntil { get; }
    public ImmutableArray<string> RuleIds { get; }

    public bool IsEffectiveOn(DateOnly date)
    {
        return date >= EffectiveFrom && (EffectiveUntil is null || date <= EffectiveUntil.Value);
    }
}

public sealed class EffectiveRuleSetCatalogue
{
    private readonly ImmutableDictionary<(string RuleSetId, int Version), EffectiveRuleSet> versions;

    public EffectiveRuleSetCatalogue(IEnumerable<EffectiveRuleSet> ruleSets)
    {
        ArgumentNullException.ThrowIfNull(ruleSets);

        var builder = ImmutableDictionary.CreateBuilder<
            (string RuleSetId, int Version),
            EffectiveRuleSet>();
        foreach (EffectiveRuleSet ruleSet in ruleSets)
        {
            ArgumentNullException.ThrowIfNull(ruleSet);
            if (!builder.TryAdd((ruleSet.RuleSetId, ruleSet.Version), ruleSet))
            {
                throw new ArgumentException(
                    "A rule-set identifier and version pair must be unique.",
                    nameof(ruleSets));
            }
        }

        versions = builder.ToImmutable();
    }

    public EffectiveRuleSet Resolve(string ruleSetId, int version, DateOnly effectiveOn)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        string normalizedId = RuleIdentifier.Normalize(ruleSetId, nameof(ruleSetId));

        if (!versions.TryGetValue((normalizedId, version), out EffectiveRuleSet? ruleSet) ||
            !ruleSet.IsEffectiveOn(effectiveOn))
        {
            throw new KeyNotFoundException(
                $"No effective rule set exists for '{normalizedId}', version {version}, on {effectiveOn:yyyy-MM-dd}.");
        }

        return ruleSet;
    }
}

internal static class RuleIdentifier
{
    public static string Normalize(string identifier, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifier, parameterName);
        string normalized = identifier.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("An identifier is required.", parameterName);
    }
}
