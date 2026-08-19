using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Dresses the scene's static nodes in the shell's chrome.
/// </summary>
/// <remarks>
/// The scene file declares <em>structure</em> — which bands exist, what is
/// anchored where — and nothing else. Colour and relief live here, for two
/// reasons: a StyleBox baked into a <c>.tscn</c> cannot follow a palette
/// switch, and a scene that carries its own colours is a second, silent source
/// of design decisions that drifts away from <see cref="ClassicChrome"/>.
///
/// The dresser is a node so it can be re-run: it implements
/// <see cref="IPaletteAware"/>, so the applier calls it on every palette change
/// and the static chrome repaints along with everything built in code.
/// </remarks>
public sealed partial class ShellChrome : Node, IPaletteAware
{
    private readonly Control _root;

    public ShellChrome(Control root)
    {
        Name = "ShellChrome";
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public override void _Ready() => Dress();

    /// <inheritdoc />
    public void OnPaletteChanged() => Dress();

    private void Dress()
    {
        DressBackground();
        DressMainMenu();
        DressWorkspace();
    }

    private void DressBackground()
    {
        if (_root.GetNodeOrNull<ColorRect>("Background") is ColorRect background)
        {
            background.Color = ManagerDesignTokens.Canvas;
        }
    }

    private void DressMainMenu()
    {
        Panel("MainMenu/TopBar", ClassicChrome.ContextBand());
        Text("MainMenu/TopBar/TopContent/Brand", ManagerDesignTokens.Accent);
        Text("MainMenu/TopBar/ScreenTitle", ManagerDesignTokens.TextPrimary);
        Text("MainMenu/TopBar/TopContent/Build", ManagerDesignTokens.Focus);

        Panel("MainMenu/MenuFrame", ClassicChrome.TitlePlate());
        Text("MainMenu/MenuFrame/MenuContent/Eyebrow", ManagerDesignTokens.Accent);
        Text("MainMenu/MenuFrame/MenuContent/Title", ManagerDesignTokens.TextPrimary);
        Text("MainMenu/MenuFrame/MenuContent/Description", ManagerDesignTokens.TextSecondary);

        // The action list is the main menu's command bar: the destinations sit
        // in the order a career is lived, the exit is the only destructive one
        // and starting a career is the only advance.
        Command("MainMenu/MenuFrame/MenuContent/Actions/NewCareer", ClassicChrome.Intent.Advance);
        Command("MainMenu/MenuFrame/MenuContent/Actions/ContinueCareer", ClassicChrome.Intent.Neutral);
        Command("MainMenu/MenuFrame/MenuContent/Actions/LoadCareer", ClassicChrome.Intent.Neutral);
        Command("MainMenu/MenuFrame/MenuContent/Actions/DataEditor", ClassicChrome.Intent.Neutral);
        Command("MainMenu/MenuFrame/MenuContent/Actions/Options", ClassicChrome.Intent.Neutral);
        Command("MainMenu/MenuFrame/MenuContent/Actions/Exit", ClassicChrome.Intent.Destructive);

        Panel("MainMenu/Hero", ClassicChrome.Plate(ManagerDesignTokens.Space16));
        Text("MainMenu/Hero/HeroContent/HeroEyebrow", ManagerDesignTokens.Focus);
        Text("MainMenu/Hero/HeroContent/HeroTitle", ManagerDesignTokens.TextPrimary);
        Text("MainMenu/Hero/HeroContent/HeroText", ManagerDesignTokens.TextSecondary);
        Text("MainMenu/Hero/HeroContent/Legal", ManagerDesignTokens.TextSecondary);
        foreach (string metric in new[] { "Metric1", "Metric2", "Metric3" })
        {
            Panel($"MainMenu/Hero/HeroContent/{metric}", ClassicChrome.RaisedPlate());
            Text($"MainMenu/Hero/HeroContent/{metric}/Text", ManagerDesignTokens.TextPrimary);
        }

        Panel("MainMenu/CareerSetup", ClassicChrome.Plate(ManagerDesignTokens.Space16));
        Text("MainMenu/CareerSetup/SetupContent/Eyebrow", ManagerDesignTokens.Accent);
        Text("MainMenu/CareerSetup/SetupContent/Title", ManagerDesignTokens.TextPrimary);
        Text("MainMenu/CareerSetup/SetupContent/Hint", ManagerDesignTokens.TextSecondary);
        Text("MainMenu/CareerSetup/SetupContent/SelectedClub", ManagerDesignTokens.Focus);
        Command("MainMenu/CareerSetup/SetupContent/SetupActions/Back", ClassicChrome.Intent.Neutral);
        Command("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer", ClassicChrome.Intent.Advance);

        Panel("MainMenu/StatusStrip", ClassicChrome.StatusStrip());
        Text("MainMenu/StatusStrip/WorldStatus", ManagerDesignTokens.Positive);
    }

    private void DressWorkspace()
    {
        Panel("Workspace/Frame/Layout/Sidebar", RailBackground());
        Text("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Brand", ManagerDesignTokens.Accent);
        Text("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Desk", ManagerDesignTokens.TextSecondary);
        Panel("Workspace/Frame/StatusBar", ClassicChrome.StatusStrip());
        Text("Workspace/Frame/StatusBar/StatusText", ManagerDesignTokens.TextSecondary);
        Text("Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/Loading", ManagerDesignTokens.TextSecondary);
    }

    private static StyleBoxFlat RailBackground()
    {
        StyleBoxFlat box = ClassicChrome.Plate(ManagerDesignTokens.Space4);
        box.BgColor = ManagerDesignTokens.Navigation;
        box.BorderColor = ManagerDesignTokens.Accent;
        box.BorderWidthLeft = 0;
        box.BorderWidthTop = 0;
        box.BorderWidthRight = 2;
        box.BorderWidthBottom = 0;
        return box;
    }

    private void Panel(string path, StyleBox style)
    {
        if (_root.GetNodeOrNull<Control>(path) is Control control)
        {
            control.AddThemeStyleboxOverride("panel", style);
        }
    }

    private void Text(string path, Color color)
    {
        if (_root.GetNodeOrNull<Label>(path) is Label label)
        {
            label.AddThemeColorOverride("font_color", color);
        }
    }

    private void Command(string path, ClassicChrome.Intent intent)
    {
        if (_root.GetNodeOrNull<Button>(path) is not Button button)
        {
            return;
        }

        ClassicChrome.StyleCommand(button, intent);
        button.Alignment = HorizontalAlignment.Left;
        button.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space8);
    }
}
