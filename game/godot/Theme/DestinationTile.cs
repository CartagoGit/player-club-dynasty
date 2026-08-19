using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// One destination on the main board: a plate with a coloured caption bar,
/// a name and a line saying what it is.
/// </summary>
/// <remarks>
/// The reference's main menu was a grid of nine illustrated destinations, each
/// with its label set inside the plate, and — this is the part worth copying —
/// **the destinations it could not offer stayed on the board, greyed, in their
/// place**. The menu was the map of the product, and a map with gaps is more
/// honest than a map that lies about its size.
///
/// So a tile has three states rather than two: available, unavailable-because-
/// the-world-is-not-loaded (temporary), and not-built-yet (permanent, until we
/// build it). The third carries the reason in its own hint line instead of
/// disappearing.
///
/// Like <see cref="SelectableTile"/> it is a <see cref="PanelContainer"/> and
/// not a <see cref="Button"/>: a button is not a container, and giving one
/// children makes them swallow the click and draw outside the frame.
/// </remarks>
public sealed partial class DestinationTile : PanelContainer, IPaletteAware
{
    private readonly MarginContainer _padding = new() { Name = "TilePadding" };
    private readonly VBoxContainer _content = new() { Name = "TileContent" };
    private readonly PanelContainer _captionBar = new() { Name = "Caption" };
    private readonly Label _index = new() { Name = "Index" };
    private readonly Label _title = new() { Name = "Title" };
    private readonly Label _hint = new() { Name = "Hint" };
    private readonly ShellArea _area;
    private bool _hovered;
    private bool _pressed;

    /// <summary>Raised when the player activates the tile with mouse or keyboard.</summary>
    [Signal]
    public delegate void PressedEventHandler();

    public DestinationTile(string name, ShellArea area, int index)
    {
        Name = name;
        _area = area;

        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        CustomMinimumSize = new Vector2(0, 74);

        var stack = new VBoxContainer { Name = "Stack" };
        stack.AddThemeConstantOverride("separation", 0);
        AddChild(stack);

        // The index digit comes from the Argentine variant, where the menu was
        // a numbered column and the digit's colour told you which family the
        // destination belonged to. It survives a grid just as well.
        _index.AddThemeFontSizeOverride("font_size", 12);
        _index.VerticalAlignment = VerticalAlignment.Center;
        _index.Text = index > 0 ? index.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        _captionBar.CustomMinimumSize = new Vector2(0, ManagerDesignTokens.CaptionHeight);
        var captionRow = new HBoxContainer { Name = "CaptionRow" };
        captionRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        captionRow.AddChild(_index);
        _captionBar.AddChild(captionRow);
        stack.AddChild(_captionBar);

        _padding.AddThemeConstantOverride("margin_left", ManagerDesignTokens.Space12);
        _padding.AddThemeConstantOverride("margin_right", ManagerDesignTokens.Space12);
        _padding.AddThemeConstantOverride("margin_top", ManagerDesignTokens.Space8);
        _padding.AddThemeConstantOverride("margin_bottom", ManagerDesignTokens.Space8);
        _content.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        _padding.AddChild(_content);
        stack.AddChild(_padding);

        _title.AddThemeFontSizeOverride("font_size", 15);
        _title.ClipText = true;
        _content.AddChild(_title);

        _hint.AddThemeFontSizeOverride("font_size", 11);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _content.AddChild(_hint);

        Silence(stack);
        Repaint();
    }

    /// <summary>False when the destination is not built yet; the tile stays on the board.</summary>
    public bool Available { get; private set; } = true;

    /// <summary>Sets the two lines. `available: false` greys the tile and refuses the press.</summary>
    public void Show(string title, string hint, bool available = true)
    {
        _title.Text = title.ToUpperInvariant();
        _hint.Text = hint;
        _hint.Visible = !string.IsNullOrWhiteSpace(hint);
        Available = available;
        FocusMode = available ? FocusModeEnum.All : FocusModeEnum.None;
        MouseDefaultCursorShape = available ? CursorShape.PointingHand : CursorShape.Arrow;
        TooltipText = hint;
        Repaint();
    }

    /// <summary>Activates the tile programmatically. No-op when unavailable.</summary>
    public void Press()
    {
        if (Available)
        {
            EmitSignal(SignalName.Pressed);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!Available)
        {
            return;
        }

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } click)
        {
            _pressed = click.Pressed;
            Repaint();
            if (!click.Pressed)
            {
                AcceptEvent();
                GrabFocus();
                EmitSignal(SignalName.Pressed);
            }

            return;
        }

        if (@event is InputEventKey { Pressed: true, Keycode: Key.Enter or Key.KpEnter or Key.Space })
        {
            AcceptEvent();
            EmitSignal(SignalName.Pressed);
        }
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationMouseEnter:
                _hovered = true;
                Repaint();
                break;
            case NotificationMouseExit:
                _hovered = false;
                _pressed = false;
                Repaint();
                break;
            case NotificationFocusEnter:
            case NotificationFocusExit:
                Repaint();
                break;
        }
    }

    public void OnPaletteChanged() => Repaint();

    /// <summary>
    /// Forces every descendant to ignore the pointer. Without it the labels
    /// eat the click and the tile never fires — the defect that made the
    /// language and theme pickers unusable.
    /// </summary>
    private static void Silence(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is Control control)
            {
                control.MouseFilter = MouseFilterEnum.Ignore;
            }

            Silence(child);
        }
    }

    private void Repaint()
    {
        ClassicChrome.State state = !Available
            ? ClassicChrome.State.Disabled
            : _pressed
                ? ClassicChrome.State.Pressed
                : _hovered || HasFocus()
                    ? ClassicChrome.State.Hovered
                    : ClassicChrome.State.Normal;

        AddThemeStyleboxOverride("panel", ClassicChrome.AreaEntry(_area, state));

        Color areaColor = ManagerDesignTokens.AreaColor(_area);
        _captionBar.AddThemeStyleboxOverride(
            "panel",
            ClassicChrome.Caption(Available ? areaColor : ManagerDesignTokens.Mix(areaColor, ManagerDesignTokens.Surface, 0.62f)));
        _index.AddThemeColorOverride("font_color", ManagerDesignTokens.OnArea(areaColor));
        _title.AddThemeColorOverride(
            "font_color",
            Available ? ManagerDesignTokens.TextPrimary : ManagerDesignTokens.TextSecondary);
        _hint.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
    }
}
