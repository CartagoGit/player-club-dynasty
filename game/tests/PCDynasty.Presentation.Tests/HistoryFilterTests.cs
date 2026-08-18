using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// Pure filter tests that exercise the same logic the MainController
/// wires into the History section. The presentation code is static so
/// the test duplicates the predicate; this pins the contract.
/// </summary>
public sealed class HistoryFilterTests
{
    private static HistoryEntry Match(
        string competition = "Cup",
        string home = "Alpha",
        string away = "Bravo",
        int homeGoals = 0,
        int awayGoals = 0) =>
        new(
            CompetitionDisplayName: competition,
            HomeClubId: $"club:{home.ToLowerInvariant()}",
            AwayClubId: $"club:{away.ToLowerInvariant()}",
            HomeDisplayName: home,
            AwayDisplayName: away,
            HomeGoals: homeGoals,
            AwayGoals: awayGoals,
            Seed: 1,
            PlayedAtUtc: DateTime.UtcNow);

    [Fact]
    public void AllFilterReturnsEverything()
    {
        var entries = new[]
        {
            Match("Cup A", "Alpha", "Bravo"),
            Match("Cup B", "Charlie", "Delta"),
        };

        var filtered = Apply(entries, competition: "(todas)", club: "(todos)");

        Assert.Equal(2, filtered.Length);
    }

    [Fact]
    public void CompetitionFilterRemovesOtherCompetitions()
    {
        var entries = new[]
        {
            Match("Cup A", "Alpha", "Bravo"),
            Match("Cup B", "Charlie", "Delta"),
            Match("Cup A", "Echo", "Foxtrot"),
        };

        var filtered = Apply(entries, competition: "Cup A", club: "(todos)");

        Assert.Equal(2, filtered.Length);
        Assert.All(filtered, e => Assert.Equal("Cup A", e.CompetitionDisplayName));
    }

    [Fact]
    public void ClubFilterMatchesHomeOrAway()
    {
        var entries = new[]
        {
            Match("Cup A", "Alpha", "Bravo"),
            Match("Cup A", "Charlie", "Alpha"),
            Match("Cup A", "Delta", "Echo"),
        };

        var filtered = Apply(entries, competition: "(todas)", club: "Alpha");

        Assert.Equal(2, filtered.Length);
    }

    [Fact]
    public void CombinedFiltersAndTogether()
    {
        var entries = new[]
        {
            Match("Cup A", "Alpha", "Bravo"),
            Match("Cup B", "Alpha", "Charlie"),
            Match("Cup A", "Delta", "Echo"),
        };

        var filtered = Apply(entries, competition: "Cup A", club: "Alpha");

        Assert.Single(filtered);
        Assert.Equal("Bravo", filtered[0].AwayDisplayName);
    }

    [Fact]
    public void NoMatchesReturnsEmpty()
    {
        var entries = new[] { Match("Cup A", "Alpha", "Bravo") };

        var filtered = Apply(entries, competition: "Cup Z", club: "(todos)");

        Assert.Empty(filtered);
    }

    private static HistoryEntry[] Apply(
        IReadOnlyList<HistoryEntry> entries,
        string competition,
        string club)
    {
        IEnumerable<HistoryEntry> query = entries;
        if (!string.Equals(competition, "(todas)", StringComparison.Ordinal))
        {
            query = query.Where(e => string.Equals(e.CompetitionDisplayName, competition, StringComparison.Ordinal));
        }

        if (!string.Equals(club, "(todos)", StringComparison.Ordinal))
        {
            query = query.Where(e =>
                string.Equals(e.HomeDisplayName, club, StringComparison.Ordinal)
                || string.Equals(e.AwayDisplayName, club, StringComparison.Ordinal));
        }

        return query.ToArray();
    }
}
