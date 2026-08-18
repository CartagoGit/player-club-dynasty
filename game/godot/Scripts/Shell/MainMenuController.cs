using Godot;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Core.Localization;

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
    /// Attaches the preferences UI (language + theme) to the options panel.
    /// The first call builds the sections; subsequent calls are no-ops so a
    /// world swap does not duplicate the controls.
    /// </summary>
    public void AttachPreferences(
        PCDynasty.Presentation.Core.PresentationPreferencesStore store,
        PCDynasty.Presentation.Core.ThemePaletteCatalog palettes,
        Godot.SceneTree tree,
        LocaleSession localeSession,
        LocaleActivationService activationService)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(localeSession);
        ArgumentNullException.ThrowIfNull(activationService);

        if (_preferencesStore is not null)
        {
            return;
        }

        _preferencesStore = store;
        _palettes = palettes;
        _tree = tree;
        _localeSession = localeSession;
        _activationService = activationService;

        VBoxContainer options = GetNode<VBoxContainer>("../MainMenu/OptionsPanel/OptionsContent");
        var preferences = _preferencesStore.Load();
        _optionsMenu = new OptionsMenu();
        _optionsMenu.Closed += ShowMainMenu;

        var languageSection = new LocaleSection(_localeSession, _activationService)
        {
            Name = "LanguageSection",
        };
        languageSection.LocaleSelected += OnLocaleSelected;
        var themeSection = new ThemeSection(
            _palettes,
            preferences.ThemeId,
            OnThemeSelected)
        {
            Name = "ThemeSection",
        };

        // Settings is a menu of areas, not one stacked panel: the player picks
        // language, look, display or data first and only then sees its controls.
        options.AddChild(_optionsMenu);
        _optionsMenu.AddSection(_localeSession.ActiveFile.Options.LanguageHeader, languageSection, "SectionLanguage");
        _optionsMenu.AddSection(_localeSession.ActiveFile.Options.ThemeHeader, themeSection, "SectionTheme");
        _optionsMenu.AddSection(_localeSession.ActiveFile.Options.DisplayHeader, BuildDisplaySection(preferences), "SectionDisplay");

        ThemePaletteApplier.Apply(_tree, _palettes.Resolve(preferences.ThemeId));
    }

    private void OnLocaleSelected(LocaleSelectionResult result)
    {
        if (_preferencesStore is null || _localeSession is null)
        {
            return;
        }

        if (!result.Succeeded || result.Failure is not null)
        {
            ShowPickerRejection(result.Failure);
            return;
        }

        PCDynasty.Presentation.Core.PresentationPreferences preferences = _preferencesStore
            .Load()
            .WithLocale(new PCDynasty.Presentation.Core.LocaleId(result.Selected));
        _preferencesStore.Save(preferences);
        RefreshSceneTranslations();
    }

    private void ShowPickerRejection(
        PCDynasty.Presentation.Core.Diagnostics.ApplicationError? error)
    {
        if (error is null)
        {
            return;
        }

        var panel = new ApplicationErrorPanel(error);
        GetTree().Root.AddChild(panel);
        panel.PrimaryPressed += () => panel.QueueFree();
    }

    private void OnThemeSelected(PCDynasty.Presentation.Core.ThemePaletteId id)
    {
        if (_preferencesStore is null || _palettes is null || _tree is null)
        {
            return;
        }

        PCDynasty.Presentation.Core.PresentationPreferences preferences = _preferencesStore
            .Load()
            .WithTheme(id);
        _preferencesStore.Save(preferences);
        ThemePaletteApplier.Apply(_tree, _palettes.Resolve(id));
    }

    /// <summary>
    /// The display area: one row per preference, each mirroring what will be
    /// restored on the next launch.
    /// </summary>
    private VBoxContainer BuildDisplaySection(PresentationPreferences preferences)
    {
        var section = new VBoxContainer { Name = "DisplaySection" };
        section.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        section.AddChild(Toggle("Fullscreen", _localeSession!.ActiveFile.Options.Fullscreen, preferences.Fullscreen, OnFullscreenToggled));
        section.AddChild(Toggle("ReducedMotion", _localeSession.ActiveFile.Options.ReducedMotion, preferences.ReducedMotion, OnReducedMotionToggled));
        section.AddChild(Toggle("DenseTables", _localeSession.ActiveFile.Options.DenseTables, preferences.DenseTables, OnDenseTablesToggled));
        return section;
    }

    private static CheckButton Toggle(
        string name,
        string text,
        bool pressed,
        CheckButton.ToggledEventHandler onToggled)
    {
        var toggle = new CheckButton
        {
            Name = name,
            Text = text,
            ButtonPressed = pressed,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        toggle.Toggled += onToggled;
        return toggle;
    }

    private void OnFullscreenToggled(bool pressed)
    {
        if (_preferencesStore is null)
        {
            return;
        }
        _preferencesStore.Save(_preferencesStore.Load().WithFullscreen(pressed));
    }

    private void OnReducedMotionToggled(bool pressed)
    {
        if (_preferencesStore is null)
        {
            return;
        }
        _preferencesStore.Save(_preferencesStore.Load().WithReducedMotion(pressed));
    }

    private void OnDenseTablesToggled(bool pressed)
    {
        if (_preferencesStore is null)
        {
            return;
        }
        _preferencesStore.Save(_preferencesStore.Load().WithDenseTables(pressed));
    }

    /// <summary>
    /// Marks every <see cref="CanvasItem"/> in the tree for redraw so the
    /// new locale's <c>tr()</c> lookups replace the old strings without
    /// rebuilding the scene.
    /// </summary>
    private void RefreshSceneTranslations()
    {
        if (_tree?.Root is null)
        {
            return;
        }

        RedrawForLocale(_tree.Root);
    }

    private static void RedrawForLocale(Node node)
    {
        if (node is CanvasItem canvasItem)
        {
            canvasItem.QueueRedraw();
        }
        foreach (Node child in node.GetChildren())
        {
            RedrawForLocale(child);
        }
    }

    private PCDynasty.Presentation.Core.PresentationPreferencesStore? _preferencesStore;
    private PCDynasty.Presentation.Core.ThemePaletteCatalog? _palettes;
    private LocaleSession? _localeSession;
    private OptionsMenu? _optionsMenu;

    /// <summary>The settings menu, so the composition root can add its own area.</summary>
    public OptionsMenu? Settings => _optionsMenu;
    private LocaleActivationService? _activationService;
    private Godot.SceneTree? _tree;

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
        StartCareer.Pressed += ConfirmCareer;

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
