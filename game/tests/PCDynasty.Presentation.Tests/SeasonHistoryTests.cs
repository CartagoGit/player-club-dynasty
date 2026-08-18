using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class SeasonHistoryTests
{
    [Fact]
    public void AppendAddsEntryToSnapshot()
    {
        var history = new SeasonHistory();
        var entry = NewEntry(position: 1, points: 9);

        history.Append(entry);

        Assert.Single(history.Snapshot());
    }

    [Fact]
    public void AppendPreservesInsertionOrder()
    {
        var history = new SeasonHistory();
        var first = NewEntry(position: 1, points: 9);
        var second = NewEntry(position: 2, points: 6);

        history.Append(first);
        history.Append(second);

        var snapshot = history.Snapshot();
        Assert.Equal(2, snapshot.Count);
        Assert.Same(first, snapshot[0]);
        Assert.Same(second, snapshot[1]);
    }

    [Fact]
    public void SnapshotIsImmutableForCaller()
    {
        var history = new SeasonHistory();
        history.Append(NewEntry(position: 1, points: 9));

        var snapshot = history.Snapshot();
        history.Append(NewEntry(position: 2, points: 6));

        Assert.Single(snapshot);
        Assert.Equal(2, history.Snapshot().Count);
    }

    [Fact]
    public void ClearEmptiesTheHistory()
    {
        var history = new SeasonHistory();
        history.Append(NewEntry(position: 1, points: 9));
        history.Append(NewEntry(position: 2, points: 6));

        history.Clear();

        Assert.Empty(history.Snapshot());
    }

    [Fact]
    public void AppendRejectsNullEntry()
    {
        var history = new SeasonHistory();
        Assert.Throws<ArgumentNullException>(() => history.Append(null!));
    }

    [Fact]
    public void NewHistoriesAreIndependent()
    {
        var first = new SeasonHistory();
        var second = new SeasonHistory();

        first.Append(NewEntry(position: 1, points: 9));

        Assert.Single(first.Snapshot());
        Assert.Empty(second.Snapshot());
    }

    [Fact]
    public void LatestSeasonForReturnsMostRecentForClub()
    {
        var history = new SeasonHistory();
        history.Append(NewEntryFor("club:a", "Alpha", position: 3, points: 6));
        history.Append(NewEntryFor("club:b", "Bravo", position: 1, points: 9));
        history.Append(NewEntryFor("club:a", "Alpha", position: 2, points: 7));

        var latest = history.LatestSeasonFor("club:a");

        Assert.NotNull(latest);
        Assert.Equal("Alpha", latest!.ManagedClubDisplayName);
        Assert.Equal(2, latest.ManagedPosition);
        Assert.Equal(7, latest.ManagedPoints);
    }

    [Fact]
    public void LatestSeasonForReturnsNullForUnknownClub()
    {
        var history = new SeasonHistory();
        history.Append(NewEntryFor("club:a", "Alpha", position: 1, points: 9));

        Assert.Null(history.LatestSeasonFor("club:unknown"));
    }

    [Fact]
    public void SeasonsForFiltersByClubIdAndPreservesOrder()
    {
        var history = new SeasonHistory();
        history.Append(NewEntryFor("club:a", "Alpha", position: 3, points: 6));
        history.Append(NewEntryFor("club:b", "Bravo", position: 1, points: 9));
        history.Append(NewEntryFor("club:a", "Alpha", position: 1, points: 9));

        var seasons = history.SeasonsFor("club:a");

        Assert.Equal(2, seasons.Count);
        Assert.Equal(3, seasons[0].ManagedPosition);
        Assert.Equal(1, seasons[1].ManagedPosition);
    }

    private static SeasonHistoryEntry NewEntry(int position, int points) =>
        NewEntryFor("club:test", "Test FC", position, points);

    private static SeasonHistoryEntry NewEntryFor(string clubId, string clubName, int position, int points) =>
        new(
            CompetitionDisplayName: "Test Cup",
            ManagedClubId: clubId,
            ManagedClubDisplayName: clubName,
            ManagedPosition: position,
            ManagedPoints: points,
            ManagedGoalsFor: 10,
            ManagedGoalsAgainst: 5,
            TotalClubs: 6,
            TotalFixtures: 15,
            PlayedAtUtc: DateTime.UtcNow,
            FinalStandings: System.Collections.Immutable.ImmutableArray<StandingRow>.Empty);
}
