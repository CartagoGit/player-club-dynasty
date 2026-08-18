using PCDynasty.Football.Competitions;
using PCDynasty.Rules.Versioning;
using Xunit;

namespace PCDynasty.Football.Tests;

public sealed class DivisionRuleSetTests
{
    [Fact]
    public void ResolvesFootballScoringByEffectiveRuleVersion()
    {
        var catalogue = new EffectiveRuleSetCatalogue(
        [
            Rules(version: 1, from: new DateOnly(2025, 1, 1), until: new DateOnly(2025, 12, 31)),
            Rules(version: 2, from: new DateOnly(2026, 1, 1), until: null),
        ]);

        DivisionRuleSet division = DivisionRuleSet.Resolve(
            catalogue,
            "rules:football:division-alpha",
            version: 2,
            effectiveOn: new DateOnly(2026, 8, 11),
            new FootballScoringRules(winPoints: 3, drawPoints: 1, lossPoints: 0),
            participantCount: 20,
            promotionPlaces: 2,
            relegationPlaces: 3);
        FootballPointsAward award = division.Scoring.Award(homeScore: 2, awayScore: 1);

        Assert.Equal(2, division.Version);
        Assert.Equal(3, award.HomePoints);
        Assert.Equal(0, award.AwayPoints);
    }

    [Fact]
    public void RefusesAnInactiveVersionInsteadOfSubstitutingIt()
    {
        var catalogue = new EffectiveRuleSetCatalogue(
        [
            Rules(version: 1, from: new DateOnly(2025, 1, 1), until: new DateOnly(2025, 12, 31)),
            Rules(version: 2, from: new DateOnly(2026, 1, 1), until: null),
        ]);

        Assert.Throws<KeyNotFoundException>(() => DivisionRuleSet.Resolve(
            catalogue,
            "rules:football:division-alpha",
            version: 1,
            effectiveOn: new DateOnly(2026, 8, 11),
            new FootballScoringRules(3, 1, 0),
            20,
            2,
            3));
    }

    [Fact]
    public void ValidatesScoringAndSeasonBoundaries()
    {
        Assert.Throws<ArgumentException>(() => new FootballScoringRules(1, 3, 0));

        var catalogue = new EffectiveRuleSetCatalogue([Rules(1, new DateOnly(2026, 1, 1), null)]);
        Assert.Throws<ArgumentException>(() => DivisionRuleSet.Resolve(
            catalogue,
            "rules:football:division-alpha",
            1,
            new DateOnly(2026, 8, 11),
            new FootballScoringRules(3, 1, 0),
            participantCount: 4,
            promotionPlaces: 2,
            relegationPlaces: 3));
    }

    private static EffectiveRuleSet Rules(int version, DateOnly from, DateOnly? until)
    {
        return new EffectiveRuleSet(
            "rules:football:division-alpha",
            version,
            from,
            until,
            ["rule:football:points", "rule:football:movement"]);
    }
}
