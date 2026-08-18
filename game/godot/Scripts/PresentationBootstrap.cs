using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Importers;
using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using PCDynasty.Presentation.Careers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Shell;
using PCDynasty.Presentation.Workspace;

namespace PCDynasty.Presentation;

public partial class PresentationBootstrap : Control
{
    private Label WorldStatus => GetNode<Label>("MainMenu/MenuFrame/MenuContent/WorldStatus");
    private Label WorldMetric => GetNode<Label>("MainMenu/Hero/HeroContent/Metric1/Text");
    private Control MainMenu => GetNode<Control>("MainMenu");
    private Control Workspace => GetNode<Control>("Workspace");

    private MainMenuController? _mainMenuController;
    private ManagerWorkspaceController? _workspaceController;
    private readonly ICompetitionSimulationService _competitionSimulation =
        new CompetitionSimulationService(TimeProvider.System);
    private SqliteCareerProfileStore? _careerProfileStore;
    private CareerRuntimeFactory? _careerRuntimeFactory;

    public override void _Ready()
    {
        _ = InitializeAndLoadAsync();
    }

    private async Task InitializeAndLoadAsync()
    {
        try
        {
            // Content root: parent of godot/ is game/, content/ is at game/content/.
            // Resolved through Godot's ProjectSettings so a packaged build keeps working.
            string gameRoot = ProjectSettings.GlobalizePath("res://../");
            string contentRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                gameRoot,
                "content",
                "starter-world"));

            var reader = new JsonContentReader();
            var service = new WorldService(reader, contentRoot);
            await service.ReloadAsync();

            int clubCount = service.ListClubs().Count;
            WorldStatus.Text = $"MUNDO LISTO · {clubCount} CLUBES · SIMULACIÓN DETERMINISTA";
            WorldMetric.Text = $"{clubCount} CLUBES · MUNDO FICCIONAL · IDENTIDAD PROPIA";
            AttachMainMenu(service);
        }
        catch (System.Exception exception)
        {
            WorldStatus.Text = $"NO SE PUDO CARGAR EL MUNDO · {exception.Message}";
            GD.PushError($"[WorldService] {exception}");
        }
    }

    private void AttachMainMenu(IManagerWorld service)
    {
        string databasePath = CareerDatabasePath();
        _careerProfileStore = new SqliteCareerProfileStore(databasePath);
        _careerRuntimeFactory = new CareerRuntimeFactory(databasePath, TimeProvider.System);
        string? savedClubId = _careerProfileStore.TryLoad(out CareerProfile? profile)
            ? profile!.ClubExternalId
            : null;
        _mainMenuController = new MainMenuController { Name = "MainMenuController" };
        _mainMenuController.Configure(
            service,
            new ManagerShell(savedClubId),
            clubId => OpenCareer(service, clubId, CareerRuntimeMode.New),
            clubId => OpenCareer(service, clubId, CareerRuntimeMode.Continue));
        AddChild(_mainMenuController);
    }

    private void OpenCareer(IManagerWorld service, string clubId, CareerRuntimeMode mode)
    {
        ClubRow club = service.FindClub(clubId)
            ?? throw new InvalidOperationException($"Club '{clubId}' does not exist in the loaded world.");
        CareerRuntime runtime = _careerRuntimeFactory!.Create(mode, club, service);
        SaveCareerProfile(service, clubId, mode);
        OpenWorkspace(service, clubId, runtime);
    }

    private void SaveCareerProfile(IManagerWorld service, string clubId, CareerRuntimeMode mode)
    {
        DateTimeOffset now = TimeProvider.System.GetUtcNow();
        DateTimeOffset startedAt = now;
        DateTimeOffset lastPlayedAt = now;
        if (mode == CareerRuntimeMode.Continue &&
            _careerProfileStore!.TryLoad(out CareerProfile? existing))
        {
            startedAt = existing!.StartedAtUtc;
            if (lastPlayedAt < startedAt)
            {
                lastPlayedAt = startedAt;
            }
        }

        string clubName = service.FindClub(clubId)?.DisplayName ?? clubId;
        _careerProfileStore!.Save(new CareerProfile(clubId, clubName, startedAt, lastPlayedAt));
    }

    private static string CareerDatabasePath()
    {
        string? externalPath = System.Environment.GetEnvironmentVariable("PCDYNASTY_CAREER_DB");
        if (!string.IsNullOrWhiteSpace(externalPath))
        {
            return Path.GetFullPath(externalPath);
        }

        string savesRoot = Path.GetFullPath(Path.Combine(
            ProjectSettings.GlobalizePath("res://../"),
            "..",
            "saves"));
        return Path.Combine(savesRoot, "career.sqlite");
    }

    private void OpenWorkspace(IManagerWorld service, string clubId, CareerRuntime runtime)
    {
        MainMenu.Visible = false;
        Workspace.Visible = true;

        _workspaceController?.QueueFree();
        _workspaceController = new ManagerWorkspaceController
        {
            Name = "WorkspaceController",
            ContentRootPath = new NodePath("../Workspace/Layout/ContentMargin/ContentScroll/Content"),
            StatusLabelPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Status"),
            OverviewButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Overview"),
            SquadButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Squad"),
            TacticsButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Tactics"),
            CompetitionsButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Competitions"),
            MarketButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Market"),
            FinancesButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Finances"),
            HistoryButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/History"),
            WorldButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/World"),
            MainMenuButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu"),
        };
        _workspaceController.Configure(
            service,
            clubId,
            _competitionSimulation,
            runtime.Lineups,
            runtime.Session,
            runtime.History,
            runtime.Finances,
            runtime.TransferMarket,
            runtime.Squad,
            runtime.Calendar);
        _workspaceController.MainMenuRequested += CloseWorkspace;
        AddChild(_workspaceController);
    }

    private void CloseWorkspace()
    {
        _workspaceController?.QueueFree();
        _workspaceController = null;
        Workspace.Visible = false;
        MainMenu.Visible = true;
        _mainMenuController?.ShowMainMenu();
    }
}
