using PCDynasty.Contracts.Careers;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Owns the mutable history of exactly one active career.
/// </summary>
public sealed class CareerSession
{
    private readonly MatchHistory _matches = new();
    private readonly SeasonHistory _seasons = new();

    public void Record(HistoryEntry entry) => _matches.Append(entry);

    public void Record(SeasonHistoryEntry entry) => _seasons.Append(entry);

    public IReadOnlyList<HistoryEntry> MatchSnapshot() => _matches.Snapshot();

    public IReadOnlyList<SeasonHistoryEntry> SeasonSnapshot() => _seasons.Snapshot();

    public IReadOnlyList<SeasonHistoryEntry> SeasonsFor(string clubId) => _seasons.SeasonsFor(clubId);

    public SeasonHistoryEntry? LatestSeasonFor(string clubId) => _seasons.LatestSeasonFor(clubId);
}
