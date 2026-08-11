using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

public sealed class SeasonSimulatorTests
{
    private static readonly IReadOnlyList<ClubRow> FourClubs = new[]
    {
        new ClubRow("club:a", "Alpha FC", "Alpha", "A-Town", "AAA", "1900", 80, 70, 20),
        new ClubRow("club:b", "Bravo United", "Bravo", "B-Town", "AAA", "1910", 60, 50, 20),
        new ClubRow("club:c", "Charlie Rovers", "Charlie", "C-Town", "BBB", "1920", 55, 45, 20),
        new ClubRow("club:d", "Delta Sporting", "Delta", "D-Town", "BBB", "1930", 50, 40, 20),
    };

    [Fact]
    public void SimulatePlaysRoundRobinOfFourClubs()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 1L);

        // Four clubs -> six fixtures (4 choose 2).
        Assert.Equal(6, result.Fixtures.Length);
    }

    [Fact]
    public void StandingsHasOneRowPerParticipant()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 1L);

        Assert.Equal(FourClubs.Count, result.Standings.Length);
    }

    [Fact]
    public void EachClubPlaysThreeMatches()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 1L);

        Assert.All(result.Standings, row => Assert.Equal(3, row.Played));
    }

    [Fact]
    public void WinsPlusDrawsPlusLossesEqualPlayed()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 7L);

        Assert.All(result.Standings, row =>
            Assert.Equal(row.Played, row.Wins + row.Draws + row.Losses));
    }

    [Fact]
    public void StandingsAreOrderedByPointsThenGoalDifference()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 1L);

        for (int i = 1; i < result.Standings.Length; i++)
        {
            var prev = result.Standings[i - 1];
            var curr = result.Standings[i];
            Assert.True(
                prev.Points > curr.Points
                || (prev.Points == curr.Points && prev.GoalDifference >= curr.GoalDifference),
                $"Row {i} ({curr.DisplayName}: {curr.Points} pts, GD {curr.GoalDifference}) "
                + $"should rank below row {i - 1} ({prev.DisplayName}: {prev.Points} pts, GD {prev.GoalDifference}).");
        }
    }

    [Fact]
    public void TotalGoalsAreSymmetric()
    {
        var result = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 99L);

        int sumFor = result.Standings.Sum(row => row.GoalsFor);
        int sumAgainst = result.Standings.Sum(row => row.GoalsAgainst);
        Assert.Equal(sumFor, sumAgainst);
    }

    [Fact]
    public void ResultIsDeterministicForTheSameSeed()
    {
        var first = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 42L);
        var second = SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Test Cup",
            participants: FourClubs,
            seed: 42L);

        Assert.Equal(first.Standings.Length, second.Standings.Length);
        for (int i = 0; i < first.Standings.Length; i++)
        {
            Assert.Equal(first.Standings[i].Points, second.Standings[i].Points);
            Assert.Equal(first.Standings[i].GoalsFor, second.Standings[i].GoalsFor);
            Assert.Equal(first.Standings[i].GoalsAgainst, second.Standings[i].GoalsAgainst);
        }
    }

    [Fact]
    public void RejectsFewerThanTwoParticipants()
    {
        Assert.Throws<ArgumentException>(() => SeasonSimulator.Simulate(
            competitionExternalId: "competition:test",
            competitionDisplayName: "Solo",
            participants: new[] { FourClubs[0] },
            seed: 1L));
    }
}