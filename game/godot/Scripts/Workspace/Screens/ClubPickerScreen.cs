using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class ClubPickerScreen
{
    private readonly Action<string> _selectClub;

    internal ClubPickerScreen(Action<string> selectClub)
    {
        _selectClub = selectClub;
    }

    internal void Render(Control root, IReadOnlyList<ClubRow> clubs)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new Label { Text = $"SELECCIONA UN CLUB · {clubs.Count} DISPONIBLES" };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "CLUB", 260);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "PAÍS", 100);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "FUND.", 60);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "PREST.", 70, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "AFIC.", 70, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "PLANT.", 70, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "ACCIÓN", 110, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        foreach (ClubRow club in clubs) layout.AddChild(BuildRow(club));
        root.AddChild(layout);
    }

    private HBoxContainer BuildRow(ClubRow club)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnValue(row, club.DisplayName, 260, ManagerDesignTokens.TextPrimary);
        WorkspaceUiFactory.AddColumnValue(row, club.CountryShortName, 100, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, club.FoundedYear, 60, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, club.Prestige.ToString(WorkspaceUiFactory.InvariantCulture), 70, WorkspaceUiFactory.ValueColor(club.Prestige), HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, club.Fanbase.ToString(WorkspaceUiFactory.InvariantCulture), 70, WorkspaceUiFactory.ValueColor(club.Fanbase), HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, club.SquadSize.ToString(WorkspaceUiFactory.InvariantCulture), 70, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);

        var button = new Button { Text = "Elegir" };
        button.AddThemeFontSizeOverride("font_size", 12);
        string clubId = club.ExternalId;
        button.Pressed += () => _selectClub(clubId);
        var cell = new HBoxContainer { CustomMinimumSize = new Vector2(110, 0) };
        cell.AddChild(button);
        row.AddChild(cell);
        return row;
    }
}
