using PCDynasty.Core.Contests;
using PCDynasty.Simulation.Contests;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class ContestSimulatorTests
{
    private static readonly ContestSimulationOptions Options = new(
        durationTicks: 90,
        eventCount: 24,
        kindId: "contest:opportunity",
        maximumMagnitude: 3);

    [Fact]
    public void SameSnapshotAndSeedProduceTheSameOrderedEvents()
    {
        ContestSnapshot snapshot = CreateSnapshot(seed: 741L);

        ContestSimulationResult first = ContestSimulator.Run(snapshot, Options);
        ContestSimulationResult second = ContestSimulator.Run(snapshot, Options);

        Assert.Equal(EventValues(first), EventValues(second));
        Assert.Equal(Enumerable.Range(0, Options.EventCount), first.Events.Select(item => item.Sequence));
        Assert.Equal(first.Events.OrderBy(item => item.Tick).Select(item => item.Tick),
            first.Events.Select(item => item.Tick));
    }

    [Fact]
    public void ASeedChangeProducesAnotherTimeline()
    {
        ContestSimulationResult first = ContestSimulator.Run(CreateSnapshot(seed: 741L), Options);
        ContestSimulationResult second = ContestSimulator.Run(CreateSnapshot(seed: 742L), Options);

        Assert.False(EventValues(first).SequenceEqual(EventValues(second)));
    }

    [Fact]
    public void EventsOnlyReferenceSnapshotParticipants()
    {
        ContestSnapshot snapshot = CreateSnapshot(seed: 741L);
        string[] participantIds = snapshot.Participants
            .Select(participant => participant.ParticipantId)
            .ToArray();

        ContestSimulationResult result = ContestSimulator.Run(snapshot, Options);

        Assert.All(result.Events, item => Assert.Contains(item.ActorId, participantIds));
        Assert.All(result.Events, item => Assert.InRange(item.Magnitude, 1, Options.MaximumMagnitude));
    }

    [Fact]
    public void RejectsOptionsThatCannotProduceABoundedTimeline()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContestSimulationOptions(0, 24, "contest:opportunity", 3));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContestSimulationOptions(90, -1, "contest:opportunity", 3));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContestSimulationOptions(90, 24, "contest:opportunity", 0));
    }

    private static ContestSnapshot CreateSnapshot(long seed)
    {
        return new ContestSnapshot(
            "contest:2026:42",
            "rules:test-sport:v1",
            seed,
            [
                new ContestParticipantSnapshot("participant:home", [80, 75, 70], form: 72),
                new ContestParticipantSnapshot("participant:away", [65, 60, 70], form: 68),
            ]);
    }

    private static IEnumerable<(int Sequence, int Tick, string Kind, string? Actor, int Magnitude)>
        EventValues(ContestSimulationResult result)
    {
        return result.Events.Select(item =>
            (item.Sequence, item.Tick, item.KindId, item.ActorId, item.Magnitude));
    }
}
