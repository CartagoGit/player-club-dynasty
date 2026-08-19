using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class HistoryScreen
{
    private const int ColumnWhen = 130;
    private const int ColumnCompetition = 200;
    private const int ColumnHome = 200;
    private const int ColumnScore = 50;
    private const int ColumnAway = 200;
    private const int ColumnSeed = 110;
    private const int ColumnAction = 100;

    private readonly IManagerWorld _world;
    private readonly CareerSession _careerSession;
    private readonly Action _refresh;
    private readonly HistorySection _texts;
    private string _competitionFilter = "(todas)";
    private string _clubFilter = "(todos)";
    private HistoryOrder _order = HistoryOrder.Date;

    internal HistoryScreen(
        IManagerWorld world,
        CareerSession careerSession,
        Action refresh,
        HistorySection texts)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _careerSession = careerSession ?? throw new ArgumentNullException(nameof(careerSession));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
    }

    internal void Render(Control root)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(layout);

        IReadOnlyList<HistoryEntry> entries = _careerSession.MatchSnapshot();
        string[] competitions = entries
            .Select(entry => entry.CompetitionDisplayName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        ClubRow[] clubs = _world.ListClubs().ToArray();

        var header = new HeaderPanel { Name = "HistoryHeaderPanel" };
        header.SetTexts(
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _texts.SummaryFormat,
            new[]
            {
                entries.Count.ToString(WorkspaceUiFactory.InvariantCulture),
                entries.Sum(entry => entry.HomeGoals + entry.AwayGoals).ToString(WorkspaceUiFactory.InvariantCulture),
                competitions.Length.ToString(WorkspaceUiFactory.InvariantCulture),
            });
        layout.AddChild(header);

        if (entries.Count == 0)
        {
            layout.AddChild(WorkspaceUiFactory.CreateText(
                (_texts.Empty),
                ManagerDesignTokens.TextSecondary));
            return;
        }

        layout.AddChild(BuildFilters(competitions, clubs));
        layout.AddChild(BuildAggregate(entries));

        var list = new ListSection { Name = "MatchList", Area = ShellArea.Tracking };
        list.SetEmptyText(_texts.FilterEmpty);
        list.AddColumnHeader(_texts.Columns.When, ColumnWhen);
        list.AddColumnHeader(_texts.Columns.Competition, ColumnCompetition);
        list.AddColumnHeader(_texts.Columns.Home, ColumnHome);
        list.AddColumnHeader(_texts.Columns.Score, ColumnScore, HorizontalAlignment.Center);
        list.AddColumnHeader(_texts.Columns.Away, ColumnAway);
        list.AddColumnHeader(_texts.Columns.Seed, ColumnSeed, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Action, ColumnAction, HorizontalAlignment.Right);

        HistoryEntry[] filtered = Order(Filter(entries)).ToArray();
        foreach (HistoryEntry entry in filtered)
        {
            list.AddRow(BuildRow(entry));
        }
        if (filtered.Length == 0)
        {
            list.RefreshEmptyState();
        }
        layout.AddChild(list);
    }

    private HBoxContainer BuildFilters(string[] competitions, ClubRow[] clubs)
    {
        var row = new HBoxContainer { Name = "HistoryFilters" };
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        row.AddChild(FilterLabel((_texts.Filters.CompetitionLabel)));
        var competitionButton = new OptionButton { Name = "CompetitionFilter" };
        competitionButton.AddItem((_texts.Filters.AllOption), 0);
        for (int index = 0; index < competitions.Length; index++) competitionButton.AddItem(competitions[index], index + 1);
        int selectedCompetition = Array.IndexOf(competitions, _competitionFilter);
        competitionButton.Selected = selectedCompetition < 0 ? 0 : selectedCompetition + 1;
        competitionButton.ItemSelected += idx =>
        {
            _competitionFilter = competitionButton.GetItemText((int)idx);
            _refresh();
        };
        row.AddChild(competitionButton);

        row.AddChild(FilterLabel((_texts.Filters.ClubLabel)));
        var clubButton = new OptionButton { Name = "ClubFilter" };
        clubButton.AddItem((_texts.Filters.AllOption), 0);
        for (int index = 0; index < clubs.Length; index++) clubButton.AddItem(clubs[index].DisplayName, index + 1);
        int selectedClub = Array.FindIndex(clubs, club => club.DisplayName == _clubFilter);
        clubButton.Selected = selectedClub < 0 ? 0 : selectedClub + 1;
        clubButton.ItemSelected += idx =>
        {
            _clubFilter = clubButton.GetItemText((int)idx);
            _refresh();
        };
        row.AddChild(clubButton);

        row.AddChild(FilterLabel((_texts.Filters.OrderLabel)));
        var orderButton = new OptionButton { Name = "OrderFilter" };
        orderButton.AddItem((_texts.Filters.OrderOptions.Date), 0);
        orderButton.AddItem((_texts.Filters.OrderOptions.Competition), 1);
        orderButton.AddItem((_texts.Filters.OrderOptions.Goals), 2);
        orderButton.Selected = (int)_order;
        orderButton.ItemSelected += idx =>
        {
            _order = (HistoryOrder)idx;
            _refresh();
        };
        row.AddChild(orderButton);
        return row;
    }

    private PanelContainer BuildAggregate(IReadOnlyList<HistoryEntry> entries)
    {
        var statistics = new Dictionary<string, ClubAggregate>(StringComparer.Ordinal);
        foreach (HistoryEntry entry in entries)
        {
            Accumulate(statistics, entry.HomeClubId, entry.HomeGoals, entry.AwayGoals);
            Accumulate(statistics, entry.AwayClubId, entry.AwayGoals, entry.HomeGoals);
        }

        string[] rows = statistics
            .Select(pair =>
            {
                ClubAggregate value = pair.Value;
                string clubName = _world.FindClub(pair.Key)?.DisplayName ?? pair.Key;
                int difference = value.GoalsFor - value.GoalsAgainst;
                return new
                {
                    Points = value.Wins * 3 + value.Draws,
                    Text = $"{clubName} · {value.Played}PJ {value.Wins}G {value.Draws}E {value.Losses}P · {value.GoalsFor}GF {value.GoalsAgainst}GC ({difference:+#;-#;0}) · {value.Wins * 3 + value.Draws} pts",
                };
            })
            .OrderByDescending(row => row.Points)
            .Take(8)
            .Select(row => row.Text)
            .ToArray();
        return WorkspaceUiFactory.CreatePanel(
            (_texts.Aggregate.Title),
            rows.Length == 0 ? new[] { (_texts.Aggregate.Empty) } : rows,
            ManagerDesignTokens.Accent);
    }

    private IEnumerable<HistoryEntry> Filter(IEnumerable<HistoryEntry> entries)
    {
        if (!string.Equals(_competitionFilter, "(todas)", StringComparison.Ordinal))
        {
            entries = entries.Where(entry => string.Equals(
                entry.CompetitionDisplayName,
                _competitionFilter,
                StringComparison.Ordinal));
        }

        if (!string.Equals(_clubFilter, "(todos)", StringComparison.Ordinal))
        {
            entries = entries.Where(entry =>
                string.Equals(entry.HomeDisplayName, _clubFilter, StringComparison.Ordinal)
                || string.Equals(entry.AwayDisplayName, _clubFilter, StringComparison.Ordinal));
        }

        return entries;
    }

    private IEnumerable<HistoryEntry> Order(IEnumerable<HistoryEntry> entries) => _order switch
    {
        HistoryOrder.Competition => entries
            .OrderBy(entry => entry.CompetitionDisplayName, StringComparer.Ordinal)
            .ThenByDescending(entry => entry.PlayedAtUtc),
        HistoryOrder.Goals => entries
            .OrderByDescending(entry => entry.HomeGoals + entry.AwayGoals)
            .ThenByDescending(entry => entry.PlayedAtUtc),
        _ => entries.OrderByDescending(entry => entry.PlayedAtUtc),
    };

    private static void Accumulate(
        IDictionary<string, ClubAggregate> statistics,
        string clubId,
        int goalsFor,
        int goalsAgainst)
    {
        statistics.TryGetValue(clubId, out ClubAggregate current);
        statistics[clubId] = current.Add(goalsFor, goalsAgainst);
    }

    private static Label FilterLabel(string text)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        label.AddThemeFontSizeOverride("font_size", 12);
        return label;
    }

    private ZebraRow BuildRow(HistoryEntry entry)
    {
        var row = new ZebraRow(0);
        row.AddCell(entry.PlayedAtUtc.ToLocalTime().ToString("HH:mm:ss", WorkspaceUiFactory.InvariantCulture), ColumnWhen, ManagerDesignTokens.TextSecondary);
        row.AddCell(entry.CompetitionDisplayName, ColumnCompetition, ManagerDesignTokens.TextSecondary);
        row.AddCell(entry.HomeDisplayName, ColumnHome, ManagerDesignTokens.TextPrimary);
        row.AddCell($"{entry.HomeGoals}-{entry.AwayGoals}", ColumnScore, ManagerDesignTokens.Accent, HorizontalAlignment.Center);
        row.AddCell(entry.AwayDisplayName, ColumnAway, ManagerDesignTokens.TextPrimary);
        row.AddCell(entry.Seed.ToString(WorkspaceUiFactory.InvariantCulture), ColumnSeed, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        row.AddAction(
            _texts.Actions.View,
            _texts.Actions.ViewTooltip,
            _ => { /* future slice: open match replay view */ },
            enabled: false);
        return row;
    }

    private enum HistoryOrder
    {
        Date,
        Competition,
        Goals,
    }

    private readonly record struct ClubAggregate(
        int Played,
        int Wins,
        int Draws,
        int Losses,
        int GoalsFor,
        int GoalsAgainst)
    {
        internal ClubAggregate Add(int goalsFor, int goalsAgainst) => new(
            Played + 1,
            Wins + (goalsFor > goalsAgainst ? 1 : 0),
            Draws + (goalsFor == goalsAgainst ? 1 : 0),
            Losses + (goalsFor < goalsAgainst ? 1 : 0),
            GoalsFor + goalsFor,
            GoalsAgainst + goalsAgainst);
    }
}
