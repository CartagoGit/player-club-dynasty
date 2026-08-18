using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class WorldScreen
{
    private readonly WorldService _world;

    internal WorldScreen(WorldService world)
    {
        _world = world;
    }

    internal void Render(Control root)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        IReadOnlyList<CompetitionRow> competitions = _world.ListAllCompetitions();

        var header = new Label { Text = $"MUNDIAL DE COMPETICIONES · {competitions.Count} LIGAS Y COPAS" };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        int participants = competitions
            .SelectMany(competition => competition.Rivals)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var summary = new Label { Text = $"{participants} clubes inscritos en al menos una competición" };
        summary.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        summary.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(summary);
        layout.AddChild(new HSeparator());

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "COMPETICIÓN", 260);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "FORMATO", 120);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "FRECUENCIA", 120);
        WorkspaceUiFactory.AddColumnHeader(headerRow, "RIVALES", 80, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        foreach (CompetitionRow competition in competitions)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
            WorkspaceUiFactory.AddColumnValue(row, competition.DisplayName, 260, ManagerDesignTokens.TextPrimary);
            WorkspaceUiFactory.AddColumnValue(row, competition.Format, 120, ManagerDesignTokens.TextSecondary);
            WorkspaceUiFactory.AddColumnValue(row, competition.Frequency, 120, ManagerDesignTokens.TextSecondary);
            WorkspaceUiFactory.AddColumnValue(row, competition.Rivals.Count.ToString(WorkspaceUiFactory.InvariantCulture), 80, ManagerDesignTokens.Accent, HorizontalAlignment.Right);
            layout.AddChild(row);
        }

        root.AddChild(layout);
    }
}
