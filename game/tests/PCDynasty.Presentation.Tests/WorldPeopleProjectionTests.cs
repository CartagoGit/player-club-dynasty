using System.IO;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The shipped world fields full squads plus managers and support staff, so it
/// is the natural fixture for the rule audit a00001 found broken: the squad
/// used to be every person referencing the club, which put managers on the
/// pitch with zero-valued attributes.
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
    public async Task EveryClubFieldsASquadWithoutItsManager()
    {
        WorldService world = await LoadAsync();

        foreach (string club in Clubs)
        {
            IReadOnlyList<PlayerRow> squad = world.ListPlayers(club);
            IReadOnlyList<ManagerRow> managers = world.ListManagers(club);
            Assert.True(squad.Count >= 22, $"{club} fields only {squad.Count} players.");
            Assert.NotEmpty(managers);
            Assert.All(squad, player => Assert.True(player.Overall > 0));
            Assert.DoesNotContain(
                squad,
                player => managers.Any(manager => manager.ExternalId == player.ExternalId));
        }
    }

    [Fact]
    public async Task ManagersNeverAppearInASquad()
    {
        WorldService world = await LoadAsync();

        IReadOnlyList<PlayerRow> squad = world.ListPlayers("club:aurora-quay");

        Assert.DoesNotContain(squad, player => player.ExternalId == "person:maela-dorn");
        Assert.Contains(squad, player => player.ExternalId == "person:alven-roake");
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
    public async Task StaffIsProjectedByItsDeclaredRole()
    {
        WorldService world = await LoadAsync();

        IReadOnlyList<StaffRow> staff = world.ListStaff("club:aurora-quay");

        Assert.Equal(2, staff.Count);
        Assert.Contains(staff, member => member.Role == "scout");
        Assert.Contains(staff, member => member.Role == "physio");
        Assert.DoesNotContain(world.ListPlayers("club:aurora-quay"), player =>
            staff.Any(member => member.ExternalId == player.ExternalId));
    }

    [Fact]
    public async Task SquadSizeCountsPlayersOnly()
    {
        WorldService world = await LoadAsync();

        ClubRow? club = world.FindClub("club:cinder-valley");

        Assert.NotNull(club);
        Assert.Equal(world.ListPlayers("club:cinder-valley").Count, club!.SquadSize);
        // Cinder Valley is the one club the shipped world gives two managers.
        Assert.Equal(2, world.ListManagers("club:cinder-valley").Count);
    }
}
