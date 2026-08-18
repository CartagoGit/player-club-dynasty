using System.Collections.Immutable;

namespace PCDynasty.History.Events;

public sealed class HistoricalEvent
{
    public HistoricalEvent(
        long sequence,
        int occurredOnDay,
        string typeId,
        int schemaVersion,
        IEnumerable<string> subjectIds,
        IEnumerable<KeyValuePair<string, string>> values)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentOutOfRangeException.ThrowIfNegative(occurredOnDay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentNullException.ThrowIfNull(subjectIds);
        ArgumentNullException.ThrowIfNull(values);

        var normalizedSubjects = ImmutableArray.CreateBuilder<string>();
        var uniqueSubjects = new HashSet<string>(StringComparer.Ordinal);
        foreach (string subjectId in subjectIds)
        {
            string normalized = HistoryValue.NormalizeIdentifier(subjectId, nameof(subjectIds));
            if (!uniqueSubjects.Add(normalized))
            {
                throw new ArgumentException("Subject identifiers must be unique.", nameof(subjectIds));
            }

            normalizedSubjects.Add(normalized);
        }

        if (normalizedSubjects.Count == 0)
        {
            throw new ArgumentException("A historical event must identify at least one subject.", nameof(subjectIds));
        }

        var normalizedValues = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in values)
        {
            string normalizedKey = HistoryValue.NormalizeIdentifier(key, nameof(values));
            ArgumentNullException.ThrowIfNull(value);
            if (!normalizedValues.TryAdd(normalizedKey, value))
            {
                throw new ArgumentException("Value identifiers must be unique.", nameof(values));
            }
        }

        Sequence = sequence;
        OccurredOnDay = occurredOnDay;
        TypeId = HistoryValue.NormalizeIdentifier(typeId, nameof(typeId));
        SchemaVersion = schemaVersion;
        SubjectIds = normalizedSubjects.ToImmutable();
        Values = normalizedValues.ToImmutable();
    }

    public long Sequence { get; }
    public int OccurredOnDay { get; }
    public string TypeId { get; }
    public int SchemaVersion { get; }
    public ImmutableArray<string> SubjectIds { get; }
    public ImmutableDictionary<string, string> Values { get; }
}

internal static class HistoryValue
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
