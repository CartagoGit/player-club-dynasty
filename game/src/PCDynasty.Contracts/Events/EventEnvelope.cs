using System.Collections.Immutable;

namespace PCDynasty.Contracts.Events;

public sealed class EventEnvelope
{
    public EventEnvelope(
        string eventId,
        string eventType,
        ContractVersion version,
        long sequence,
        long occurredAtTick,
        string correlationId,
        string? causationId,
        IEnumerable<byte> payload)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentOutOfRangeException.ThrowIfNegative(occurredAtTick);
        ArgumentNullException.ThrowIfNull(payload);
        if (version.Major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        EventId = ContractIdentifier.Normalize(eventId, nameof(eventId));
        EventType = ContractIdentifier.Normalize(eventType, nameof(eventType));
        Version = version;
        Sequence = sequence;
        OccurredAtTick = occurredAtTick;
        CorrelationId = ContractIdentifier.Normalize(correlationId, nameof(correlationId));
        CausationId = ContractIdentifier.NormalizeOptional(causationId, nameof(causationId));
        Payload = payload.ToImmutableArray();
    }

    public string EventId { get; }
    public string EventType { get; }
    public ContractVersion Version { get; }
    public long Sequence { get; }
    public long OccurredAtTick { get; }
    public string CorrelationId { get; }
    public string? CausationId { get; }
    public ImmutableArray<byte> Payload { get; }
}
