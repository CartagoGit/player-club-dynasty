using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core.Localization;
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
    private readonly WorldSection _texts;

    internal WorldScreen(IManagerWorld world, WorldSection texts)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
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
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _texts.SummaryFormat,
            new[]
            {
                competitions.Count.ToString(WorkspaceUiFactory.InvariantCulture),
                participants.ToString(WorkspaceUiFactory.InvariantCulture),
            });
        layout.AddChild(header);

        var list = new ListSection { Name = "WorldCompetitions" };
        list.SetEmptyText(_texts.Empty);
        list.AddColumnHeader(_texts.Columns.Name, ColumnName);
        list.AddColumnHeader(_texts.Columns.Format, ColumnFormat);
        list.AddColumnHeader(_texts.Columns.Frequency, ColumnFrequency);
        list.AddColumnHeader(_texts.Columns.Rivals, ColumnRivals, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Action, ColumnAction, HorizontalAlignment.Right);

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

    private ZebraRow BuildRow(CompetitionRow competition)
    {
        var row = new ZebraRow(0);
        row.AddCell(competition.DisplayName, ColumnName, ManagerDesignTokens.TextPrimary);
        row.AddCell(competition.Format, ColumnFormat, ManagerDesignTokens.TextSecondary);
        row.AddCell(competition.Frequency, ColumnFrequency, ManagerDesignTokens.TextSecondary);
        row.AddCell(competition.Rivals.Count.ToString(WorkspaceUiFactory.InvariantCulture), ColumnRivals, ManagerDesignTokens.Accent, HorizontalAlignment.Right);
        row.AddAction(
            _texts.Actions.Inspect,
            _texts.Actions.InspectTooltip,
            _ => { /* future slice: open competition detail */ },
            enabled: false);
        return row;
    }
}
