using System.Collections.Immutable;
using PCDynasty.Core.Contests;

namespace PCDynasty.Simulation.Contests;

public sealed class ContestSimulationOptions
{
    private const int MaximumEventCount = 10_000;

    public ContestSimulationOptions(
        int durationTicks,
        int eventCount,
        string kindId,
        int maximumMagnitude)
    {
        if (durationTicks <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationTicks),
                durationTicks,
                "Contest duration must be greater than zero.");
        }

        if (eventCount is < 0 or > MaximumEventCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventCount),
                eventCount,
                $"Event count must be between zero and {MaximumEventCount}.");
        }

        if (maximumMagnitude <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumMagnitude),
                maximumMagnitude,
                "Maximum event magnitude must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(kindId);
        string normalizedKindId = kindId.Trim();
        if (normalizedKindId.Length == 0)
        {
            throw new ArgumentException("An event kind identifier is required.", nameof(kindId));
        }

        DurationTicks = durationTicks;
        EventCount = eventCount;
        KindId = normalizedKindId;
        MaximumMagnitude = maximumMagnitude;
    }

    public int DurationTicks { get; }

    public int EventCount { get; }

    public string KindId { get; }

    public int MaximumMagnitude { get; }
}

public sealed class ContestSimulationResult
{
    internal ContestSimulationResult(ImmutableArray<ContestEvent> events)
    {
        Events = events;
    }

    public ImmutableArray<ContestEvent> Events { get; }
}

public static class ContestSimulator
{
    public static ContestSimulationResult Run(
        ContestSnapshot snapshot,
        ContestSimulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(options);

        int[] participantWeights = CalculateParticipantWeights(snapshot.Participants);
        long totalWeight = participantWeights.Sum(weight => (long)weight);
        var random = new DeterministicRandom(unchecked((ulong)snapshot.Seed));
        var generatedEvents = new GeneratedEvent[options.EventCount];

        for (int index = 0; index < generatedEvents.Length; index++)
        {
            int participantIndex = SelectParticipant(ref random, participantWeights, totalWeight);
            generatedEvents[index] = new GeneratedEvent(
                Tick: (int)random.NextInt64((long)options.DurationTicks + 1),
                GenerationOrder: index,
                ActorId: snapshot.Participants[participantIndex].ParticipantId,
                Magnitude: random.Next(options.MaximumMagnitude) + 1);
        }

        Array.Sort(generatedEvents, static (left, right) =>
        {
            int tickComparison = left.Tick.CompareTo(right.Tick);
            return tickComparison != 0
                ? tickComparison
                : left.GenerationOrder.CompareTo(right.GenerationOrder);
        });

        var events = ImmutableArray.CreateBuilder<ContestEvent>(generatedEvents.Length);
        for (int sequence = 0; sequence < generatedEvents.Length; sequence++)
        {
            GeneratedEvent generated = generatedEvents[sequence];
            events.Add(new ContestEvent(
                sequence,
                generated.Tick,
                options.KindId,
                generated.ActorId,
                generated.Magnitude));
        }

        return new ContestSimulationResult(events.ToImmutable());
    }

    private static int[] CalculateParticipantWeights(
        ImmutableArray<ContestParticipantSnapshot> participants)
    {
        var weights = new int[participants.Length];
        for (int participantIndex = 0; participantIndex < participants.Length; participantIndex++)
        {
            ContestParticipantSnapshot participant = participants[participantIndex];
            long attributeTotal = 0;
            foreach (int attribute in participant.Attributes)
            {
                attributeTotal += attribute;
            }

            int averageAttribute = (int)(attributeTotal / participant.Attributes.Length);
            weights[participantIndex] = Math.Max(1, (averageAttribute * 3) + participant.Form);
        }

        return weights;
    }

    private static int SelectParticipant(
        ref DeterministicRandom random,
        IReadOnlyList<int> weights,
        long totalWeight)
    {
        long selectedWeight = random.NextInt64(totalWeight);
        for (int index = 0; index < weights.Count; index++)
        {
            if (selectedWeight < weights[index])
            {
                return index;
            }

            selectedWeight -= weights[index];
        }

        return weights.Count - 1;
    }

    private readonly record struct GeneratedEvent(
        int Tick,
        int GenerationOrder,
        string ActorId,
        int Magnitude);

    private struct DeterministicRandom
    {
        private ulong state;

        public DeterministicRandom(ulong seed)
        {
            state = seed;
        }

        public int Next(int exclusiveMaximum)
        {
            return (int)NextInt64(exclusiveMaximum);
        }

        public long NextInt64(long exclusiveMaximum)
        {
            ulong sample = NextUInt64();
            return (long)(sample % (ulong)exclusiveMaximum);
        }

        private ulong NextUInt64()
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong value = state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
