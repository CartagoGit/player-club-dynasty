using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Process-local log of matches played in the current session. Backed by an
/// immutable append-only list so the UI can render a stable ordering without
/// owning the mutation surface. Persists across screens, dies with the
/// process; Phase 5 swaps this for a SQLite-backed save.
/// </summary>
public sealed class MatchHistory
{
    private readonly object _gate = new();
    private ImmutableArray<HistoryEntry> _entries = ImmutableArray<HistoryEntry>.Empty;

    public static MatchHistory Instance { get; } = new();

    private MatchHistory()
    {
    }

    /// <summary>
    /// Test seam: a fresh instance per test keeps cross-test pollution out
    /// of the singleton. Production code uses <see cref="Instance"/>.
    /// </summary>
    public static MatchHistory CreateForTesting() => new();

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