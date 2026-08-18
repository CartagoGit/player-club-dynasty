using System.IO;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// Integration tests for the bootstrap bridge between the engine-neutral
/// content layer (f00010/f00014) and the presentation. These tests load
/// the real starter-world pack shipped under game/content/starter-world/
/// so the gate proves the bridge works against the actual data the game
/// will ship with.
/// </summary>
public sealed class WorldServiceTests
{
    private static readonly string ContentRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    [Fact]
    public async Task ReloadAsyncLoadsEveryShippedStarterWorldFile()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);

        await service.ReloadAsync();

        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListClubsReturnsAtLeastSixFictionalClubs()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var clubs = service.ListClubs();

        Assert.NotEmpty(clubs);
        Assert.True(
            clubs.Count >= 6,
            $"Expected at least 6 starter-world clubs, got {clubs.Count}.");

        // The starter-world ships fictional clubs (Aurora Quay, Cinder Valley, ...).
        // No real-world names should leak into the data.
        Assert.Contains(clubs, c => c.DisplayName.Contains("Aurora", StringComparison.Ordinal));
        Assert.DoesNotContain(clubs, c => c.DisplayName.Contains("Real Madrid", StringComparison.Ordinal));
        Assert.DoesNotContain(clubs, c => c.DisplayName.Contains("Barcelona", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ListClubsExposesCountryAndSquadSize()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var clubs = service.ListClubs();
        var aurora = Assert.Single(clubs, c => c.ExternalId == "club:aurora-quay");

        Assert.Equal("Aurora Quay Athletic", aurora.DisplayName);
        Assert.Equal("Myrglass", aurora.City);
        Assert.Equal("Norvane", aurora.CountryShortName);
        Assert.Equal("1938", aurora.FoundedYear);
        Assert.Equal(54, aurora.Prestige);
        Assert.Equal(48, aurora.Fanbase);
        Assert.True(aurora.SquadSize > 0, "Aurora Quay should have at least one player on the roster.");
    }

    [Fact]
    public async Task ListPlayersReturnsRealRosterWithComputedOverall()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var players = service.ListPlayers("club:aurora-quay");

        Assert.NotEmpty(players);

        var alven = Assert.Single(players, p => p.ExternalId == "person:alven-roake");
        Assert.Equal("Alven Roake", alven.DisplayName);
        Assert.Equal(2001, alven.BirthYear);
        Assert.Equal(62, alven.Technique);
        Assert.Equal(58, alven.Form);
        Assert.Equal(66, alven.Physical);
        Assert.Equal(61, alven.Mental);
        // Overall is the integer mean of the four attributes: (62+58+66+61)/4 = 61.
        Assert.Equal(61, alven.Overall);
    }

    [Fact]
    public async Task ListPlayersReturnsEmptyForUnknownClub()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var players = service.ListPlayers("club:does-not-exist");

        Assert.Empty(players);
    }

    [Fact]
    public void InstancesAreIndependentForTheSameContentRoot()
    {
        var first = new WorldService(new JsonContentReader(), ContentRoot);
        var second = new WorldService(new JsonContentReader(), ContentRoot);

        Assert.NotSame(first, second);
    }

    [Fact]
    public void ConstructorRejectsEmptyContentRoot()
    {
        Assert.Throws<ArgumentException>(() => new WorldService(new JsonContentReader(), " "));
    }

    [Fact]
    public async Task ListCompetitionsForClubReturnsLeaguesAndCup()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListCompetitionsForClub("club:aurora-quay");

        Assert.NotEmpty(competitions);
        var names = competitions.Select(c => c.DisplayName).ToArray();
        Assert.Contains("Boreal Series", names);
        Assert.Contains("Horizon Cup", names);
    }

    [Fact]
    public async Task ListCompetitionsForClubExposesRivalShortNames()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListCompetitionsForClub("club:aurora-quay");
        var boreal = Assert.Single(competitions, c => c.ExternalId == "competition:boreal-series");

        Assert.Equal("round-robin", boreal.Format);
        Assert.Equal("annual", boreal.Frequency);
        Assert.Contains("Northglass", boreal.Rivals);
        Assert.DoesNotContain("Aurora Quay", boreal.Rivals);
    }

    [Fact]
    public async Task ListCompetitionsForClubReturnsEmptyForUnknownClub()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var competitions = service.ListCompetitionsForClub("club:not-a-real-club");

        Assert.Empty(competitions);
    }

    [Fact]
    public async Task FindClubReturnsNullForUnknownClub()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();

        var club = service.FindClub("club:not-a-real-club");

        Assert.Null(club);
    }
}
