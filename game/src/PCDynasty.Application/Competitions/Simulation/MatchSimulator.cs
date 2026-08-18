using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Core.Contests;
using PCDynasty.Simulation.Contests;

namespace PCDynasty.Application.Competitions.Simulation;

/// <summary>
/// Stateless helper that turns two <see cref="ClubRow"/>s into a deterministic
/// match by projecting the engine-neutral <see cref="ContestSimulator"/>.
///
/// The match is reduced to a sequence of <see cref="MatchEvent"/>s the UI can
/// render (goals, attempts, key moments) plus a final score split by club.
/// </summary>
public static class MatchSimulator
{
    /// <summary>
    /// Football matches run for 90 logical minute ticks, which keeps event
    /// ordering human-readable for every presentation adapter.
    /// </summary>
    public const int MatchDurationMinutes = 90;

    /// <summary>
    /// Number of "contest events" generated per match. ~5x the expected goals
    /// for a low-scoring sport (matches typically end 0-3) keeps the density
    /// meaningful without spamming the UI.
    /// </summary>
    public const int DefaultEventCount = 40;

    public static MatchResult Simulate(
        ClubRow home,
        ClubRow away,
        long seed,
        MatchSimulatorOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(home);
        ArgumentNullException.ThrowIfNull(away);
        if (string.Equals(home.ExternalId, away.ExternalId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A match needs two distinct clubs.",
                nameof(away));
        }

        MatchSimulatorOptions resolved = options ?? MatchSimulatorOptions.Default;
        var snapshot = new ContestSnapshot(
            contestId: $"match:{home.ExternalId}-vs-{away.ExternalId}",
            rulesetId: "rules:football-standard-scoring",
            seed: seed,
            participants: new[]
            {
                new ContestParticipantSnapshot(
                    participantId: home.ExternalId,
                    attributes: BuildAttributes(home),
                    form: Math.Clamp(home.Prestige / 2, 0, 100)),
                new ContestParticipantSnapshot(
                    participantId: away.ExternalId,
                    attributes: BuildAttributes(away),
                    form: Math.Clamp(away.Prestige / 2, 0, 100)),
            });

        ContestSimulationResult result = ContestSimulator.Run(snapshot, new ContestSimulationOptions(
            durationTicks: MatchDurationMinutes,
            eventCount: resolved.EventCount,
            kindId: "football.attempt",
            maximumMagnitude: resolved.MaximumEventMagnitude));

        var events = result.Events
            .Select(contest => ProjectEvent(contest, home, away))
            .ToImmutableArray();

        int homeGoals = CountGoals(events, home.ExternalId);
        int awayGoals = CountGoals(events, away.ExternalId);

        return new MatchResult(
            HomeClub: home,
            AwayClub: away,
            HomeGoals: homeGoals,
            AwayGoals: awayGoals,
            Seed: seed,
            Events: events);
    }

    private static ImmutableArray<int> BuildAttributes(ClubRow club)
    {
        // The contest simulator weighs participants by the average of their
        // declared attributes plus form. We surface four facets so the
        // distribution is meaningful: prestige, fanbase (engagement),
        // squad size (depth) and a synthetic discipline derived from
        // squad size (more players => more chances, capped).
        int discipline = Math.Clamp(50 + (club.SquadSize * 2), 0, 100);
        return ImmutableArray.Create(
            club.Prestige,
            club.Fanbase,
            club.SquadSize * 4,
            discipline);
    }

    private static MatchEvent ProjectEvent(ContestEvent contest, ClubRow home, ClubRow away)
    {
        bool isHome = string.Equals(contest.ActorId, home.ExternalId, StringComparison.Ordinal);
        ClubRow actor = isHome ? home : away;
        bool isGoal = contest.Magnitude >= 5;

        string description = isGoal
            ? $"¡GOL! {actor.ShortName} marca en el minuto {contest.Tick}'"
            : $"{actor.ShortName} · intento de magnitud {contest.Magnitude} en el minuto {contest.Tick}'";

        return new MatchEvent(
            Sequence: contest.Sequence,
            Minute: contest.Tick,
            ActorClubId: actor.ExternalId,
            ActorShortName: actor.ShortName,
            IsHomeActor: isHome,
            IsGoal: isGoal,
            Magnitude: contest.Magnitude,
            Description: description);
    }

    private static int CountGoals(ImmutableArray<MatchEvent> events, string clubExternalId)
    {
        int count = 0;
        foreach (MatchEvent matchEvent in events)
        {
            if (matchEvent.IsGoal
                && string.Equals(matchEvent.ActorClubId, clubExternalId, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }
}

public sealed record MatchSimulatorOptions(int EventCount, int MaximumEventMagnitude)
{
    public static MatchSimulatorOptions Default { get; } = new(
        EventCount: MatchSimulator.DefaultEventCount,
        MaximumEventMagnitude: 8);
}

/// <summary>
/// One concrete moment in the match. The presentation renders each row as a
/// tick on the timeline.
/// </summary>
public sealed record MatchEvent(
    long Sequence,
    int Minute,
    string ActorClubId,
    string ActorShortName,
    bool IsHomeActor,
    bool IsGoal,
    int Magnitude,
    string Description)
{
    /// <summary>
    /// Coarse-grained event type derived from <see cref="IsGoal"/> and
    /// <see cref="Magnitude"/> so the UI can colour and group them
    /// without owning a parallel taxonomy.
    /// </summary>
    public MatchEventKind Kind => Classify(IsGoal, Magnitude);

    private static MatchEventKind Classify(bool isGoal, int magnitude) => isGoal
        ? MatchEventKind.Goal
        : magnitude switch
        {
            >= 80 => MatchEventKind.Save,
            >= 60 => MatchEventKind.Shot,
            >= 40 => MatchEventKind.Card,
            _ => MatchEventKind.Miss,
        };
}

public enum MatchEventKind
{
    Goal,
    Shot,
    Save,
    Miss,
    Card,
}

/// <summary>
/// Final outcome of a simulated match, including the full event timeline so
/// the UI can render the minute-by-minute log.
/// </summary>
public sealed record MatchResult(
    ClubRow HomeClub,
    ClubRow AwayClub,
    int HomeGoals,
    int AwayGoals,
    long Seed,
    IReadOnlyList<MatchEvent> Events);
