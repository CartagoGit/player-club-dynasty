using PCDynasty.Competition.Rankings;
using PCDynasty.Football.Competitions;
using PCDynasty.Rules.Versioning;
using Xunit;

namespace PCDynasty.Football.Tests;

public sealed class PromotionRelegationResolverTests
{
    [Fact]
    public void ThousandsOfSeasonsPreserveEveryParticipantExactlyOnce()
    {
        DivisionRuleSet rules = CreateRules(participants: 20, promoted: 2, relegated: 3);

        for (int season = 0; season < 2_000; season++)
        {
            RankingTable table = CreateTable(season, rules.ParticipantCount);

            SeasonClosure closure = PromotionRelegationResolver.Resolve(table, rules);

            Assert.Equal(2, closure.Promoted.Length);
            Assert.Equal(3, closure.Relegated.Length);
            Assert.Equal(15, closure.Remaining.Length);
            string[] allParticipants = closure.Promoted
                .Concat(closure.Remaining)
                .Concat(closure.Relegated)
                .Select(outcome => outcome.ParticipantId)
                .ToArray();
            Assert.Equal(rules.ParticipantCount, allParticipants.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(rules.ParticipantCount, allParticipants.Length);
        }
    }

    [Fact]
    public void RejectsARankingWithTheWrongParticipantCardinality()
    {
        DivisionRuleSet rules = CreateRules(participants: 4, promoted: 1, relegated: 1);
        RankingTable table = CreateTable(season: 1, participantCount: 3);

        Assert.Throws<ArgumentException>(() => PromotionRelegationResolver.Resolve(table, rules));
    }

    private static RankingTable CreateTable(int season, int participantCount)
    {
        return new RankingTable(
            Enumerable.Range(0, participantCount)
                .Select(index => new RankingCandidate(
                    $"participant:{index}",
                    [((index * 17) + season) % participantCount])),
            [RankingCriterionDirection.Descending]);
    }

    private static DivisionRuleSet CreateRules(int participants, int promoted, int relegated)
    {
        var catalogue = new EffectiveRuleSetCatalogue(
        [
            new EffectiveRuleSet(
                "rules:football:division-alpha",
                1,
                new DateOnly(2026, 1, 1),
                null,
                ["rule:football:points", "rule:football:movement"]),
        ]);

        return DivisionRuleSet.Resolve(
            catalogue,
            "rules:football:division-alpha",
            1,
            new DateOnly(2026, 8, 11),
            new FootballScoringRules(3, 1, 0),
            participants,
            promoted,
            relegated);
    }
}
