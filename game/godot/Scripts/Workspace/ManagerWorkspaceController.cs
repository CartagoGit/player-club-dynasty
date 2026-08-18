using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Finances;
using PCDynasty.Application.Matches;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;
using PCDynasty.Presentation.Workspace.Persistence;
using PCDynasty.Presentation.Workspace.Screens;

namespace PCDynasty.Presentation.Workspace;

/// <summary>Coordinates navigation and club state through focused screens.</summary>
public partial class ManagerWorkspaceController : Control
{
    [Export] public NodePath ContentRootPath { get; set; } = new();
    [Export] public NodePath StatusLabelPath { get; set; } = new();
    [Export] public NodePath HeaderPath { get; set; } = new();
    [Export] public NodePath OverviewButtonPath { get; set; } = new();
    [Export] public NodePath SquadButtonPath { get; set; } = new();
    [Export] public NodePath TacticsButtonPath { get; set; } = new();
    [Export] public NodePath CompetitionsButtonPath { get; set; } = new();
    [Export] public NodePath MarketButtonPath { get; set; } = new();
    [Export] public NodePath FinancesButtonPath { get; set; } = new();
    [Export] public NodePath WorldButtonPath { get; set; } = new();
    [Export] public NodePath MainMenuButtonPath { get; set; } = new();

    public event Action? MainMenuRequested;

    private IManagerWorld? _world;
    private WorkspaceSection _selectedSection = WorkspaceSection.Overview;
    private ManagedClubContext? _managed;
    private BrowsedClubContext? _browsed;
    private ClubOverviewScreen? _overviewScreen;
    private SquadScreen? _squadScreen;
    private TacticsScreen? _tacticsScreen;
    private CompetitionsScreen? _competitionsScreen;
    private MarketScreen? _marketScreen;
    private FinancesScreen? _financesScreen;
    private HistoryScreen? _historyScreen;
    private WorldScreen? _worldScreen;
    private ClubPickerScreen? _clubPickerScreen;
    private readonly List<(Button Button, Action Handler)> _buttonBindings = [];
    private readonly Dictionary<WorkspaceSection, Button> _railEntries = [];
    private ICareerFinanceService? _finances;
    private CareerCalendar? _calendar;
    internal void Configure(
        IManagerWorld world,
        string selectedClubId,
        ICompetitionSimulationService competitionSimulation,
        ICareerLineupService lineupService,
        CareerSession careerSession,
        CareerHistoryPersistence historyPersistence,
        ICareerFinanceService financeService,
        ICareerTransferMarketService transferMarket,
        ICareerSquadProjection squadProjection,
        CareerCalendar calendar,
        LocaleFile locale)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        ArgumentNullException.ThrowIfNull(locale);
        _world = world ?? throw new ArgumentNullException(nameof(world));
        // The career manages exactly one club; browsing starts there and may
        // move anywhere without ever moving command authority with it.
        _managed = new ManagedClubContext(selectedClubId);
        _browsed = new BrowsedClubContext(selectedClubId);
        ArgumentNullException.ThrowIfNull(historyPersistence);
        ArgumentNullException.ThrowIfNull(competitionSimulation);
        ArgumentNullException.ThrowIfNull(lineupService);
        ArgumentNullException.ThrowIfNull(careerSession);
        ArgumentNullException.ThrowIfNull(financeService);
        ArgumentNullException.ThrowIfNull(transferMarket);
        ArgumentNullException.ThrowIfNull(squadProjection);

