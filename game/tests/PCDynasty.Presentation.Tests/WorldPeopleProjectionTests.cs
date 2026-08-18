using System.IO;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The shipped starter world holds eleven players and seven managers, so it is
/// the natural fixture for the rule audit a00001 found broken: the squad used
/// to be every person referencing the club, which put managers on the pitch
/// with zero-valued attributes.
/// </summary>
public sealed class WorldPeopleProjectionTests
{
    private static readonly string ContentRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    private static readonly string[] Clubs =
    [
        "club:aurora-quay",
        "club:cinder-valley",
        "club:forge-lantern",
        "club:maris-cobalt",
        "club:northglass-rovers",
        "club:solspire-azul",
    ];

    private static async Task<WorldService> LoadAsync()
    {
        var service = new WorldService(new JsonContentReader(), ContentRoot);
        await service.ReloadAsync();
        return service;
    }

    [Fact]
    public async Task SquadsHoldEveryPlayerAndNoManager()
    {
        WorldService world = await LoadAsync();

        int players = Clubs.Sum(club => world.ListPlayers(club).Count);
        int managers = Clubs.Sum(club => world.ListManagers(club).Count);

        Assert.Equal(11, players);
        Assert.Equal(7, managers);
    }

    [Fact]
    public async Task ManagersNeverAppearInASquad()
    {
        WorldService world = await LoadAsync();

        IReadOnlyList<PlayerRow> squad = world.ListPlayers("club:aurora-quay");

        Assert.Equal(2, squad.Count);
        Assert.DoesNotContain(squad, player => player.ExternalId == "person:maela-dorn");
        Assert.All(squad, player => Assert.True(player.Overall > 0));
    }

    [Fact]
    public async Task ManagersAreProjectedWithTheirOwnReadModel()
    {
        WorldService world = await LoadAsync();

        ManagerRow manager = Assert.Single(world.ListManagers("club:aurora-quay"));

        Assert.Equal("person:maela-dorn", manager.ExternalId);
        Assert.Equal("Maela Dorn", manager.DisplayName);
        Assert.Equal("Norvane", manager.CountryShortName);
        Assert.True(manager.Reputation > 0);
    }

    [Fact]
    public async Task PlayersCarryTheirDeclaredCountry()
    {
        WorldService world = await LoadAsync();

        PlayerRow player = Assert.Single(
            world.ListPlayers("club:aurora-quay"),
            candidate => candidate.ExternalId == "person:alven-roake");

        Assert.Equal("Norvane", player.CountryShortName);
    }

    [Fact]
    public async Task StaffIsEmptyUntilTheContentDeclaresOtherOccupations()
    {
        WorldService world = await LoadAsync();

        Assert.All(Clubs, club => Assert.Empty(world.ListStaff(club)));
    }

    [Fact]
    public async Task SquadSizeCountsPlayersOnly()
    {
        WorldService world = await LoadAsync();

        ClubRow? club = world.FindClub("club:cinder-valley");

        Assert.NotNull(club);
        Assert.Equal(1, club!.SquadSize);
        Assert.Equal(2, world.ListManagers("club:cinder-valley").Count);
    }
}
