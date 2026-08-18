using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal static class SeasonReportView
{
    internal static VBoxContainer Build(ClubRow club, SeasonResult season)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new Label { Text = $"{season.CompetitionDisplayName.ToUpperInvariant()} · CLASIFICACIÓN FINAL" };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var meta = new Label
        {
            Text = $"{season.Standings.Length} clubes · {season.Fixtures.Length} partidos · simulación determinista",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(meta);
        layout.AddChild(new HSeparator());
        layout.AddChild(BuildStandingsHeader());
        layout.AddChild(new HSeparator());

        for (int index = 0; index < season.Standings.Length; index++)
        {
            layout.AddChild(BuildStandingRow(index + 1, season.Standings[index], club.ExternalId));
        }

        layout.AddChild(new HSeparator());
        var fixturesTitle = new Label { Text = "RESULTADOS" };
        fixturesTitle.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        fixturesTitle.AddThemeFontSizeOverride("font_size", 14);
        layout.AddChild(fixturesTitle);
        foreach (SeasonFixture fixture in season.Fixtures)
        {
            layout.AddChild(BuildFixtureRow(fixture, club.ExternalId));
        }

        return layout;
    }

    internal static int PositionOf(SeasonResult season, string clubExternalId)
    {
        for (int index = 0; index < season.Standings.Length; index++)
        {
            if (string.Equals(season.Standings[index].ClubExternalId, clubExternalId, StringComparison.Ordinal))
            {
                return index + 1;
            }
        }

        return 0;
    }

    private static HBoxContainer BuildStandingsHeader()
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        WorkspaceUiFactory.AddColumnHeader(row, "#", 30);
        WorkspaceUiFactory.AddColumnHeader(row, "CLUB", 260);
        WorkspaceUiFactory.AddColumnHeader(row, "PTS", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "PJ", 50, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "G", 50, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "E", 50, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "P", 50, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "GF", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "GC", 60, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "DG", 60, HorizontalAlignment.Right);
        return row;
    }

    private static HBoxContainer BuildStandingRow(int position, StandingRow standing, string managedClubId)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        bool isManaged = string.Equals(standing.ClubExternalId, managedClubId, StringComparison.Ordinal);
        Color primary = isManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextPrimary;
        Color secondary = isManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextSecondary;
        WorkspaceUiFactory.AddColumnValue(row, position.ToString(WorkspaceUiFactory.InvariantCulture), 30, secondary);
        WorkspaceUiFactory.AddColumnValue(row, standing.DisplayName, 260, primary);
        WorkspaceUiFactory.AddColumnValue(row, standing.Points.ToString(WorkspaceUiFactory.InvariantCulture), 60, primary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.Played.ToString(WorkspaceUiFactory.InvariantCulture), 50, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.Wins.ToString(WorkspaceUiFactory.InvariantCulture), 50, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.Draws.ToString(WorkspaceUiFactory.InvariantCulture), 50, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.Losses.ToString(WorkspaceUiFactory.InvariantCulture), 50, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.GoalsFor.ToString(WorkspaceUiFactory.InvariantCulture), 60, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.GoalsAgainst.ToString(WorkspaceUiFactory.InvariantCulture), 60, secondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, standing.GoalDifference.ToString("+#;-#;0", WorkspaceUiFactory.InvariantCulture), 60, WorkspaceUiFactory.DifferenceColor(standing.GoalDifference), HorizontalAlignment.Right);
        return row;
    }

    private static HBoxContainer BuildFixtureRow(SeasonFixture fixture, string managedClubId)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        Color home = string.Equals(fixture.HomeClubId, managedClubId, StringComparison.Ordinal)
            ? ManagerDesignTokens.Accent
            : ManagerDesignTokens.TextPrimary;
        Color away = string.Equals(fixture.AwayClubId, managedClubId, StringComparison.Ordinal)
            ? ManagerDesignTokens.Accent
            : ManagerDesignTokens.TextPrimary;
        WorkspaceUiFactory.AddColumnValue(row, fixture.HomeDisplayName, 260, home);
        WorkspaceUiFactory.AddColumnValue(row, fixture.HomeGoals.ToString(WorkspaceUiFactory.InvariantCulture), 40, home, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, "-", 20, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Center);
        WorkspaceUiFactory.AddColumnValue(row, fixture.AwayGoals.ToString(WorkspaceUiFactory.InvariantCulture), 40, away);
        WorkspaceUiFactory.AddColumnValue(row, fixture.AwayDisplayName, 260, away);
        return row;
    }
}
