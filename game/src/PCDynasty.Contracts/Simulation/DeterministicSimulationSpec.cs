namespace PCDynasty.Contracts.Simulation;

public sealed class DeterministicSimulationSpec
{
    public DeterministicSimulationSpec(
        int protocolVersion,
        string prngAlgorithmId,
        int prngVersion,
        string canonicalSerializationId,
        int canonicalSerializationVersion,
        int ticksPerDay,
        long numericScale)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(protocolVersion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(prngVersion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(canonicalSerializationVersion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerDay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numericScale);

        ProtocolVersion = protocolVersion;
        PrngAlgorithmId = ContractValue.NormalizeIdentifier(
            prngAlgorithmId,
            nameof(prngAlgorithmId));
        PrngVersion = prngVersion;
        CanonicalSerializationId = ContractValue.NormalizeIdentifier(
            canonicalSerializationId,
            nameof(canonicalSerializationId));
        CanonicalSerializationVersion = canonicalSerializationVersion;
        TicksPerDay = ticksPerDay;
        NumericScale = numericScale;
    }

    public int ProtocolVersion { get; }
    public string PrngAlgorithmId { get; }
    public int PrngVersion { get; }
    public string CanonicalSerializationId { get; }
    public int CanonicalSerializationVersion { get; }
    public int TicksPerDay { get; }
    public long NumericScale { get; }
}

internal static class ContractValue
{
    public static string NormalizeIdentifier(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
