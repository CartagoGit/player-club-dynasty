using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class SeasonHistoryTests
{
    [Fact]
    public void AppendAddsEntryToSnapshot()
    {
        var history = SeasonHistory.CreateForTesting();
        var entry = NewEntry(position: 1, points: 9);

        history.Append(entry);

        Assert.Single(history.Snapshot());
    }

    [Fact]
    public void AppendPreservesInsertionOrder()
    {
        var history = SeasonHistory.CreateForTesting();
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
        var history = SeasonHistory.CreateForTesting();
        history.Append(NewEntry(position: 1, points: 9));

        var snapshot = history.Snapshot();
        history.Append(NewEntry(position: 2, points: 6));

        Assert.Single(snapshot);
        Assert.Equal(2, history.Snapshot().Count);
    }

    [Fact]
    public void ClearEmptiesTheHistory()
    {
        var history = SeasonHistory.CreateForTesting();
        history.Append(NewEntry(position: 1, points: 9));
        history.Append(NewEntry(position: 2, points: 6));

        history.Clear();

        Assert.Empty(history.Snapshot());
    }

    [Fact]
    public void AppendRejectsNullEntry()
    {
        var history = SeasonHistory.CreateForTesting();
        Assert.Throws<ArgumentNullException>(() => history.Append(null!));
    }

    [Fact]
    public void SingletonInstanceIsStable()
    {
        Assert.Same(SeasonHistory.Instance, SeasonHistory.Instance);
    }

    private static SeasonHistoryEntry NewEntry(int position, int points) =>
        new(
            CompetitionDisplayName: "Test Cup",
            ManagedClubId: "club:test",
            ManagedClubDisplayName: "Test FC",
            ManagedPosition: position,
            ManagedPoints: points,
            ManagedGoalsFor: 10,
            ManagedGoalsAgainst: 5,
            TotalClubs: 6,
            TotalFixtures: 15,
            PlayedAtUtc: DateTime.UtcNow,
            FinalStandings: System.Collections.Immutable.ImmutableArray<StandingRow>.Empty);
}