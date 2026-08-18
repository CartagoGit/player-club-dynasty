using PCDynasty.Simulation.Teams;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class ChemistryAdvancerTests
{
    [Fact]
    public void DurableEventsAdvanceChemistryWithExplainableFactors()
    {
        TeamChemistry initial = TeamChemistry.Create(
            "team:7",
            cohesion: 45,
            trust: 50,
            familiarity: 35);
        TeamChemistryEvent[] events =
        [
            new(2, new DateOnly(2026, 1, 8), ChemistryEventKind.RosterContinuity, 12, "week:2"),
            new(1, new DateOnly(2026, 1, 3), ChemistryEventKind.SharedSuccess, 10, "contest:1"),
        ];

        TeamChemistry advanced = ChemistryAdvancer.Advance(initial, events);

        Assert.True(advanced.Cohesion > initial.Cohesion);
        Assert.True(advanced.Trust > initial.Trust);
        Assert.True(advanced.Familiarity > initial.Familiarity);
        Assert.Equal(2, advanced.LastAppliedSequence);
        Assert.Equal(2, advanced.Factors.Length);
        Assert.Contains(
            advanced.Factors,
            factor => factor.Kind == ChemistryEventKind.RosterContinuity &&
                      factor.Explanation.Contains("familiarity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ConflictReducesTrustAndIsNotAHiddenPlayerAttribute()
    {
        TeamChemistry initial = TeamChemistry.Create("team:7", 70, 75, 65);
        TeamChemistry advanced = ChemistryAdvancer.Advance(
            initial,
            [
                new TeamChemistryEvent(
                    1,
                    new DateOnly(2026, 1, 4),
                    ChemistryEventKind.InternalConflict,
                    20,
                    "incident:4"),
            ]);

        Assert.True(advanced.Trust < initial.Trust);
        Assert.True(advanced.Cohesion < initial.Cohesion);
        Assert.Contains(
            advanced.Factors,
            factor => factor.SourceId == "incident:4" && factor.TrustDelta < 0);
        Assert.DoesNotContain(
            typeof(TeamChemistry).GetProperties(),
            property => property.Name.Contains("Player", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EventSequenceMakesAdvancementDeterministicAndIdempotentAtTheBoundary()
    {
        TeamChemistry initial = TeamChemistry.Create("team:7", 50, 50, 50);
        var first = new TeamChemistryEvent(
            1,
            new DateOnly(2026, 1, 4),
            ChemistryEventKind.SharedSuccess,
            8,
            "contest:1");

        TeamChemistry advanced = ChemistryAdvancer.Advance(initial, [first]);
        TeamChemistry repeatedFromSameState = ChemistryAdvancer.Advance(initial, [first]);

        Assert.Equal(advanced.Cohesion, repeatedFromSameState.Cohesion);
        Assert.Equal(advanced.Trust, repeatedFromSameState.Trust);
        Assert.Throws<ArgumentException>(() => ChemistryAdvancer.Advance(
            advanced,
            [first]));
        Assert.Throws<ArgumentException>(() => ChemistryAdvancer.Advance(
            initial,
            [first, first]));
    }
}
