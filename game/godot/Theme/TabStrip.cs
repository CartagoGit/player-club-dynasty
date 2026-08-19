using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// A row of tabs along the top edge of a plate.
/// </summary>
/// <remarks>
/// The classic dossier used two strips at once — the facet of the content on
/// top, the working mode underneath — so the shape is part of the grammar, not
/// a modern import. A tab is the right control when the choices are *kinds of
/// the same thing* and only one can be open: it names every option, keeps the
/// panel below a fixed size, and costs one click to reach any of them.
///
/// The tabs are plain <see cref="Button"/>s with text and no children, which is
/// the only shape of button that reliably receives its own clicks; anything
/// richer belongs in a <see cref="SelectableTile"/>.
/// </remarks>
public sealed partial class TabStrip : PanelContainer, IPaletteAware
{
    private readonly HBoxContainer _tabs = new() { Name = "Tabs" };
    private readonly List<Button> _buttons = [];
    private int _selected = -1;

    /// <summary>Raised with the index of the tab the player opened.</summary>
    [Signal]
    public delegate void TabSelectedEventHandler(int index);

    public TabStrip()
    {
        Name = "TabStrip";
        _tabs.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        AddChild(_tabs);
        Repaint();
    }

    /// <summary>The area colour the strip and its active tab carry.</summary>
    public ShellArea Area { get; set; } = ShellArea.Settings;

    /// <summary>Index of the open tab, or -1 when the strip is empty.</summary>
    public int Selected => _selected;

    /// <summary>Appends a tab. The first one added opens automatically.</summary>
    /// <param name="name">
    /// Stable node name. The title is translated, so anything that addresses a
    /// tab — a test, a shortcut — uses this instead.
    /// </param>
    public Button AddTab(string title, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var button = new Button
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Tab{_buttons.Count + 1}" : name,
            Text = title.ToUpperInvariant(),
            TooltipText = title,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ClipText = true,
        };
        int index = _buttons.Count;
        button.Pressed += () => Select(index);
        _tabs.AddChild(button);
        _buttons.Add(button);

        if (_selected < 0)
        {
            Select(0);
        }
        else
        {
            Repaint();
        }
        return button;
    }

    /// <summary>Opens a tab by index. Out-of-range indices are ignored.</summary>
    public void Select(int index)
    {
        if (index < 0 || index >= _buttons.Count || index == _selected)
        {
            Repaint();
            return;
        }

        _selected = index;
        Repaint();
        EmitSignal(SignalName.TabSelected, index);
    }

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    private void Repaint()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.CommandBar());
        for (int index = 0; index < _buttons.Count; index++)
        {
            Button button = _buttons[index];
            bool active = index == _selected;
            ClassicChrome.State state = active ? ClassicChrome.State.Selected : ClassicChrome.State.Normal;
            button.AddThemeStyleboxOverride("normal", ClassicChrome.AreaEntry(Area, state));
            button.AddThemeStyleboxOverride(
                "hover",
                ClassicChrome.AreaEntry(Area, active ? ClassicChrome.State.Selected : ClassicChrome.State.Hovered));
            button.AddThemeStyleboxOverride("pressed", ClassicChrome.AreaEntry(Area, ClassicChrome.State.Pressed));
            button.AddThemeStyleboxOverride("focus", ClassicChrome.Focus());
            button.AddThemeColorOverride(
                "font_color",
                active ? ManagerDesignTokens.TextPrimary : ManagerDesignTokens.TextSecondary);
            button.AddThemeColorOverride("font_hover_color", ManagerDesignTokens.TextPrimary);
            button.AddThemeColorOverride("font_pressed_color", ManagerDesignTokens.TextPrimary);
            button.AddThemeFontSizeOverride("font_size", 13);
        }
    }
}
