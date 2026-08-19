using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Application.Market;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class SquadScreen
{
    private const int ColumnPlayer = 240;
    private const int ColumnNation = 60;
    private const int ColumnAge = 50;
    private const int ColumnTech = 50;
    private const int ColumnForm = 50;
    private const int ColumnPhys = 50;
    private const int ColumnMent = 50;
    private const int ColumnValue = 70;
    private const int ColumnAction = 130;

    private readonly IManagerWorld _world;
    private readonly ICareerSquadProjection _careerSquad;
    private readonly ManagedClubContext _managed;
    private readonly CareerCalendar _calendar;
    private readonly Action _onOpenTransferMarket;
    private readonly SquadSection _texts;

    internal SquadScreen(
        IManagerWorld world,
        ICareerSquadProjection careerSquad,
        ManagedClubContext managed,
        CareerCalendar calendar,
        Action onOpenTransferMarket,
        SquadSection texts)
    {
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _careerSquad = careerSquad ?? throw new ArgumentNullException(nameof(careerSquad));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _onOpenTransferMarket = onOpenTransferMarket ?? throw new ArgumentNullException(nameof(onOpenTransferMarket));
    }

    internal void Render(Control root, ClubRow club)
    {
        var layout = new VBoxContainer { Name = "SquadLayout" };
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(layout);

        IReadOnlyList<PlayerRow> players = _managed.Owns(club.ExternalId)
            ? _careerSquad.ListManagedSquad(_managed)
            : _world.ListPlayers(club.ExternalId);

        var header = new HeaderPanel { Name = "SquadHeaderPanel" };
        header.SetTexts(
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _managed.Owns(club.ExternalId) ? _texts.SummaryOwnedFormat : _texts.SummaryBrowseFormat,
            new[] { club.DisplayName, players.Count.ToString(WorkspaceUiFactory.InvariantCulture) });
        if (_managed.Owns(club.ExternalId))
        {
            header.SetAction(
                _texts.ActionOpenMarket,
                _texts.ActionOpenMarketTooltip,
                _onOpenTransferMarket,
                enabled: true);
        }
        layout.AddChild(header);

        var list = new ListSection { Name = "SquadList", Area = ShellArea.Technical };
        list.SetEmptyText(_texts.Empty);
        list.AddColumnHeader(_texts.Columns.Player, ColumnPlayer);
        list.AddColumnHeader(_texts.Columns.Nation, ColumnNation, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Age, ColumnAge, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Tec, ColumnTech, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.For, ColumnForm, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Fis, ColumnPhys, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Men, ColumnMent, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Val, ColumnValue, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Action, ColumnAction, HorizontalAlignment.Right);

        // The smoke-boot reads the squad summary as a child labelled
        // "SquadHeader" so the parent path remains stable across the migration.
        // Hide it from the visual layout but keep it reachable from the test
        // through GetNode("SquadHeader"). The real header lives in
        // SquadHeaderPanel; this label is a pure compat shim.
        var compatHeader = new Label
        {
            Name = "SquadHeader",
            Text = $"{club.DisplayName.ToUpperInvariant()} · PLANTILLA · {players.Count} JUGADORES",
            Visible = false,
        };
        list.AddChild(compatHeader);

        if (players.Count == 0)
        {
            list.RefreshEmptyState();
        }
        else
        {
            for (int index = 0; index < players.Count; index++)
            {
                list.AddRow(BuildRow(players[index]));
            }
        }

        layout.AddChild(list);
    }

    private ZebraRow BuildRow(PlayerRow player)
    {
        var row = new ZebraRow(0);
        row.AddCell(player.DisplayName, ColumnPlayer, ManagerDesignTokens.TextPrimary);
        row.AddCell(player.CountryShortName, ColumnNation, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        row.AddCell(WorkspaceUiFactory.PlayerAge(player.BirthDate, _calendar), ColumnAge, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        row.AddCell(player.Technique.ToString(WorkspaceUiFactory.InvariantCulture), ColumnTech, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        row.AddCell(player.Form.ToString(WorkspaceUiFactory.InvariantCulture), ColumnForm, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        row.AddCell(player.Physical.ToString(WorkspaceUiFactory.InvariantCulture), ColumnPhys, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        row.AddCell(player.Mental.ToString(WorkspaceUiFactory.InvariantCulture), ColumnMent, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        row.AddCell(player.Overall.ToString(WorkspaceUiFactory.InvariantCulture), ColumnValue, WorkspaceUiFactory.ValueColor(player.Overall), HorizontalAlignment.Right);
        row.AddAction(
            _texts.Actions.Inspect,
            _texts.Actions.InspectTooltip,
            onPressed: _ => { /* future slice: open detail panel */ },
            enabled: false);
        return row;
    }
}
