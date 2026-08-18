using PCDynasty.Contracts.Simulation;
using PCDynasty.World.State;

namespace PCDynasty.World.Seasons;

public static class MacroSeasonScheduler
{
    public static MacroSeasonResult Advance(
        WorldCheckpoint current,
        MacroSeasonTransition transition)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(transition);
        if (transition.WorldTick <= current.Sequence)
        {
            throw new ArgumentOutOfRangeException(
                nameof(transition),
                "The transition tick must be after the current checkpoint sequence.");
        }

        if (transition.EffectiveDate.ToDateOnly() <= current.Date.ToDateOnly())
        {
            throw new ArgumentOutOfRangeException(
                nameof(transition),
                "The transition date must advance world time.");
        }

        var checkpoint = new WorldCheckpoint(
            current.SchemaVersion,
            transition.WorldTick,
            transition.EffectiveDate,
            transition.NextClubSeasons);
        MacroSeasonFact[] facts = checkpoint.ClubSeasons
            .Select(state => new MacroSeasonFact(
                new EventSequence(
                    transition.WorldTick,
                    phase: 0,
                    EmitterId(state),
                    localSequence: 0),
                state))
            .OrderBy(fact => fact.Sequence)
            .ToArray();

        return new MacroSeasonResult(checkpoint, facts);
    }

    private static string EmitterId(ClubSeasonState state) =>
        $"macro:{state.ClubId}:{state.SeasonId}:{state.CompetitionId}";
}
