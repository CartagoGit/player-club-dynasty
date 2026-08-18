using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Application.Market;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class SquadScreen
{
    private readonly IManagerWorld _world;
    private readonly ICareerSquadProjection _careerSquad;
    private readonly ManagedClubContext _managed;
    private readonly CareerCalendar _calendar;

    internal SquadScreen(
        IManagerWorld world,
        ICareerSquadProjection careerSquad,
        ManagedClubContext managed,
        CareerCalendar calendar)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _careerSquad = careerSquad ?? throw new ArgumentNullException(nameof(careerSquad));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
    }

    internal void Render(Control root, ClubRow club)
    {
        // Browsing a rival shows the world roster; only the managed club sees
        // the career projection with its signings.
        IReadOnlyList<PlayerRow> players = _managed.Owns(club.ExternalId)
            ? _careerSquad.ListManagedSquad(_managed)
            : _world.ListPlayers(club.ExternalId);
        var layout = new VBoxContainer { Name = "SquadList" };
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new Label
        {
            Name = "SquadHeader",
            Text = $"{club.DisplayName.ToUpperInvariant()} · PLANTILLA · {players.Count} JUGADORES",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "JUGADOR", 280);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "NAC.", 80);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "EDAD", 60);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "TEC", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "FOR", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "FIS", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "MEN", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "VAL", 80, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        if (players.Count == 0)
        {
            layout.AddChild(WorkspaceUiFactory.CreateText("(sin jugadores inscritos)", ManagerDesignTokens.TextSecondary));
        }
        else
        {
            foreach (PlayerRow player in players) layout.AddChild(BuildRow(player));
        }

        root.AddChild(layout);
    }

    private HBoxContainer BuildRow(PlayerRow player)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnValue(row, player.DisplayName, 280, ManagerDesignTokens.TextPrimary);
        WorkspaceUiFactory.AddColumnValue(row, player.CountryShortName, 80, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, WorkspaceUiFactory.PlayerAge(player.BirthDate, _calendar), 60, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, player.Technique.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Form.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Physical.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Mental.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Overall.ToString(WorkspaceUiFactory.InvariantCulture), 80, WorkspaceUiFactory.ValueColor(player.Overall), HorizontalAlignment.Right);
        return row;
    }
}
