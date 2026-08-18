using PCDynasty.Rules.Versioning;
using Xunit;

namespace PCDynasty.Rules.Tests;

public sealed class EffectiveRuleSetTests
{
    [Fact]
    public void ResolvesByExplicitIdentifierVersionAndDate()
    {
        var catalogue = new EffectiveRuleSetCatalogue(
        [
            new EffectiveRuleSet(
                "rules:competition:alpha",
                version: 1,
                effectiveFrom: new DateOnly(2026, 1, 1),
                effectiveUntil: new DateOnly(2026, 6, 30),
                ruleIds: ["rule:ranking:v1"]),
            new EffectiveRuleSet(
                "rules:competition:alpha",
                version: 2,
                effectiveFrom: new DateOnly(2026, 7, 1),
                effectiveUntil: null,
                ruleIds: ["rule:ranking:v2", "rule:tiebreak:v1"]),
        ]);

        EffectiveRuleSet selected = catalogue.Resolve(
            "rules:competition:alpha",
            version: 2,
            effectiveOn: new DateOnly(2026, 8, 11));

        Assert.Equal(2, selected.Version);
        Assert.Equal(["rule:ranking:v2", "rule:tiebreak:v1"], selected.RuleIds.AsSpan().ToArray());
    }

    [Fact]
    public void DoesNotSilentlySubstituteAnotherVersion()
    {
        var catalogue = new EffectiveRuleSetCatalogue(
        [
            new EffectiveRuleSet(
                "rules:competition:alpha",
                1,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 6, 30),
                ["rule:ranking:v1"]),
            new EffectiveRuleSet(
                "rules:competition:alpha",
                2,
                new DateOnly(2026, 7, 1),
                null,
                ["rule:ranking:v2"]),
        ]);

        Assert.Throws<KeyNotFoundException>(() => catalogue.Resolve(
            "rules:competition:alpha",
            version: 1,
            effectiveOn: new DateOnly(2026, 8, 11)));
    }

    [Fact]
    public void CopiesRulesAndRejectsAmbiguousVersions()
    {
        var sourceRules = new[] { "rule:ranking:v1" };
        var rules = new EffectiveRuleSet(
            "rules:competition:alpha",
            1,
            new DateOnly(2026, 1, 1),
            null,
            sourceRules);

        sourceRules[0] = "changed";

        Assert.Equal("rule:ranking:v1", rules.RuleIds[0]);
        Assert.Throws<ArgumentException>(() => new EffectiveRuleSetCatalogue([rules, rules]));
    }

    [Fact]
    public void RejectsInvalidEffectiveRangesAndVersions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EffectiveRuleSet(
            "rules:competition:alpha",
            0,
            new DateOnly(2026, 1, 1),
            null,
            ["rule:ranking:v1"]));
        Assert.Throws<ArgumentException>(() => new EffectiveRuleSet(
            "rules:competition:alpha",
            1,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 6, 30),
            ["rule:ranking:v1"]));
    }
}
