using System.Collections.Immutable;
using PCDynasty.Core.Contests;
using PCDynasty.Football.Attributes;
using PCDynasty.Simulation.Contests;

namespace PCDynasty.Football.Simulation;

public static class FootballContestPack
{
    public const int TimelineEventCount = 36;

    private static readonly ContestSimulationOptions SimulationOptions = new(
        durationTicks: 90,
        eventCount: TimelineEventCount,
        kindId: "contest:opportunity",
        maximumMagnitude: 100);

    public static ImmutableArray<ContestEvent> Simulate(ContestSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateSnapshot(snapshot);

        ContestSimulationResult simulation = ContestSimulator.Run(snapshot, SimulationOptions);
        var footballEvents = ImmutableArray.CreateBuilder<ContestEvent>(simulation.Events.Length);

        foreach (ContestEvent contestEvent in simulation.Events)
        {
            footballEvents.Add(new ContestEvent(
                contestEvent.Sequence,
                contestEvent.Tick,
                MapEventKind(contestEvent.Magnitude),
                contestEvent.ActorId,
                contestEvent.Magnitude));
        }

        return footballEvents.ToImmutable();
    }

    private static void ValidateSnapshot(ContestSnapshot snapshot)
    {
        if (!snapshot.RulesetId.StartsWith("football:", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The Football contest adapter requires a Football ruleset.",
                nameof(snapshot));
        }

        if (snapshot.Participants.Any(participant =>
            participant.Attributes.Length != FootballAttributeCatalog.Count))
        {
            throw new ArgumentException(
                $"Every Football participant must provide all {FootballAttributeCatalog.Count} compiled attributes.",
                nameof(snapshot));
        }
    }

    private static string MapEventKind(int magnitude)
    {
        return magnitude switch
        {
            >= 92 => "football:goal",
            >= 65 => "football:attempt",
            _ => "football:progression",
        };
    }
}
