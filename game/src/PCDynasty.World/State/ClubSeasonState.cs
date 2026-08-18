using System.Collections.Immutable;
using PCDynasty.World.Commitments;

namespace PCDynasty.World.State;

public sealed class ClubSeasonState
{
    public ClubSeasonState(
        string clubId,
        string seasonId,
        string competitionId,
        IEnumerable<KeyValuePair<string, long>> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        var copiedMetrics = ImmutableDictionary.CreateBuilder<string, long>(StringComparer.Ordinal);
        foreach ((string metricId, long value) in metrics)
        {
            if (!copiedMetrics.TryAdd(
                WorldValue.NormalizeIdentifier(metricId, nameof(metrics)),
                value))
            {
                throw new ArgumentException("Metric identifiers must be unique.", nameof(metrics));
            }
        }

        ClubId = WorldValue.NormalizeIdentifier(clubId, nameof(clubId));
        SeasonId = WorldValue.NormalizeIdentifier(seasonId, nameof(seasonId));
        CompetitionId = WorldValue.NormalizeIdentifier(competitionId, nameof(competitionId));
        Metrics = copiedMetrics.ToImmutable();
    }

    public string ClubId { get; }
    public string SeasonId { get; }
    public string CompetitionId { get; }
    public ImmutableDictionary<string, long> Metrics { get; }
}
