using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class SquadScreen
{
    private readonly WorldService _world;

    internal SquadScreen(WorldService world)
    {
        _world = world;
    }

    internal void Render(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new Label { Text = $"{club.DisplayName.ToUpperInvariant()} · PLANTILLA · {club.SquadSize} JUGADORES" };
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

        IReadOnlyList<PlayerRow> players = _world.ListPlayers(club.ExternalId);
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

    private static HBoxContainer BuildRow(PlayerRow player)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnValue(row, player.DisplayName, 280, ManagerDesignTokens.TextPrimary);
        WorkspaceUiFactory.AddColumnValue(row, WorkspaceUiFactory.PlayerCountry(player.ExternalId), 80, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, WorkspaceUiFactory.PlayerAge(player.BirthYear), 60, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, player.Technique.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Form.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Physical.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Mental.ToString(WorkspaceUiFactory.InvariantCulture), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, player.Overall.ToString(WorkspaceUiFactory.InvariantCulture), 80, WorkspaceUiFactory.ValueColor(player.Overall), HorizontalAlignment.Right);
        return row;
    }
}
