using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Process-local log of seasons completed in the current session. Each entry
/// captures the final standings so the UI can show a progression table.
/// </summary>
public sealed class SeasonHistory
{
    private readonly object _gate = new();
    private ImmutableArray<SeasonHistoryEntry> _entries = ImmutableArray<SeasonHistoryEntry>.Empty;

    public static SeasonHistory Instance { get; } = new();

    private SeasonHistory()
    {
    }

    public static SeasonHistory CreateForTesting() => new();

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
}

public sealed record SeasonHistoryEntry(
    string CompetitionDisplayName,
    string ManagedClubId,
    string ManagedClubDisplayName,
    int ManagedPosition,
    int ManagedPoints,
    int ManagedGoalsFor,
    int ManagedGoalsAgainst,
    int TotalClubs,
    int TotalFixtures,
    DateTime PlayedAtUtc,
    ImmutableArray<StandingRow> FinalStandings);