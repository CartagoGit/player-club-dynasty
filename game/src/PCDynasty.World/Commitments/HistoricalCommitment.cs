using System.Collections.Immutable;

namespace PCDynasty.World.Commitments;

public sealed class HistoricalCommitment
{
    public HistoricalCommitment(
        string commitmentId,
        string subjectId,
        string outcomeId,
        long committedAtTick,
        long effectiveAtTick,
        SimulationFidelity originFidelity,
        IEnumerable<KeyValuePair<string, string>> values)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(committedAtTick);
        ArgumentOutOfRangeException.ThrowIfNegative(effectiveAtTick);
        if (effectiveAtTick < committedAtTick)
        {
            throw new ArgumentException("A commitment cannot become effective before it is confirmed.");
        }

        if (!Enum.IsDefined(originFidelity))
        {
            throw new ArgumentOutOfRangeException(nameof(originFidelity));
        }

        ArgumentNullException.ThrowIfNull(values);
        var normalizedValues = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in values)
        {
            string normalizedKey = WorldValue.NormalizeIdentifier(key, nameof(values));
            ArgumentNullException.ThrowIfNull(value);
            if (!normalizedValues.TryAdd(normalizedKey, value))
            {
                throw new ArgumentException("Commitment value identifiers must be unique.", nameof(values));
            }
        }

        CommitmentId = WorldValue.NormalizeIdentifier(commitmentId, nameof(commitmentId));
        SubjectId = WorldValue.NormalizeIdentifier(subjectId, nameof(subjectId));
        OutcomeId = WorldValue.NormalizeIdentifier(outcomeId, nameof(outcomeId));
        CommittedAtTick = committedAtTick;
        EffectiveAtTick = effectiveAtTick;
        OriginFidelity = originFidelity;
        Values = normalizedValues.ToImmutable();
    }

    public string CommitmentId { get; }
    public string SubjectId { get; }
    public string OutcomeId { get; }
    public long CommittedAtTick { get; }
    public long EffectiveAtTick { get; }
    public SimulationFidelity OriginFidelity { get; }
    public ImmutableDictionary<string, string> Values { get; }
}

internal static class WorldValue
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
