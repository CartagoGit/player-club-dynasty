using Godot;
using PCDynasty.Application.Shell;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Shell;

public partial class MainMenuController : Node
{
    private IManagerWorld? _world;
    private ManagerShell? _shell;
    private Action<string>? _startNewCareer;
    private Action<string>? _continueCareer;
    private const int ColumnClub = 240;
    private const int ColumnCountry = 110;
    private const int ColumnCity = 150;
    private const int ColumnFounded = 70;
    private const int ColumnSquad = 70;
    private const int ColumnPrestige = 90;
    private const int ColumnAction = 90;

    private Control MenuFrame => GetNode<Control>("../MainMenu/MenuFrame");
    private Control CareerSetup => GetNode<Control>("../MainMenu/CareerSetup");
    private Control OptionsPanel => GetNode<Control>("../MainMenu/OptionsPanel");
    private Control Hero => GetNode<Control>("../MainMenu/Hero");
    private Label ScreenTitle => GetNode<Label>("../MainMenu/TopBar/ScreenTitle");
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

        Control options = GetNode<Control>("../MainMenu/OptionsPanel");
        var preferences = _preferencesStore.Load();
        _optionsMenu = new OptionsBoard();
        _optionsMenu.Closed += ShowMainMenu;

        var languageSection = new LocaleSection(_localeSession, _activationService)
        {
            Name = "LanguageSection",
        };
        languageSection.LocaleSelected += OnLocaleSelected;
        var themeSection = new ThemeSection(
            _palettes,
            preferences.ThemeId,
            OnThemeSelected,
            _localeSession.ActiveFile.Options.ThemeHint)
        {
            Name = "ThemeSection",
        };

        // Settings is one configuration screen: every group is a captioned
        // plate and all of them are visible at once, the way the classic
        // configuration screen laid out match, resolution, sound and timing.
        _optionsMenu.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        options.AddChild(_optionsMenu);
        OptionsSection copy = _localeSession.ActiveFile.Options;
        var sections = new SettingsSections(_preferencesStore, copy);
        _optionsMenu.AddSection(copy.LanguageHeader, languageSection, "SectionLanguage");
        _optionsMenu.AddSection(copy.ThemeHeader, themeSection, "SectionTheme");
        _optionsMenu.AddSection(copy.DisplayHeader, sections.BuildGraphics(preferences), "SectionDisplay");
        _optionsMenu.AddSection(copy.Audio.Header, sections.BuildAudio(preferences), "SectionAudio");
        _optionsMenu.AddSection(copy.Video.Header, sections.BuildVideo(preferences), "SectionVideo");

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
    private OptionsBoard? _optionsMenu;

    /// <summary>The settings board, so the composition root can add its own group.</summary>
    public OptionsBoard? Settings => _optionsMenu;
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
        Hero.Visible = false;
        OptionsPanel.Visible = false;
        CareerSetup.Visible = true;
        ScreenTitle.Text = "NUEVA CARRERA";
        StartCareer.Disabled = true;
        SelectedClub.Text = "Selecciona el club con el que quieres construir tu historia.";
        PopulateClubs();
    }

    /// <summary>
    /// The club list is a dense table, not a stack of buttons with padded
    /// captions: proportional type never lines up, so the columns used to
    /// wander from row to row.
    /// </summary>
    private void PopulateClubs()
    {
        foreach (Node child in ClubList.GetChildren())
        {
            ClubList.RemoveChild(child);
            child.QueueFree();
        }

        var table = new ListSection { Name = "ClubTable", Area = ShellArea.World };
        table.AddColumnHeader("CLUB", ColumnClub);
        table.AddColumnHeader("PAÍS", ColumnCountry);
        table.AddColumnHeader("CIUDAD", ColumnCity);
        table.AddColumnHeader("FUND.", ColumnFounded, HorizontalAlignment.Right);
        table.AddColumnHeader("PLANT.", ColumnSquad, HorizontalAlignment.Right);
        table.AddColumnHeader("PRESTIGIO", ColumnPrestige, HorizontalAlignment.Right);
        table.AddColumnHeader("", ColumnAction, HorizontalAlignment.Right);

        IReadOnlyList<ClubRow> clubs = _world!.ListClubs();
        for (int index = 0; index < clubs.Count; index++)
        {
            ClubRow club = clubs[index];
            var row = new ZebraRow(index)
            {
                TooltipText = $"{club.City} · fundado {club.FoundedYear} · {club.SquadSize} jugadores",
            };
            row.AddCell(club.DisplayName, ColumnClub, ManagerDesignTokens.TextPrimary);
            row.AddCell(club.CountryShortName, ColumnCountry, ManagerDesignTokens.TextSecondary);
            row.AddCell(club.City, ColumnCity, ManagerDesignTokens.TextSecondary);
            row.AddCell(
                club.FoundedYear.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ColumnFounded,
                ManagerDesignTokens.TextSecondary,
                HorizontalAlignment.Right);
            row.AddCell(
                club.SquadSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ColumnSquad,
                ManagerDesignTokens.TextSecondary,
                HorizontalAlignment.Right);
            row.AddCell(
                club.Prestige.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ColumnPrestige,
                ManagerDesignTokens.Accent,
                HorizontalAlignment.Right);
            row.AddAction("ELEGIR", club.DisplayName, _ => SelectClub(club), enabled: true, name: "Choose");
            table.AddRow(row);
        }

        table.RowActivated += index =>
        {
            if (index >= 0 && index < clubs.Count)
            {
                table.SelectedIndex = index;
                SelectClub(clubs[index]);
            }
        };
        ClubList.AddChild(table);
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
        Hero.Visible = false;
        CareerSetup.Visible = false;
        OptionsPanel.Visible = true;
        ScreenTitle.Text = _localeSession is null
            ? "OPCIONES"
            : _localeSession.ActiveFile.Options.Header.Title.ToUpperInvariant();
    }

    public void ShowMainMenu()
    {
        _shell!.ReturnToMainMenu();
        RefreshCareerActions();
        MenuFrame.Visible = true;
        Hero.Visible = true;
        CareerSetup.Visible = false;
        OptionsPanel.Visible = false;
        ScreenTitle.Text = "MENÚ PRINCIPAL";
    }

    private void RefreshCareerActions()
    {
        ContinueCareer.Disabled = !_shell!.State.CanContinue;
        ContinueCareer.TooltipText = ContinueCareer.Disabled
            ? "No existe todavía una carrera guardada."
            : "Continuar la última carrera.";
    }

}
