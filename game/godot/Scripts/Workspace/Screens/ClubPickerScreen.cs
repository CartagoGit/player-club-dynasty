using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class ClubPickerScreen
{
    private const int ColumnClub = 260;
    private const int ColumnCountry = 100;
    private const int ColumnFounded = 60;
    private const int ColumnPrestige = 70;
    private const int ColumnFanbase = 70;
    private const int ColumnSquad = 70;
    private const int ColumnAction = 130;

    private readonly Action<string> _selectClub;
    private readonly PickerSection _texts;

    internal ClubPickerScreen(Action<string> selectClub, PickerSection texts)
    {
        _selectClub = selectClub ?? throw new ArgumentNullException(nameof(selectClub));
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
    }

    internal void Render(Control root, IReadOnlyList<ClubRow> clubs, string? managedClubId)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(layout);

        var header = new HeaderPanel { Name = "ClubPickerHeaderPanel" };
        header.SetTexts(
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _texts.SummaryFormat,
            new[] { clubs.Count.ToString(WorkspaceUiFactory.InvariantCulture) });
        layout.AddChild(header);

        var list = new ListSection { Name = "ClubPickerRows", Area = ShellArea.World };
        list.SetEmptyText(_texts.Empty);
        list.AddColumnHeader(_texts.Columns.Club, ColumnClub);
        list.AddColumnHeader(_texts.Columns.Country, ColumnCountry);
        list.AddColumnHeader(_texts.Columns.Founded, ColumnFounded);
        list.AddColumnHeader(_texts.Columns.Prestige, ColumnPrestige, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Fanbase, ColumnFanbase, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Squad, ColumnSquad, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Action, ColumnAction, HorizontalAlignment.Right);

        for (int index = 0; index < clubs.Count; index++)
        {
            list.AddRow(BuildRow(clubs[index], managedClubId));
        }
        if (clubs.Count == 0)
        {
            list.RefreshEmptyState();
        }
        layout.AddChild(list);
    }

    private ZebraRow BuildRow(ClubRow club, string? managedClubId)
    {
        var row = new ZebraRow(0);
        string prefix = string.Equals(club.ExternalId, managedClubId, StringComparison.Ordinal)
            ? (_texts.OwnedBadge) + " · "
            : string.Empty;
        row.AddCell(prefix + club.DisplayName, ColumnClub, ManagerDesignTokens.TextPrimary);
        row.AddCell(club.CountryShortName, ColumnCountry, ManagerDesignTokens.TextSecondary);
        row.AddCell(club.FoundedYear, ColumnFounded, ManagerDesignTokens.TextSecondary);
        row.AddCell(club.Prestige.ToString(WorkspaceUiFactory.InvariantCulture), ColumnPrestige, WorkspaceUiFactory.ValueColor(club.Prestige), HorizontalAlignment.Right);
        row.AddCell(club.Fanbase.ToString(WorkspaceUiFactory.InvariantCulture), ColumnFanbase, WorkspaceUiFactory.ValueColor(club.Fanbase), HorizontalAlignment.Right);
        row.AddCell(club.SquadSize.ToString(WorkspaceUiFactory.InvariantCulture), ColumnSquad, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        string clubId = club.ExternalId;
        row.AddAction(
            _texts.Actions.Choose,
            _texts.Actions.ChooseTooltip,
            _ => _selectClub(clubId),
            enabled: !string.Equals(club.ExternalId, managedClubId, StringComparison.Ordinal),
            name: "Choose");
        return row;
    }
}
