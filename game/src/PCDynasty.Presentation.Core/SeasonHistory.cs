using System.Collections.Immutable;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Career-local log of seasons completed in the current session. Each entry
/// captures the final standings so the UI can show a progression table.
/// </summary>
public sealed class SeasonHistory
{
    private readonly object _gate = new();
    private ImmutableArray<SeasonHistoryEntry> _entries = ImmutableArray<SeasonHistoryEntry>.Empty;

    public void Append(SeasonHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (_gate)
        {
            _entries = _entries.Add(entry);
        }
    }

    public IReadOnlyList<SeasonHistoryEntry> Snapshot()
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
            _entries = ImmutableArray<SeasonHistoryEntry>.Empty;
        }
    }

    /// <summary>
    /// Most recent season the given club played in this session, if any.
    /// Used by the career dashboard so the manager sees their latest
    /// result at a glance.
    /// </summary>
    public SeasonHistoryEntry? LatestSeasonFor(string clubId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubId);
        lock (_gate)
        {
            for (int i = _entries.Length - 1; i >= 0; i--)
            {
                if (string.Equals(_entries[i].ManagedClubId, clubId, StringComparison.Ordinal))
                {
                    return _entries[i];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// All seasons the given club played in this session, oldest first.
    /// </summary>
    public IReadOnlyList<SeasonHistoryEntry> SeasonsFor(string clubId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubId);
        lock (_gate)
        {
            return _entries
                .Where(entry => string.Equals(entry.ManagedClubId, clubId, StringComparison.Ordinal))
                .ToImmutableArray();
        }
    }
}
