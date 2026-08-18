using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Importers;
using PCDynasty.Content.Worlds;
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
    private Control MenuFrame => GetNode<Control>("MainMenu/MenuFrame");
    private Control OptionsPanel => GetNode<Control>("MainMenu/OptionsPanel");
    private Control Workspace => GetNode<Control>("Workspace");

    private MainMenuController? _mainMenuController;
    private ManagerWorkspaceController? _workspaceController;
    private readonly ICompetitionSimulationService _competitionSimulation =
        new CompetitionSimulationService(TimeProvider.System);
    private SqliteCareerProfileStore? _careerProfileStore;
    private CareerRuntimeFactory? _careerRuntimeFactory;
    private ContentSourcePreference? _contentSourcePreference;
    private ContentSourceSection? _contentSourceSection;
    private string _integratedContentRoot = string.Empty;

    public override void _Ready()
    {
        _ = InitializeAndLoadAsync();
    }

    private async Task InitializeAndLoadAsync()
    {
        // Content root: parent of godot/ is game/, content/ is at game/content/.
        // Resolved through Godot's ProjectSettings so a packaged build keeps working.
        string gameRoot = ProjectSettings.GlobalizePath("res://../");
        _integratedContentRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            gameRoot,
            "content",
            "starter-world"));
        _contentSourcePreference = new ContentSourcePreference(ContentSourcePreferencePath());

        // The remembered choice may be stale; the integrated world is always a
        // valid fallback, so a bad external path never blocks the game.
        ContentSourceDescriptor source = _contentSourcePreference.Resolve(_integratedContentRoot);
        if (await LoadWorldAsync(source).ConfigureAwait(true) is not null && !source.IsIntegrated)
        {
            _contentSourcePreference.ClearExternalPath();
            await LoadWorldAsync(ContentSourceDescriptor.Integrated(_integratedContentRoot))
                .ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Loads a described world and rebuilds the menu around it. Returns an
    /// error message, or null when the world became the active one.
    /// </summary>
    private async Task<string?> LoadWorldAsync(ContentSourceDescriptor source)
    {
        try
        {
            var service = new WorldService(ContentReaderFactory.Create(source.FormatId), source);
            await service.ReloadAsync();

            int clubCount = service.ListClubs().Count;
            WorldStatus.Text = $"MUNDO LISTO · {clubCount} CLUBES · SIMULACIÓN DETERMINISTA";
            WorldMetric.Text = $"{clubCount} CLUBES · MUNDO FICCIONAL · IDENTIDAD PROPIA";
            AttachMainMenu(service);
            _contentSourceSection?.Refresh(service.Source, service.ContentHash);
            return null;
        }
        catch (System.Exception exception)
        {
            WorldStatus.Text = $"NO SE PUDO CARGAR EL MUNDO · {exception.Message}";
            GD.PushError($"[WorldService] {exception}");
            return exception.Message;
        }
    }

    private void SelectExternalWorld(string path)
    {
        ContentSourceDescriptor source;
        try
        {
            source = ContentSourceDescriptor.External(path);
        }
        catch (ArgumentException exception)
        {
            _contentSourceSection?.ReportError(exception.Message);
            return;
        }

        _ = SwapWorldAsync(source, remember: true);
    }

    private void RestoreIntegratedWorld() =>
        _ = SwapWorldAsync(
            ContentSourceDescriptor.Integrated(_integratedContentRoot),
            remember: false);

    /// <summary>
    /// Replaces the active world without blocking the frame and keeps the
    /// player where they were. Waiting on the load here would deadlock: the
    /// continuation is posted back to this very thread.
    /// </summary>
    private async Task SwapWorldAsync(ContentSourceDescriptor source, bool remember)
    {
        bool optionsWereOpen = OptionsPanel.Visible;
        string? error = await LoadWorldAsync(source).ConfigureAwait(true);
        if (error is not null)
        {
            _contentSourceSection?.ReportError(error);
            return;
        }

        if (remember)
        {
            _contentSourcePreference!.SaveExternalPath(source.Path);
        }
        else
        {
            _contentSourcePreference!.ClearExternalPath();
        }

        if (optionsWereOpen)
        {
            MenuFrame.Visible = false;
            OptionsPanel.Visible = true;
        }
    }

    private static string ContentSourcePreferencePath() =>
        System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(CareerDatabasePath()) ?? ".",
            "content-source.json");

    private void AttachMainMenu(WorldService service)
    {
        string databasePath = CareerDatabasePath();
        _careerProfileStore = new SqliteCareerProfileStore(databasePath);
        _careerRuntimeFactory = new CareerRuntimeFactory(databasePath, TimeProvider.System);
        string? savedClubId = _careerProfileStore.TryLoad(out CareerProfile? profile)
            ? profile!.ClubExternalId
            : null;
        // A saved club belongs to the world it was chosen in; a different world
        // may not contain it.
        if (savedClubId is not null && service.FindClub(savedClubId) is null)
        {
            savedClubId = null;
        }

        var shell = new ManagerShell(savedClubId);
        if (_mainMenuController is null)
        {
            _mainMenuController = new MainMenuController { Name = "MainMenuController" };
            _mainMenuController.Configure(
                service,
                shell,
                clubId => OpenCareer(service, clubId, CareerRuntimeMode.New),
                clubId => OpenCareer(service, clubId, CareerRuntimeMode.Continue));
            AddChild(_mainMenuController);
        }
        else
        {
            _mainMenuController.ReplaceWorld(
                service,
                shell,
                clubId => OpenCareer(service, clubId, CareerRuntimeMode.New),
                clubId => OpenCareer(service, clubId, CareerRuntimeMode.Continue));
        }

        AttachContentSourceSection(service);
    }

    /// <summary>
    /// The source picker lives in the options panel and survives a world swap,
    /// so it is built once and refreshed afterwards.
    /// </summary>
    private void AttachContentSourceSection(WorldService world)
    {
        var options = GetNodeOrNull<Control>("MainMenu/OptionsPanel/OptionsContent");
        if (options is null)
        {
            return;
        }

        if (_contentSourceSection is not null)
        {
            _contentSourceSection.Refresh(world.Source, world.ContentHash);
            return;
        }

        var section = new ContentSourceSection();
        section.Configure(
            world.Source,
            world.ContentHash,
            SelectExternalWorld,
            RestoreIntegratedWorld);
        _contentSourceSection = section;
        options.AddChild(section);
        // Above the spacer and the Back button the scene already declares.
        options.MoveChild(section, Math.Max(0, options.GetChildCount() - 3));
    }

    private void OpenCareer(WorldService service, string clubId, CareerRuntimeMode mode)
    {
        ClubRow club = service.FindClub(clubId)
            ?? throw new InvalidOperationException($"Club '{clubId}' does not exist in the loaded world.");
        CareerRuntime runtime = _careerRuntimeFactory!.Create(mode, club, service);
        SaveCareerProfile(service, clubId, mode);
        OpenWorkspace(service, clubId, runtime);
    }

    private void SaveCareerProfile(WorldService service, string clubId, CareerRuntimeMode mode)
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

    private void OpenWorkspace(WorldService service, string clubId, CareerRuntime runtime)
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
