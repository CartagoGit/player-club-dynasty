using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// A clickable plate that can hold real content: an icon, a swatch, several
/// labels, anything a container can lay out.
/// </summary>
/// <remarks>
/// It exists because <see cref="Button"/> is not a container. Adding a
/// <see cref="Control"/> child to a button breaks it twice over: the child
/// inherits <c>MouseFilter.Stop</c> and swallows every click, so
/// <c>Pressed</c> never fires, and the button does not lay the child out, so
/// it draws at its natural size outside the frame. That is exactly how the
/// language and theme pickers ended up unclickable and overflowing.
///
/// This tile owns its input instead: it is a <see cref="PanelContainer"/> with
/// a real container inside, it forces every descendant to
/// <c>MouseFilter.Ignore</c> as it is added, and it handles mouse and keyboard
/// itself. Group behaviour is explicit rather than inherited from
/// <see cref="ButtonGroup"/>, because the tile is not a button.
/// </remarks>
public sealed partial class SelectableTile : PanelContainer, IPaletteAware
{
    private readonly MarginContainer _padding = new() { Name = "TilePadding" };
    private readonly VBoxContainer _content = new() { Name = "TileContent" };
    private readonly List<SelectableTile> _group;
    private bool _selected;
    private bool _hovered;
    private bool _pressed;
    private bool _disabled;

    /// <summary>
    /// Raised when the player activates the tile with mouse or keyboard.
    /// </summary>
    /// <remarks>
    /// A Godot signal rather than a plain C# event, so the tile behaves like
    /// every other control in the scene: connectable from GDScript, visible in
    /// the editor, and reachable from the smoke test.
    /// </remarks>
    [Signal]
    public delegate void PressedEventHandler();

    /// <summary>
    /// Creates a tile. Pass a shared list to make a set of tiles mutually
    /// exclusive: selecting one clears the rest.
    /// </summary>
    public SelectableTile(List<SelectableTile>? group = null)
    {
        _group = group ?? [];
        _group.Add(this);

        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;
        MouseDefaultCursorShape = CursorShape.PointingHand;

        _padding.AddThemeConstantOverride("margin_left", ManagerDesignTokens.Space12);
        _padding.AddThemeConstantOverride("margin_right", ManagerDesignTokens.Space12);
        _padding.AddThemeConstantOverride("margin_top", ManagerDesignTokens.Space8);
        _padding.AddThemeConstantOverride("margin_bottom", ManagerDesignTokens.Space8);
        base.AddChild(_padding);

        _content.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        _padding.AddChild(_content);

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        FocusEntered += Repaint;
        FocusExited += Repaint;

        Repaint();
    }

    /// <summary>The container callers fill. Everything added here is laid out properly.</summary>
    public Container Content => _content;

    /// <summary>The area whose colour this tile carries.</summary>
    public ShellArea Area { get; set; } = ShellArea.None;

    /// <summary>Whether this tile is the chosen one in its group.</summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value)
            {
                return;
            }
            _selected = value;
            Repaint();
        }
    }

    /// <summary>A disabled tile paints muted and ignores input.</summary>
    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled == value)
            {
                return;
            }
            _disabled = value;
            MouseFilter = value ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
            FocusMode = value ? FocusModeEnum.None : FocusModeEnum.All;
            Repaint();
        }
    }

    /// <summary>Adds content and makes sure it can never steal the tile's click.</summary>
    public void AddContent(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _content.AddChild(child);
        Silence(child);
    }

    /// <summary>Selects this tile and clears every other tile in its group.</summary>
    public void Select()
    {
        foreach (SelectableTile tile in _group)
        {
            tile.Selected = ReferenceEquals(tile, this);
        }
    }

    /// <summary>
    /// Activates the tile as if the player had clicked it. Exposed so a test or
    /// a keyboard shortcut can drive the control through the same path the
    /// mouse takes.
    /// </summary>
    public void Press() => Activate();

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    public override void _GuiInput(InputEvent @event)
    {
        if (_disabled)
        {
            return;
        }

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouse)
        {
            _pressed = mouse.Pressed;
            Repaint();
            if (!mouse.Pressed)
            {
                Activate();
            }
            AcceptEvent();
            return;
        }

        if (@event.IsActionPressed("ui_accept"))
        {
            Activate();
            AcceptEvent();
        }
    }

    /// <summary>
    /// Every descendant is silenced as it enters the tree, not only the ones
    /// added through <see cref="AddContent"/>: a caller that builds a nested
    /// container would otherwise re-introduce the very bug this class exists
    /// to prevent.
    /// </summary>
    private static void Silence(Node node)
    {
        if (node is Control control)
        {
            control.MouseFilter = MouseFilterEnum.Ignore;
        }
        foreach (Node child in node.GetChildren())
        {
            Silence(child);
        }
    }

    private void Activate()
    {
        _pressed = false;
        Select();
        EmitSignal(SignalName.Pressed);
        Repaint();
    }

    private void OnMouseEntered()
    {
        _hovered = true;
        Repaint();
    }

    private void OnMouseExited()
    {
        _hovered = false;
        _pressed = false;
        Repaint();
    }

    private void Repaint()
    {
        // Late-added children still get silenced: the walk is cheap and it is
        // the only place guaranteed to run after the caller finished building.
        Silence(_content);

        ClassicChrome.State state = _disabled
            ? ClassicChrome.State.Disabled
            : _pressed
                ? ClassicChrome.State.Pressed
                : _selected
                    ? ClassicChrome.State.Selected
                    : _hovered || HasFocus()
                        ? ClassicChrome.State.Hovered
                        : ClassicChrome.State.Normal;

        AddThemeStyleboxOverride("panel", ClassicChrome.AreaEntry(Area, state));
    }
}
