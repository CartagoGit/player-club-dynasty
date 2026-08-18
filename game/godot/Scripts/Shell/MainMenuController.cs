using Godot;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;

namespace PCDynasty.Presentation.Shell;

public partial class MainMenuController : Node
{
    private IManagerWorld? _world;
    private ManagerShell? _shell;
    private Action<string>? _startNewCareer;
    private Action<string>? _continueCareer;
    private readonly ButtonGroup _clubButtons = new();

    private Control MenuFrame => GetNode<Control>("../MainMenu/MenuFrame");
    private Control CareerSetup => GetNode<Control>("../MainMenu/CareerSetup");
    private Control OptionsPanel => GetNode<Control>("../MainMenu/OptionsPanel");
    private VBoxContainer ClubList => GetNode<VBoxContainer>("../MainMenu/CareerSetup/SetupContent/ClubScroll/ClubList");
    private Label SelectedClub => GetNode<Label>("../MainMenu/CareerSetup/SetupContent/SelectedClub");
    private Button StartCareer => GetNode<Button>("../MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer");
    private Button ContinueCareer => GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/ContinueCareer");
    private Button LoadCareer => GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/LoadCareer");
    private Button DataEditor => GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/DataEditor");

    public void Configure(
        IManagerWorld world,
        ManagerShell shell,
        Action<string> startNewCareer,
        Action<string> continueCareer)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _shell = shell ?? throw new ArgumentNullException(nameof(shell));
        _startNewCareer = startNewCareer ?? throw new ArgumentNullException(nameof(startNewCareer));
        _continueCareer = continueCareer ?? throw new ArgumentNullException(nameof(continueCareer));
    }

    /// <summary>
    /// Points the menu at a different world without rebuilding the controller.
    /// Re-creating it would connect the scene's buttons a second time, which
    /// Godot rejects, so a world swap replaces dependencies in place.
    /// </summary>
    public void ReplaceWorld(
        IManagerWorld world,
        ManagerShell shell,
        Action<string> startNewCareer,
        Action<string> continueCareer)
    {
        Configure(world, shell, startNewCareer, continueCareer);
        RefreshCareerActions();
        if (CareerSetup.Visible)
        {
            PopulateClubs();
        }
    }

    public override void _Ready()
    {
        if (_world is null || _shell is null || _startNewCareer is null || _continueCareer is null)
        {
            GD.PushError("[MainMenu] Configure must run before the controller enters the tree.");
            return;
        }

        BindActions();
        ShowMainMenu();
    }

    private void BindActions()
    {
        GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/NewCareer").Pressed += BeginNewCareer;
        ContinueCareer.Pressed += ContinueSavedCareer;
        GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/Options").Pressed += OpenOptions;
        GetNode<Button>("../MainMenu/MenuFrame/MenuContent/Actions/Exit").Pressed += () => GetTree().Quit();
        GetNode<Button>("../MainMenu/CareerSetup/SetupContent/SetupActions/Back").Pressed += ShowMainMenu;
        GetNode<Button>("../MainMenu/OptionsPanel/OptionsContent/Back").Pressed += ShowMainMenu;
        StartCareer.Pressed += ConfirmCareer;

        var fullscreen = GetNode<CheckButton>("../MainMenu/OptionsPanel/OptionsContent/Fullscreen");
        fullscreen.Toggled += ToggleFullscreen;

        RefreshCareerActions();

        LoadCareer.Disabled = true;
        LoadCareer.TooltipText = "La gestión de múltiples carreras llegará con el selector de guardados.";
        DataEditor.Disabled = true;
        DataEditor.TooltipText = "El editor declarativo está disponible como herramienta separada durante esta fase.";
    }

    private void BeginNewCareer()
    {
        ShellTransitionResult result = _shell!.BeginNewCareer();
        if (!result.Accepted)
        {
            return;
        }

        MenuFrame.Visible = false;
        OptionsPanel.Visible = false;
        CareerSetup.Visible = true;
        StartCareer.Disabled = true;
        SelectedClub.Text = "Selecciona el club con el que quieres construir tu historia.";
        PopulateClubs();
    }

    private void PopulateClubs()
    {
        foreach (Node child in ClubList.GetChildren())
        {
            child.QueueFree();
        }

        foreach (ClubRow club in _world!.ListClubs())
        {
            var button = new Button
            {
                Text = $"  {club.DisplayName,-32}  {club.CountryShortName,-10}  PRESTIGIO {club.Prestige,3}",
                Alignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(0, 44),
                ToggleMode = true,
                ButtonGroup = _clubButtons,
                TooltipText = $"{club.City} · fundado {club.FoundedYear} · {club.SquadSize} jugadores",
            };
            button.Pressed += () => SelectClub(club);
            ClubList.AddChild(button);
        }
    }

    private void SelectClub(ClubRow club)
    {
        ShellTransitionResult result = _shell!.SelectClub(club.ExternalId);
        if (!result.Accepted)
        {
            return;
        }

        SelectedClub.Text = $"CLUB ELEGIDO · {club.DisplayName} · {club.City}, {club.CountryShortName}";
        StartCareer.Disabled = false;
    }

    private void ConfirmCareer()
    {
        ShellTransitionResult result = _shell!.ConfirmCareer();
        if (!result.Accepted || result.State.SelectedClubId is null)
        {
            return;
        }

        _startNewCareer!(result.State.SelectedClubId);
    }

    private void ContinueSavedCareer()
    {
        ShellTransitionResult result = _shell!.ContinueCareer();
        if (result.Accepted && result.State.SelectedClubId is not null)
        {
            _continueCareer!(result.State.SelectedClubId);
        }
    }

    private void OpenOptions()
    {
        if (!_shell!.OpenSettings().Accepted)
        {
            return;
        }

        MenuFrame.Visible = false;
        CareerSetup.Visible = false;
        OptionsPanel.Visible = true;
    }

    public void ShowMainMenu()
    {
        _shell!.ReturnToMainMenu();
        RefreshCareerActions();
        MenuFrame.Visible = true;
        CareerSetup.Visible = false;
        OptionsPanel.Visible = false;
    }

    private void RefreshCareerActions()
    {
        ContinueCareer.Disabled = !_shell!.State.CanContinue;
        ContinueCareer.TooltipText = ContinueCareer.Disabled
            ? "No existe todavía una carrera guardada."
            : "Continuar la última carrera.";
    }

    private static void ToggleFullscreen(bool enabled)
    {
        if (DisplayServer.GetName() == "headless")
        {
            return;
        }

        DisplayServer.WindowSetMode(enabled
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
    }
}
