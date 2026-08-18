using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal static class MatchReportView
{
    internal static VBoxContainer Build(
        MatchResult result,
        string competitionDisplayName,
        Action replay,
        Action back)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        layout.AddChild(BuildScoreboard(result));

        MatchStatsRow stats = MatchStats.Compute(result);
        var meta = new Label
        {
            Text = $"{competitionDisplayName} · seed {result.Seed} · {result.Events.Count} intentos · {result.Events.Count(entry => entry.IsGoal)} goles",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(meta);
        layout.AddChild(BuildStats(stats));
        AddGoals(layout, result);
        layout.AddChild(BuildActions(replay, back));
        return layout;
    }

    private static HBoxContainer BuildScoreboard(MatchResult result)
    {
        var scoreboard = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        scoreboard.AddThemeConstantOverride("separation", ManagerDesignTokens.Space24);
        scoreboard.AddChild(CreateScoreLabel(result.HomeClub.ShortName.ToUpperInvariant(), 22, ManagerDesignTokens.TextPrimary));
        scoreboard.AddChild(CreateScoreLabel($"{result.HomeGoals} - {result.AwayGoals}", 32, ManagerDesignTokens.Accent));
        scoreboard.AddChild(CreateScoreLabel(result.AwayClub.ShortName.ToUpperInvariant(), 22, ManagerDesignTokens.TextPrimary));
        return scoreboard;
    }

    private static Label CreateScoreLabel(string text, int size, Color color)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static PanelContainer BuildStats(MatchStatsRow stats)
    {
        PanelContainer panel = WorkspaceUiFactory.CreateSurfacePanel();
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var title = new Label { Text = "ESTADÍSTICAS" };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 14);
        column.AddChild(title);
        column.AddChild(BuildStatRow("Tiros", stats.HomeShots, stats.AwayShots));
        column.AddChild(BuildStatRow("Paradas", stats.HomeSaves, stats.AwaySaves));
        column.AddChild(BuildStatRow("Fuera", stats.HomeMisses, stats.AwayMisses));
        column.AddChild(BuildStatRow("Tarjetas", stats.HomeCards, stats.AwayCards));
        column.AddChild(BuildStatRow("Posesión", $"{stats.HomePossession}%", $"{stats.AwayPossession}%"));
        panel.AddChild(column);
        return panel;
    }

    private static HBoxContainer BuildStatRow(string label, int home, int away) =>
        BuildStatRow(
            label,
            home.ToString(WorkspaceUiFactory.InvariantCulture),
            away.ToString(WorkspaceUiFactory.InvariantCulture));

    private static HBoxContainer BuildStatRow(string label, string home, string away)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnValue(row, label, 140, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, home, 80, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, "-", 20, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Center);
        WorkspaceUiFactory.AddColumnValue(row, away, 80, ManagerDesignTokens.TextPrimary);
        return row;
    }

    private static void AddGoals(VBoxContainer layout, MatchResult result)
    {
        var title = new Label { Text = "GOLES" };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 14);
        layout.AddChild(title);

        MatchEvent[] goals = result.Events.Where(entry => entry.IsGoal).OrderBy(entry => entry.Minute).ToArray();
        if (goals.Length == 0)
        {
            layout.AddChild(WorkspaceUiFactory.CreateText("Sin goles — portería a cero", ManagerDesignTokens.TextSecondary));
            return;
        }

        foreach (MatchEvent goal in goals)
        {
            var row = new Label { Text = $"  Min {goal.Minute,3}' · {goal.ActorShortName}" };
            row.AddThemeColorOverride("font_color", ManagerDesignTokens.Positive);
            row.AddThemeFontSizeOverride("font_size", 14);
            layout.AddChild(row);
        }
    }

    private static HBoxContainer BuildActions(Action replay, Action back)
    {
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        var replayButton = new Button { Text = "  ↻ Repetir (misma seed)", Alignment = HorizontalAlignment.Left };
        replayButton.Pressed += replay;
        actions.AddChild(replayButton);
        var backButton = new Button { Text = "  ← Volver a competiciones", Alignment = HorizontalAlignment.Left };
        backButton.Pressed += back;
        actions.AddChild(backButton);
        return actions;
    }
}
