using PCDynasty.Contracts.Simulation;
using PCDynasty.World.Seasons;
using PCDynasty.World.State;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class MacroSeasonTransitionTests
{
    [Fact]
    public void AdvanceEmitsProtocolOrderedFactsIndependentlyOfInputOrder()
    {
        var current = new WorldCheckpoint(
            schemaVersion: 3,
            sequence: 7,
            new WorldDate(2030, 6, 30),
            Array.Empty<ClubSeasonState>());
        ClubSeasonState first = State("club:a", "season:2030", 11);
        ClubSeasonState second = State("club:b", "season:2030", 7);
        MacroSeasonResult ordered = MacroSeasonScheduler.Advance(
            current,
            new MacroSeasonTransition(
                worldTick: 900,
                new WorldDate(2030, 7, 1),
                new[] { first, second }));
        MacroSeasonResult reversed = MacroSeasonScheduler.Advance(
            current,
            new MacroSeasonTransition(
                worldTick: 900,
                new WorldDate(2030, 7, 1),
                new[] { second, first }));

        Assert.Equal(8, ordered.Checkpoint.Sequence);
        Assert.Equal(new WorldDate(2030, 7, 1), ordered.Checkpoint.Date);
        Assert.Collection(
            ordered.Facts,
            fact => Assert.Equal("club:a", fact.State.ClubId),
            fact => Assert.Equal("club:b", fact.State.ClubId));
        Assert.All(ordered.Facts, fact => Assert.IsType<EventSequence>(fact.Sequence));
        Assert.Equal(
            ordered.Facts.Select(Fingerprint),
            reversed.Facts.Select(Fingerprint));
        Assert.True(ordered.Facts[0].Sequence < ordered.Facts[1].Sequence);
    }

    [Fact]
    public void AdvanceRejectsNonForwardDatesAndRegressingTicks()
    {
        var current = new WorldCheckpoint(
            schemaVersion: 1,
            sequence: 12,
            new WorldDate(2030, 7, 1),
            Array.Empty<ClubSeasonState>());
        Assert.Throws<ArgumentOutOfRangeException>(() => MacroSeasonScheduler.Advance(
            current,
            new MacroSeasonTransition(
                worldTick: 11,
                new WorldDate(2030, 7, 2),
                Array.Empty<ClubSeasonState>())));
        Assert.Throws<ArgumentOutOfRangeException>(() => MacroSeasonScheduler.Advance(
            current,
            new MacroSeasonTransition(
                worldTick: 13,
                new WorldDate(2030, 7, 1),
                Array.Empty<ClubSeasonState>())));
    }

    private static ClubSeasonState State(string clubId, string seasonId, long rating) =>
        new(
            clubId,
            seasonId,
            "competition:open",
            new[] { KeyValuePair.Create("rating", rating) });

    private static string Fingerprint(MacroSeasonFact fact) => string.Join(
        '|',
        fact.Sequence.WorldTick,
        fact.Sequence.Phase,
        fact.Sequence.EmitterId,
        fact.Sequence.LocalSequence,
        fact.State.ClubId,
        fact.State.SeasonId,
        fact.State.CompetitionId);
}
