using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Career-local log of matches played in the current session. Backed by an
/// immutable append-only list so the UI can render a stable ordering without
/// owning the mutation surface. Persists across screens, dies with the
/// process; persistence is supplied by an adapter at the composition root.
/// </summary>
public sealed class MatchHistory
{
    private readonly object _gate = new();
    private ImmutableArray<HistoryEntry> _entries = ImmutableArray<HistoryEntry>.Empty;

    public void Append(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_gate)
        {
            _entries = _entries.Add(entry);
        }
    }

    public IReadOnlyList<HistoryEntry> Snapshot()
    {
        lock (_gate)
        {
            return _entries;
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _entries = ImmutableArray<HistoryEntry>.Empty;
        }
    }
}

public sealed record HistoryEntry(
    string CompetitionDisplayName,
    string HomeDisplayName,
    string AwayDisplayName,
    string HomeClubId,
    string AwayClubId,
    int HomeGoals,
    int AwayGoals,
    long Seed,
    DateTime PlayedAtUtc);
