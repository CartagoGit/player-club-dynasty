using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Importers;
using PCDynasty.Persistence.Sqlite;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Shell;
using PCDynasty.Presentation.Workspace;
using PCDynasty.Presentation.Workspace.Persistence;

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
    private CareerSession? _careerSession;
    private CareerHistoryPersistence? _historyPersistence;

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
        _careerSession ??= new CareerSession();
        _historyPersistence ??= CreateHistoryPersistence(_careerSession);
        _historyPersistence.Load();
        _mainMenuController = new MainMenuController { Name = "MainMenuController" };
        _mainMenuController.Configure(service, new ManagerShell(), clubId => OpenWorkspace(service, clubId));
        AddChild(_mainMenuController);
    }

    private static CareerHistoryPersistence CreateHistoryPersistence(CareerSession session)
    {
        string savesRoot = Path.GetFullPath(Path.Combine(
            ProjectSettings.GlobalizePath("res://../"),
            "..",
            "saves"));
        var store = new SqliteCareerHistoryStore(Path.Combine(savesRoot, "career.sqlite"));
        return new CareerHistoryPersistence(store, session);
    }

    private void OpenWorkspace(IManagerWorld service, string clubId)
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
            CompetitionsButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Competitions"),
            HistoryButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/History"),
            WorldButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/World"),
            MainMenuButtonPath = new NodePath("../Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu"),
        };
        _workspaceController.Configure(
            service,
            clubId,
            _competitionSimulation,
            _careerSession!,
            _historyPersistence!);
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
