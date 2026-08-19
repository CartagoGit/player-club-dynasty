using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The fixed strip of commands along the bottom of a screen.
/// </summary>
/// <remarks>
/// The grammar makes one promise about this bar and it has to hold everywhere:
/// the place of a command never changes. Exits and secondary actions sit on the
/// left, the command that moves the career forward sits at the far right, and
/// it is the only control in the shell that spends time. A player can press
/// anything else without fear.
/// </remarks>
public sealed partial class CommandBar : PanelContainer, IPaletteAware
{
    private readonly HBoxContainer _row = new() { Name = "CommandRow" };
    private readonly HBoxContainer _left = new() { Name = "LeadingCommands" };
    private readonly HBoxContainer _right = new() { Name = "TrailingCommands" };
    private readonly List<(Button Button, ClassicChrome.Intent Intent)> _commands = [];

    public CommandBar()
    {
        Name = "CommandBar";
        CustomMinimumSize = new Vector2(0, ManagerDesignTokens.CommandBarHeight);

        _row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        AddChild(_row);

        _left.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _row.AddChild(_left);

        var spacer = new Control { Name = "CommandSpacer", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _row.AddChild(spacer);

        _right.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _row.AddChild(_right);

        Repaint();
    }

    /// <summary>Adds a command on the left: exits, saves, secondary actions.</summary>
    public Button AddLeading(
        string name,
        string text,
        Action onPressed,
        ClassicChrome.Intent intent = ClassicChrome.Intent.Neutral) =>
        Add(_left, name, text, onPressed, intent);

    /// <summary>Adds a command on the right. The last one added is the advance.</summary>
    public Button AddTrailing(
        string name,
        string text,
        Action onPressed,
        ClassicChrome.Intent intent = ClassicChrome.Intent.Advance) =>
        Add(_right, name, text, onPressed, intent);

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    private Button Add(
        Container host,
        string name,
        string text,
        Action onPressed,
        ClassicChrome.Intent intent)
    {
        ArgumentNullException.ThrowIfNull(onPressed);
        Button button = ClassicChrome.CommandButton(name, text, intent);
        button.CustomMinimumSize = new Vector2(150, ManagerDesignTokens.MinimumPointerTarget);
        button.Pressed += onPressed;
        host.AddChild(button);
        _commands.Add((button, intent));
        return button;
    }

    private void Repaint()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.CommandBar());
        foreach ((Button button, ClassicChrome.Intent intent) in _commands)
        {
            ClassicChrome.StyleCommand(button, intent);
        }
    }
}
