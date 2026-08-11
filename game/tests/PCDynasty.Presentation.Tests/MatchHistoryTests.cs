using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class MatchHistoryTests
{
    [Fact]
    public void AppendAddsEntryToSnapshot()
    {
        var history = MatchHistory.CreateForTesting();
        var entry = NewEntry(homeGoals: 2, awayGoals: 1);

        history.Append(entry);

        Assert.Single(history.Snapshot());
    }

    [Fact]
    public void AppendPreservesInsertionOrder()
    {
        var history = MatchHistory.CreateForTesting();
        var first = NewEntry(seed: 1);
        var second = NewEntry(seed: 2);
        var third = NewEntry(seed: 3);

        history.Append(first);
        history.Append(second);
        history.Append(third);

        var snapshot = history.Snapshot();
        Assert.Equal(3, snapshot.Count);
        Assert.Same(first, snapshot[0]);
        Assert.Same(second, snapshot[1]);
        Assert.Same(third, snapshot[2]);
    }

    [Fact]
    public void SnapshotIsImmutableForCaller()
    {
        var history = MatchHistory.CreateForTesting();
        history.Append(NewEntry(seed: 1));

        var snapshot = history.Snapshot();
        history.Append(NewEntry(seed: 2));

        // The earlier snapshot must not reflect the later append.
        Assert.Single(snapshot);
        Assert.Equal(2, history.Snapshot().Count);
    }

    [Fact]
    public void ClearEmptiesTheHistory()
    {
        var history = MatchHistory.CreateForTesting();
        history.Append(NewEntry(seed: 1));
        history.Append(NewEntry(seed: 2));

        history.Clear();

        Assert.Empty(history.Snapshot());
    }

    [Fact]
    public void AppendRejectsNullEntry()
    {
        var history = MatchHistory.CreateForTesting();
        Assert.Throws<ArgumentNullException>(() => history.Append(null!));
    }

    [Fact]
    public void SingletonInstanceIsStable()
    {
        Assert.Same(MatchHistory.Instance, MatchHistory.Instance);
    }

    private static HistoryEntry NewEntry(int homeGoals = 0, int awayGoals = 0, long seed = 0) =>
        new(
            CompetitionDisplayName: "Test Cup",
            HomeDisplayName: "Home",
            AwayDisplayName: "Away",
            HomeClubId: "club:home",
            AwayClubId: "club:away",
            HomeGoals: homeGoals,
            AwayGoals: awayGoals,
            Seed: seed,
            PlayedAtUtc: DateTime.UtcNow);
}