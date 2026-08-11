using PCDynasty.Core.Contests;
using Xunit;

namespace PCDynasty.Core.Tests.Contests;

public sealed class ContestSnapshotTests
{
    [Fact]
    public void CopiesSerializableSportNeutralInputAtTheBoundary()
    {
        var attributes = new[] { 70, 80, 60 };
        var participants = new[]
        {
            new ContestParticipantSnapshot("participant:home", attributes, form: 65),
            new ContestParticipantSnapshot("participant:away", [50, 55, 60], form: 72),
        };
        var snapshot = new ContestSnapshot(
            "contest:2026:42",
            "rules:test-sport:v1",
            seed: 741L,
            participants);

        attributes[0] = 0;
        participants[0] = new ContestParticipantSnapshot("participant:changed", [1], form: 1);

        Assert.Equal("contest:2026:42", snapshot.ContestId);
        Assert.Equal("rules:test-sport:v1", snapshot.RulesetId);
        Assert.Equal(741L, snapshot.Seed);
        Assert.Equal("participant:home", snapshot.Participants[0].ParticipantId);
        Assert.Equal(70, snapshot.Participants[0].Attributes[0]);
        Assert.All(
            typeof(ContestSnapshot).GetProperties()
                .Concat(typeof(ContestParticipantSnapshot).GetProperties())
                .Concat(typeof(ContestEvent).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void CanBeRehydratedThroughPrimitivePortData()
    {
        var original = new ContestSnapshot(
            "contest:2026:42",
            "rules:test-sport:v1",
            seed: 741L,
            [new ContestParticipantSnapshot("participant:home", [70, 80, 60], form: 65)]);

        var rehydrated = new ContestSnapshot(
            original.ContestId,
            original.RulesetId,
            original.Seed,
            original.Participants.Select(participant => new ContestParticipantSnapshot(
                participant.ParticipantId,
                participant.Attributes,
                participant.Form)));

        Assert.Equal(original.ContestId, rehydrated.ContestId);
        Assert.Equal(original.RulesetId, rehydrated.RulesetId);
        Assert.Equal(original.Seed, rehydrated.Seed);
        Assert.Equal(
            original.Participants[0].Attributes.AsSpan().ToArray(),
            rehydrated.Participants[0].Attributes.AsSpan().ToArray());
    }

    [Fact]
    public void RejectsDuplicateParticipantsAndInvalidCompiledScores()
    {
        Assert.Throws<ArgumentException>(() => new ContestSnapshot(
            "contest:2026:42",
            "rules:test-sport:v1",
            seed: 741L,
            [
                new ContestParticipantSnapshot("participant:same", [70], form: 50),
                new ContestParticipantSnapshot("participant:same", [60], form: 50),
            ]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContestParticipantSnapshot("participant:home", [101], form: 50));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContestParticipantSnapshot("participant:home", [50], form: -1));
    }

    [Fact]
    public void DefinesOrderedEventsWithoutSportSpecificVocabulary()
    {
        var contestEvent = new ContestEvent(
            sequence: 3,
            tick: 17,
            kindId: "contest:contribution",
            actorId: "participant:home",
            magnitude: 2);

        Assert.Equal(3, contestEvent.Sequence);
        Assert.Equal(17, contestEvent.Tick);
        Assert.Equal("contest:contribution", contestEvent.KindId);
        Assert.Equal("participant:home", contestEvent.ActorId);
        Assert.Equal(2, contestEvent.Magnitude);

        string[] exportedNames = typeof(ContestSnapshot).Assembly
            .GetExportedTypes()
            .Select(type => type.Name)
            .ToArray();
        Assert.DoesNotContain(exportedNames, name =>
            name.Contains("Goal", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Football", StringComparison.OrdinalIgnoreCase));
    }
}