        _overviewScreen = new ClubOverviewScreen(world, careerSession, () => SelectSection(WorkspaceSection.ClubPicker));
        _squadScreen = new SquadScreen(
            world,
            squadProjection,
            _managed,
            calendar,
            () => SelectSection(WorkspaceSection.Market),
            locale.Squad);
        _tacticsScreen = new TacticsScreen(world, lineupService, _managed);
        _competitionsScreen = new CompetitionsScreen(
            world,
            competitionSimulation,
            historyPersistence,
            _managed,
            RenderCurrent,
            SetStatus,
            () => SelectSection(WorkspaceSection.History),
            locale.Competitions);
        _marketScreen = new MarketScreen(
            transferMarket,
            _managed,
            calendar,
            RenderCurrent,
            locale.Market);
        _financesScreen = new FinancesScreen(financeService, _managed);
        _historyScreen = new HistoryScreen(world, careerSession, RenderCurrent, locale.History);
        _finances = financeService;
        _calendar = calendar;
        _worldScreen = new WorldScreen(world, locale.World);
        _clubPickerScreen = new ClubPickerScreen(SelectClub, locale.Picker);
    }

    public override void _Ready()
    {
        if (_world is null || _overviewScreen is null)
        {
            GD.PushError("[ManagerWorkspaceController] Configure must run before the node enters the tree.");
            return;
        }

        Wire(OverviewButtonPath, WorkspaceSection.Overview);
        Wire(SquadButtonPath, WorkspaceSection.Squad);
        Wire(TacticsButtonPath, WorkspaceSection.Tactics);
        Wire(CompetitionsButtonPath, WorkspaceSection.Competitions);
        Wire(MarketButtonPath, WorkspaceSection.Market);
        Wire(FinancesButtonPath, WorkspaceSection.Finances);
        Wire(WorldButtonPath, WorkspaceSection.World);

        Button? mainMenu = GetNodeOrNull<Button>(MainMenuButtonPath);
        if (mainMenu is not null) Bind(mainMenu, () => MainMenuRequested?.Invoke());
        RenderCurrent();
    }

    public override void _ExitTree()
    {
        foreach ((Button button, Action handler) in _buttonBindings)
        {
            button.Pressed -= handler;
        }

        _buttonBindings.Clear();
    }

    private void Wire(NodePath path, WorkspaceSection section)
    {
        Button? button = GetNodeOrNull<Button>(path);
        if (button is null)
        {
            return;
        }

        _railEntries[section] = button;
        Bind(button, () => SelectSection(section));
    }

    /// <summary>The active area keeps its gold edge so the rail answers "where am I".</summary>
    private void HighlightActiveArea()
    {
        foreach ((WorkspaceSection section, Button entry) in _railEntries)
        {
            bool active = section == _selectedSection
                || (_selectedSection == WorkspaceSection.History && section == WorkspaceSection.Competitions)
                || (_selectedSection == WorkspaceSection.ClubPicker && section == WorkspaceSection.Overview);
            entry.AddThemeStyleboxOverride("normal", ClassicChrome.RailEntry(active));
            entry.AddThemeColorOverride(
                "font_color",
                active ? ManagerDesignTokens.TextPrimary : ManagerDesignTokens.TextSecondary);
        }
    }

    private void Bind(Button button, Action handler)
    {
        button.Pressed += handler;
        _buttonBindings.Add((button, handler));
    }

    private void SelectSection(WorkspaceSection section)
    {
        _selectedSection = section;
        RenderCurrent();
    }

    private void SelectClub(string clubExternalId)
    {
        _browsed = new BrowsedClubContext(clubExternalId);
        SelectSection(WorkspaceSection.Overview);
    }

    private void RenderCurrent()
    {
        Control? root = GetNodeOrNull<Control>(ContentRootPath);
        if (root is null || _world is null)
        {
            return;
        }

        HighlightActiveArea();
        WorkspaceUiFactory.Clear(root);
        IReadOnlyList<ClubRow> clubs = _world.ListClubs();
        if (_browsed is null || _managed is null || clubs.Count == 0)
        {
            root.AddChild(WorkspaceUiFactory.CreateText("No hay clubs cargados.", ManagerDesignTokens.TextSecondary));
            return;
        }

        ClubRow? club = clubs.FirstOrDefault(candidate => candidate.ExternalId == _browsed.ClubExternalId);
        if (club is null)
        {
            SetStatus("● Club no encontrado");
            return;
        }

        RenderHeader(clubs);
        switch (_selectedSection)
        {
            case WorkspaceSection.Squad:
                _squadScreen!.Render(root, club);
                SetStatus($"● {club.DisplayName} · plantilla");
                break;
            case WorkspaceSection.Competitions:
                _competitionsScreen!.Render(root, club);
                SetStatus($"● {club.DisplayName} · competiciones");
                break;
            case WorkspaceSection.Tactics:
                _tacticsScreen!.Render(root, club);
                SetStatus($"● {club.DisplayName} · plan de partido");
                break;
            case WorkspaceSection.Finances:
                ClubRow managedClub = clubs.First(candidate => _managed.Owns(candidate.ExternalId));
                _financesScreen!.Render(root, managedClub);
                SetStatus($"● {managedClub.DisplayName} · finanzas");
                break;
            case WorkspaceSection.Market:
                ClubRow marketClub = clubs.First(candidate => _managed.Owns(candidate.ExternalId));
                _marketScreen!.Render(root, marketClub);
                SetStatus($"● {marketClub.DisplayName} · mercado de fichajes");
                break;
            case WorkspaceSection.History:
                _historyScreen!.Render(root);
                SetStatus("● Historial de partidos");
                break;
            case WorkspaceSection.World:
                _worldScreen!.Render(root);
                SetStatus("● Mundial de competiciones");
                break;
            case WorkspaceSection.ClubPicker:
                _clubPickerScreen!.Render(root, clubs, _managed.ClubExternalId);
                SetStatus("● Selección de club");
                break;
            default:
                _overviewScreen!.Render(root, club);
                SetStatus($"● {club.DisplayName}");
                break;
        }
    }

    /// <summary>
    /// The header always describes the managed club, never the browsed one:
    /// it answers "what do I run", not "what am I looking at".
    /// </summary>
    private void RenderHeader(IReadOnlyList<ClubRow> clubs)
    {
        WorkspaceHeader? header = GetNodeOrNull<WorkspaceHeader>(HeaderPath);
        if (header is null || _managed is null || _calendar is null || _finances is null)
        {
            return;
        }

        ClubRow? managedClub = clubs.FirstOrDefault(club => _managed.Owns(club.ExternalId));
        if (managedClub is null)
        {
            return;
        }

        string money;
        try
        {
            ClubFinanceOverview overview = _finances.GetOverview(_managed);
            money = WorkspaceUiFactory.FormatMoney(overview.Cash, overview.Currency);
        }
        catch (InvalidOperationException)
        {
            // The career has no finance state yet; the header still names the club.
            money = "—";
        }

        header.Show(managedClub, _calendar, money);
    }

    private void SetStatus(string text)
    {
        Label? status = GetNodeOrNull<Label>(StatusLabelPath);
        if (status is not null) status.Text = text;
    }
}
