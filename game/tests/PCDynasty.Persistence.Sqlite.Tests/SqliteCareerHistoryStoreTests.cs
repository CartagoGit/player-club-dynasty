using System.Collections.Immutable;
using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteCareerHistoryStoreTests : IDisposable
{
    private readonly string _databasePath;

    public SqliteCareerHistoryStoreTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"pcd-career-{Guid.NewGuid():N}.sqlite");
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void SaveMatchAndLoadMatchesRoundTrip()
    {
        var store = new SqliteCareerHistoryStore(_databasePath);
        var entry = NewMatch(homeGoals: 2, awayGoals: 1, seed: 42);

        store.SaveMatch(entry);
        var loaded = store.LoadMatches();

        Assert.Single(loaded);
        Assert.Equal(2, loaded[0].HomeGoals);
        Assert.Equal(1, loaded[0].AwayGoals);
        Assert.Equal(42, loaded[0].Seed);
        Assert.Equal(entry.HomeClubId, loaded[0].HomeClubId);
    }

    [Fact]
    public void SaveMatchPersistsMultipleEntriesInOrder()
    {
        var store = new SqliteCareerHistoryStore(_databasePath);
        var first = NewMatch(homeGoals: 1, awayGoals: 0, playedAtUtc: DateTime.UtcNow.AddSeconds(-30));
        var second = NewMatch(homeGoals: 3, awayGoals: 3, playedAtUtc: DateTime.UtcNow.AddSeconds(-15));

        store.SaveMatch(first);
        store.SaveMatch(second);

        var loaded = store.LoadMatches();
        Assert.Equal(2, loaded.Count);
        Assert.Equal(first.PlayedAtUtc, loaded[0].PlayedAtUtc);
        Assert.Equal(second.PlayedAtUtc, loaded[1].PlayedAtUtc);
    }

    [Fact]
    public void SaveSeasonPersistsStandingsAndRoundTrips()
    {
        var store = new SqliteCareerHistoryStore(_databasePath);
        var standings = ImmutableArray.Create(
            new StandingRow("club:a", "Alpha FC", "Alpha", 9, 3, 3, 0, 0, 8, 2),
            new StandingRow("club:b", "Bravo FC", "Bravo", 6, 3, 2, 0, 1, 5, 3));
        var season = NewSeason(position: 1, points: 9, standings: standings);

        store.SaveSeason(season);
        var loaded = store.LoadSeasons();

        Assert.Single(loaded);
        Assert.Equal("Managed FC", loaded[0].ManagedClubDisplayName);
        Assert.Equal(9, loaded[0].ManagedPoints);
        Assert.Equal(2, loaded[0].FinalStandings.Length);
        Assert.Equal("Alpha FC", loaded[0].FinalStandings[0].DisplayName);
        Assert.Equal(9, loaded[0].FinalStandings[0].Points);
        Assert.Equal(6, loaded[0].FinalStandings[1].Points);
    }

    [Fact]
    public void NewStoreStartsEmpty()
    {
        var store = new SqliteCareerHistoryStore(_databasePath);

        Assert.Empty(store.LoadMatches());
        Assert.Empty(store.LoadSeasons());
    }

    [Fact]
    public void ClearWipesEverything()
    {
        var store = new SqliteCareerHistoryStore(_databasePath);
        store.SaveMatch(NewMatch());
        store.SaveSeason(NewSeason(1, 9, ImmutableArray<StandingRow>.Empty));

        store.Clear();

        Assert.Empty(store.LoadMatches());
        Assert.Empty(store.LoadSeasons());
    }

    [Fact]
    public void RejectsEmptyDatabasePath()
    {
        Assert.Throws<ArgumentException>(() => new SqliteCareerHistoryStore(" "));
    }

    [Fact]
    public void AdapterSeparatesSchemaReadsAndWrites()
    {
        string sourceRoot = Path.Combine(FindGameRoot(), "src", "PCDynasty.Persistence.Sqlite");
        string[] collaboratorNames =
        [
            "SqliteCareerHistorySchema.cs",
            "SqliteCareerHistoryReader.cs",
            "SqliteCareerHistoryWriter.cs",
        ];

        foreach (string collaboratorName in collaboratorNames)
        {
            Assert.True(
                File.Exists(Path.Combine(sourceRoot, collaboratorName)),
                $"Career history responsibility must use '{collaboratorName}'.");
        }

        Assert.True(
            File.ReadLines(Path.Combine(sourceRoot, "SqliteCareerHistoryStore.cs")).Count() <= 130,
            "The public adapter must remain a small orchestration boundary.");
    }

    private static HistoryEntry NewMatch(
        int homeGoals = 0,
        int awayGoals = 0,
        long seed = 1,
        DateTime? playedAtUtc = null) =>
        new(
            CompetitionDisplayName: "Test Cup",
            HomeClubId: "club:home",
            AwayClubId: "club:away",
            HomeDisplayName: "Home",
            AwayDisplayName: "Away",
            HomeGoals: homeGoals,
            AwayGoals: awayGoals,
            Seed: seed,
            PlayedAtUtc: playedAtUtc ?? DateTime.UtcNow);

    private static SeasonHistoryEntry NewSeason(int position, int points, ImmutableArray<StandingRow> standings) =>
        new(
            CompetitionDisplayName: "Test Cup",
            ManagedClubId: "club:managed",
            ManagedClubDisplayName: "Managed FC",
            ManagedPosition: position,
            ManagedPoints: points,
            ManagedGoalsFor: 10,
            ManagedGoalsAgainst: 5,
            TotalClubs: standings.Length,
            TotalFixtures: standings.Length * 2,
            PlayedAtUtc: DateTime.UtcNow,
            FinalStandings: standings);

    private static string FindGameRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "src", "PCDynasty.Persistence.Sqlite")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the game source root.");
    }
}
