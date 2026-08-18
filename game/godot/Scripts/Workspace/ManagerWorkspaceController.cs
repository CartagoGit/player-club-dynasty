using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;
using PCDynasty.Presentation.Workspace.Persistence;
using PCDynasty.Presentation.Workspace.Screens;

namespace PCDynasty.Presentation.Workspace;

/// <summary>
/// Coordinates workspace navigation and selected-club state. Rendering,
/// simulation and persistence are delegated to focused collaborators.
/// </summary>
public partial class ManagerWorkspaceController : Control
{
    [Export] public NodePath ContentRootPath { get; set; } = new();
    [Export] public NodePath StatusLabelPath { get; set; } = new();
    [Export] public NodePath OverviewButtonPath { get; set; } = new();
    [Export] public NodePath SquadButtonPath { get; set; } = new();
    [Export] public NodePath CompetitionsButtonPath { get; set; } = new();
    [Export] public NodePath HistoryButtonPath { get; set; } = new();
    [Export] public NodePath WorldButtonPath { get; set; } = new();
    [Export] public NodePath MainMenuButtonPath { get; set; } = new();

    public event Action? MainMenuRequested;

    private IManagerWorld? _world;
    private WorkspaceSection _selectedSection = WorkspaceSection.Overview;
    private string? _selectedClubId;
    private ClubOverviewScreen? _overviewScreen;
    private SquadScreen? _squadScreen;
    private CompetitionsScreen? _competitionsScreen;
    private HistoryScreen? _historyScreen;
    private WorldScreen? _worldScreen;
    private ClubPickerScreen? _clubPickerScreen;

    internal void Configure(
        IManagerWorld world,
        string selectedClubId,
        ICompetitionSimulationService competitionSimulation,
        CareerSession careerSession,
        CareerHistoryPersistence historyPersistence)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _selectedClubId = string.IsNullOrWhiteSpace(selectedClubId)
            ? throw new ArgumentException("Selected club id cannot be empty.", nameof(selectedClubId))
            : selectedClubId;
        ArgumentNullException.ThrowIfNull(historyPersistence);
        ArgumentNullException.ThrowIfNull(competitionSimulation);
        ArgumentNullException.ThrowIfNull(careerSession);

        _overviewScreen = new ClubOverviewScreen(world, careerSession, () => SelectSection(WorkspaceSection.ClubPicker));
        _squadScreen = new SquadScreen(world);
        _competitionsScreen = new CompetitionsScreen(
            world,
            competitionSimulation,
            historyPersistence,
            RenderCurrent,
            SetStatus);
        _historyScreen = new HistoryScreen(world, careerSession, RenderCurrent);
        _worldScreen = new WorldScreen(world);
        _clubPickerScreen = new ClubPickerScreen(SelectClub);
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
        Wire(CompetitionsButtonPath, WorkspaceSection.Competitions);
        Wire(HistoryButtonPath, WorkspaceSection.History);
        Wire(WorldButtonPath, WorkspaceSection.World);

        Button? mainMenu = GetNodeOrNull<Button>(MainMenuButtonPath);
        if (mainMenu is not null) mainMenu.Pressed += () => MainMenuRequested?.Invoke();
        RenderCurrent();
    }

    private void Wire(NodePath path, WorkspaceSection section)
    {
        Button? button = GetNodeOrNull<Button>(path);
        if (button is not null) button.Pressed += () => SelectSection(section);
    }

    private void SelectSection(WorkspaceSection section)
    {
        _selectedSection = section;
        RenderCurrent();
    }

    private void SelectClub(string clubExternalId)
    {
        _selectedClubId = clubExternalId;
        SelectSection(WorkspaceSection.Overview);
    }

    private void RenderCurrent()
    {
        Control? root = GetNodeOrNull<Control>(ContentRootPath);
        if (root is null || _world is null)
        {
            return;
        }

        WorkspaceUiFactory.Clear(root);
        IReadOnlyList<ClubRow> clubs = _world.ListClubs();
        _selectedClubId ??= clubs.Count > 0 ? clubs[0].ExternalId : null;
        if (_selectedClubId is null)
        {
            root.AddChild(WorkspaceUiFactory.CreateText("No hay clubs cargados.", ManagerDesignTokens.TextSecondary));
            return;
        }

        ClubRow? club = clubs.FirstOrDefault(candidate => candidate.ExternalId == _selectedClubId);
        if (club is null)
        {
            SetStatus("● Club no encontrado");
            return;
        }

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
            case WorkspaceSection.History:
                _historyScreen!.Render(root);
                SetStatus("● Historial de partidos");
                break;
            case WorkspaceSection.World:
                _worldScreen!.Render(root);
                SetStatus("● Mundial de competiciones");
                break;
            case WorkspaceSection.ClubPicker:
                _clubPickerScreen!.Render(root, clubs);
                SetStatus("● Selección de club");
                break;
            default:
                _overviewScreen!.Render(root, club);
                SetStatus($"● {club.DisplayName}");
                break;
        }
    }

    private void SetStatus(string text)
    {
        Label? status = GetNodeOrNull<Label>(StatusLabelPath);
        if (status is not null) status.Text = text;
    }
}
