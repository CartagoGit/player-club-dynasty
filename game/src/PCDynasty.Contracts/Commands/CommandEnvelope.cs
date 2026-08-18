using System.Collections.Immutable;

namespace PCDynasty.Contracts.Commands;

public sealed class CommandEnvelope
{
    public CommandEnvelope(
        string commandId,
        string commandType,
        ContractVersion version,
        long issuedAtTick,
        string correlationId,
        string? causationId,
        IEnumerable<byte> payload)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(issuedAtTick);
        ArgumentNullException.ThrowIfNull(payload);
        if (version.Major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        CommandId = ContractIdentifier.Normalize(commandId, nameof(commandId));
        CommandType = ContractIdentifier.Normalize(commandType, nameof(commandType));
        Version = version;
        IssuedAtTick = issuedAtTick;
        CorrelationId = ContractIdentifier.Normalize(correlationId, nameof(correlationId));
        CausationId = ContractIdentifier.NormalizeOptional(causationId, nameof(causationId));
        Payload = payload.ToImmutableArray();
    }

    public string CommandId { get; }
    public string CommandType { get; }
    public ContractVersion Version { get; }
    public long IssuedAtTick { get; }
    public string CorrelationId { get; }
    public string? CausationId { get; }
    public ImmutableArray<byte> Payload { get; }
}
