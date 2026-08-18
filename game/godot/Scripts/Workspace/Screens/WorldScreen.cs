using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class WorldScreen
{
    private const int ColumnName = 260;
    private const int ColumnFormat = 120;
    private const int ColumnFrequency = 120;
    private const int ColumnRivals = 80;
    private const int ColumnAction = 110;

    private readonly IManagerWorld _world;
    private readonly Func<string, string> _tr;

    internal WorldScreen(IManagerWorld world, Func<string, string> tr)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _tr = tr ?? throw new ArgumentNullException(nameof(tr));
    }

    internal void Render(Control root)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(layout);

        IReadOnlyList<CompetitionRow> competitions = _world.ListAllCompetitions();
        int participants = competitions
            .SelectMany(competition => competition.Rivals)
            .Distinct(StringComparer.Ordinal)
            .Count();

        var header = new HeaderPanel { Name = "WorldHeaderPanel" };
        header.SetTexts(
            "WORLD_EYEBROW",
            "WORLD_TITLE",
            "WORLD_SUMMARY",
            new[]
            {
                competitions.Count.ToString(WorkspaceUiFactory.InvariantCulture),
                participants.ToString(WorkspaceUiFactory.InvariantCulture),
            });
        layout.AddChild(header);

        var list = new ListSection { Name = "WorldCompetitions" };
        list.SetEmptyText("WORLD_EMPTY");
        list.AddColumnHeader("WORLD_COL_NAME", ColumnName);
        list.AddColumnHeader("WORLD_COL_FORMAT", ColumnFormat);
        list.AddColumnHeader("WORLD_COL_FREQUENCY", ColumnFrequency);
        list.AddColumnHeader("WORLD_COL_RIVALS", ColumnRivals, HorizontalAlignment.Right);
        list.AddColumnHeader("WORLD_COL_ACTION", ColumnAction, HorizontalAlignment.Right);

        if (competitions.Count == 0)
        {
            list.RefreshEmptyState();
        }
        else
        {
            foreach (CompetitionRow competition in competitions)
            {
                list.AddRow(BuildRow(competition));
            }
        }
        layout.AddChild(list);
    }

    private static ZebraRow BuildRow(CompetitionRow competition)
    {
        var row = new ZebraRow(0);
        row.AddCell(competition.DisplayName, ColumnName, ManagerDesignTokens.TextPrimary);
        row.AddCell(competition.Format, ColumnFormat, ManagerDesignTokens.TextSecondary);
        row.AddCell(competition.Frequency, ColumnFrequency, ManagerDesignTokens.TextSecondary);
        row.AddCell(competition.Rivals.Count.ToString(WorkspaceUiFactory.InvariantCulture), ColumnRivals, ManagerDesignTokens.Accent, HorizontalAlignment.Right);
        row.AddAction(
            "WORLD_ROW_INSPECT",
            "WORLD_ROW_INSPECT_TOOLTIP",
            _ => { /* future slice: open competition detail */ },
            enabled: false);
        return row;
    }
}
