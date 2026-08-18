using System.Collections.Immutable;
using PCDynasty.Contracts.Simulation;
using PCDynasty.World.State;

namespace PCDynasty.World.Seasons;

public sealed class MacroSeasonTransition
{
    public MacroSeasonTransition(
        long worldTick,
        WorldDate effectiveDate,
        IEnumerable<ClubSeasonState> nextClubSeasons)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(worldTick);
        ArgumentNullException.ThrowIfNull(nextClubSeasons);
        if (effectiveDate.Year <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveDate));
        }

        WorldTick = worldTick;
        EffectiveDate = effectiveDate;
        NextClubSeasons = nextClubSeasons.Select(state =>
        {
            ArgumentNullException.ThrowIfNull(state);
            return state;
        }).ToImmutableArray();
    }

    public long WorldTick { get; }
    public WorldDate EffectiveDate { get; }
    public ImmutableArray<ClubSeasonState> NextClubSeasons { get; }
}

public sealed class MacroSeasonFact
{
    public MacroSeasonFact(EventSequence sequence, ClubSeasonState state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Sequence = sequence;
    }

    public EventSequence Sequence { get; }
    public ClubSeasonState State { get; }
}

public sealed class MacroSeasonResult
{
    public MacroSeasonResult(
        WorldCheckpoint checkpoint,
        IEnumerable<MacroSeasonFact> facts)
    {
        Checkpoint = checkpoint ?? throw new ArgumentNullException(nameof(checkpoint));
        ArgumentNullException.ThrowIfNull(facts);
        Facts = facts.ToImmutableArray();
    }

    public WorldCheckpoint Checkpoint { get; }
    public ImmutableArray<MacroSeasonFact> Facts { get; }
}
