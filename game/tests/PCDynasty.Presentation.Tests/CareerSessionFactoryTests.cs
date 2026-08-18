using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class CareerSessionFactoryTests
{
    [Fact]
    public void SequentialOpeningsCreateFreshSessionsAndNewCareerClearsStoredHistory()
    {
        var store = new InMemoryCareerHistoryStore();
        store.SaveMatch(NewMatch(seed: 17));
        var factory = new CareerSessionFactory(store);

        CareerSession restored = factory.Restore();
        CareerSession created = factory.CreateNew();

        Assert.NotSame(restored, created);
        Assert.Single(restored.MatchSnapshot());
        Assert.Empty(created.MatchSnapshot());
        Assert.Empty(store.LoadMatches());
        Assert.Empty(store.LoadSeasons());
    }

    [Fact]
    public void RepeatedRestoreDoesNotReuseInMemoryMutations()
    {
        var store = new InMemoryCareerHistoryStore();
        store.SaveMatch(NewMatch(seed: 23));
        var factory = new CareerSessionFactory(store);

        CareerSession first = factory.Restore();
        first.Record(NewMatch(seed: 29));
        CareerSession second = factory.Restore();

        Assert.NotSame(first, second);
        Assert.Equal(2, first.MatchSnapshot().Count);
        Assert.Single(second.MatchSnapshot());
    }

    private static HistoryEntry NewMatch(long seed) => new(
        CompetitionDisplayName: "Boreal Series",
        HomeDisplayName: "Northglass",
        AwayDisplayName: "Cinder Valley",
        HomeClubId: "club:northglass",
        AwayClubId: "club:cinder-valley",
        HomeGoals: 1,
        AwayGoals: 0,
        Seed: seed,
        PlayedAtUtc: DateTime.UtcNow);

    private sealed class InMemoryCareerHistoryStore : ICareerHistoryStore
    {
        private readonly List<HistoryEntry> _matches = [];
        private readonly List<SeasonHistoryEntry> _seasons = [];

        public void SaveMatch(HistoryEntry entry) => _matches.Add(entry);

        public void SaveSeason(SeasonHistoryEntry entry) => _seasons.Add(entry);

        public IReadOnlyList<HistoryEntry> LoadMatches() => _matches.ToArray();

        public IReadOnlyList<SeasonHistoryEntry> LoadSeasons() => _seasons.ToArray();

        public void Clear()
        {
            _matches.Clear();
            _seasons.Clear();
        }
    }
}
