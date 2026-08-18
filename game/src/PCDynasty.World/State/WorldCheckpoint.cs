using System.Collections.Immutable;

namespace PCDynasty.World.State;

public readonly record struct WorldDate
{
    public WorldDate(int year, int month, int day)
    {
        _ = new DateOnly(year, month, day);
        Year = year;
        Month = month;
        Day = day;
    }

    public int Year { get; }
    public int Month { get; }
    public int Day { get; }

    public DateOnly ToDateOnly() => new(Year, Month, Day);
}

public sealed class WorldCheckpoint
{
    public WorldCheckpoint(
        int schemaVersion,
        long sequence,
        WorldDate date,
        IEnumerable<ClubSeasonState> clubSeasons)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        ArgumentNullException.ThrowIfNull(clubSeasons);
        if (date.Year <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(date));
        }

        var keys = new HashSet<(string ClubId, string SeasonId, string CompetitionId)>();
        ClubSeasonState[] copiedStates = clubSeasons.Select(state =>
        {
            ArgumentNullException.ThrowIfNull(state);
            if (!keys.Add((state.ClubId, state.SeasonId, state.CompetitionId)))
            {
                throw new ArgumentException(
                    "Club, season and competition keys must be unique.",
                    nameof(clubSeasons));
            }

            return state;
        }).OrderBy(state => state.ClubId, StringComparer.Ordinal)
          .ThenBy(state => state.SeasonId, StringComparer.Ordinal)
          .ThenBy(state => state.CompetitionId, StringComparer.Ordinal)
          .ToArray();

        SchemaVersion = schemaVersion;
        Sequence = sequence;
        Date = date;
        ClubSeasons = copiedStates.ToImmutableArray();
    }

    public int SchemaVersion { get; }
    public long Sequence { get; }
    public WorldDate Date { get; }
    public ImmutableArray<ClubSeasonState> ClubSeasons { get; }
}
