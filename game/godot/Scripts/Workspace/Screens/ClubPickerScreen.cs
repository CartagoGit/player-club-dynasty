using Godot;
using PCDynasty.Application.Workspace;
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
    private readonly Func<string, string> _tr;

    internal ClubPickerScreen(Action<string> selectClub, Func<string, string> tr)
    {
        _selectClub = selectClub ?? throw new ArgumentNullException(nameof(selectClub));
        _tr = tr ?? throw new ArgumentNullException(nameof(tr));
    }

    internal void Render(Control root, IReadOnlyList<ClubRow> clubs, string? managedClubId)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(layout);

        var header = new HeaderPanel { Name = "ClubPickerHeaderPanel" };
        header.SetTexts(
            "PICKER_EYEBROW",
            "PICKER_TITLE",
            "PICKER_SUMMARY",
            new[] { clubs.Count.ToString(WorkspaceUiFactory.InvariantCulture) });
        layout.AddChild(header);

        var list = new ListSection { Name = "ClubPickerRows" };
        list.SetEmptyText("PICKER_EMPTY");
        list.AddColumnHeader("PICKER_COL_CLUB", ColumnClub);
        list.AddColumnHeader("PICKER_COL_COUNTRY", ColumnCountry);
        list.AddColumnHeader("PICKER_COL_FOUNDED", ColumnFounded);
        list.AddColumnHeader("PICKER_COL_PRESTIGE", ColumnPrestige, HorizontalAlignment.Right);
        list.AddColumnHeader("PICKER_COL_FANBASE", ColumnFanbase, HorizontalAlignment.Right);
        list.AddColumnHeader("PICKER_COL_SQUAD", ColumnSquad, HorizontalAlignment.Right);
        list.AddColumnHeader("PICKER_COL_ACTION", ColumnAction, HorizontalAlignment.Right);

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
            ? _tr("PICKER_EYEBROW_OWNED_BADGE") + " · "
            : string.Empty;
        row.AddCell(prefix + club.DisplayName, ColumnClub, ManagerDesignTokens.TextPrimary);
        row.AddCell(club.CountryShortName, ColumnCountry, ManagerDesignTokens.TextSecondary);
        row.AddCell(club.FoundedYear, ColumnFounded, ManagerDesignTokens.TextSecondary);
        row.AddCell(club.Prestige.ToString(WorkspaceUiFactory.InvariantCulture), ColumnPrestige, WorkspaceUiFactory.ValueColor(club.Prestige), HorizontalAlignment.Right);
        row.AddCell(club.Fanbase.ToString(WorkspaceUiFactory.InvariantCulture), ColumnFanbase, WorkspaceUiFactory.ValueColor(club.Fanbase), HorizontalAlignment.Right);
        row.AddCell(club.SquadSize.ToString(WorkspaceUiFactory.InvariantCulture), ColumnSquad, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        string clubId = club.ExternalId;
        row.AddAction(
            "PICKER_ROW_CHOOSE",
            "PICKER_ROW_CHOOSE_TOOLTIP",
            _ => _selectClub(clubId),
            enabled: !string.Equals(club.ExternalId, managedClubId, StringComparison.Ordinal));
        return row;
    }
}
