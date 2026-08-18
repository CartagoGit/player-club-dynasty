using System.Collections.Immutable;

namespace PCDynasty.Application.History;

public enum HistorySubjectKind
{
    Career,
    Competition,
}

public sealed class HistoryView
{
    internal HistoryView(
        string subjectId,
        HistorySubjectKind subjectKind,
        ImmutableArray<HistoryEntryView> entries)
    {
        SubjectId = subjectId;
        SubjectKind = subjectKind;
        Entries = entries;
    }

    public string SubjectId { get; }
    public HistorySubjectKind SubjectKind { get; }
    public ImmutableArray<HistoryEntryView> Entries { get; }
}

public sealed class HistoryEntryView
{
    internal HistoryEntryView(
        long sequence,
        int occurredOnDay,
        string typeId,
        IEnumerable<string> subjectIds,
        IEnumerable<KeyValuePair<string, string>> values)
    {
        Sequence = sequence;
        OccurredOnDay = occurredOnDay;
        TypeId = typeId;
        SubjectIds = subjectIds.ToImmutableArray();
        Values = values.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
    }

    public long Sequence { get; }
    public int OccurredOnDay { get; }
    public string TypeId { get; }
    public ImmutableArray<string> SubjectIds { get; }
    public ImmutableDictionary<string, string> Values { get; }
}
