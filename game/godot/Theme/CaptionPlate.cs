using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// A plate that announces itself: a coloured caption bar with the group's name
/// in caps, and a body underneath.
/// </summary>
/// <remarks>
/// This is the container the grammar asks for — no block of content floats on
/// the background, every one of them is inside a plate that says what it is and
/// which area it belongs to. The caption may also carry a trailing value (a
/// count, a total) on the right.
///
/// The children are built in the constructor, never in <c>_Ready</c>: callers
/// configure a plate before adding it to the tree, and anything created in
/// <c>_Ready</c> would silently drop the text it was handed.
/// </remarks>
public sealed partial class CaptionPlate : VBoxContainer, IPaletteAware
{
    private readonly PanelContainer _captionBar = new() { Name = "Caption" };
    private readonly Label _title = new() { Name = "Title" };
    private readonly Label _trailing = new() { Name = "Trailing" };
    private readonly PanelContainer _bodyPanel = new() { Name = "Body" };
    private readonly VBoxContainer _body = new() { Name = "BodyContent" };
    private ShellArea _area = ShellArea.None;

    public CaptionPlate(string title = "", ShellArea area = ShellArea.None)
    {
        _area = area;
        AddThemeConstantOverride("separation", 0);

        var captionRow = new HBoxContainer { Name = "CaptionRow" };
        captionRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _captionBar.AddChild(captionRow);

        _title.AddThemeFontSizeOverride("font_size", 13);
        _title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _title.VerticalAlignment = VerticalAlignment.Center;
        captionRow.AddChild(_title);

        _trailing.AddThemeFontSizeOverride("font_size", 12);
        _trailing.HorizontalAlignment = HorizontalAlignment.Right;
        _trailing.VerticalAlignment = VerticalAlignment.Center;
        _trailing.Visible = false;
        captionRow.AddChild(_trailing);

        AddChild(_captionBar);

        _body.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _bodyPanel.AddChild(_body);
        AddChild(_bodyPanel);

        SetTitle(title);
        Repaint();
    }

    /// <summary>The container callers fill with the plate's content.</summary>
    public VBoxContainer Body => _body;

    /// <summary>
    /// Makes the body take every pixel its parent offers. Off by default: a
    /// plate in a grid of groups must be as tall as its content, or one long
    /// group stretches every other one beside it.
    /// </summary>
    public bool FillHeight
    {
        get => _bodyPanel.SizeFlagsVertical.HasFlag(SizeFlags.Expand);
        set => _bodyPanel.SizeFlagsVertical = value ? SizeFlags.ExpandFill : SizeFlags.ShrinkBegin;
    }

    /// <summary>The area whose colour the caption carries.</summary>
    public ShellArea Area
    {
        get => _area;
        set
        {
            _area = value;
            Repaint();
        }
    }

    /// <summary>Sets the caption text. It is always rendered in caps.</summary>
    public void SetTitle(string title)
    {
        _title.Text = (title ?? string.Empty).ToUpperInvariant();
    }

    /// <summary>Sets the value shown at the right end of the caption bar. Empty hides it.</summary>
    public void SetTrailing(string? value)
    {
        _trailing.Text = (value ?? string.Empty).ToUpperInvariant();
        _trailing.Visible = !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>Adds a control to the body.</summary>
    public void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _body.AddChild(child);
    }

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    private void Repaint()
    {
        Color caption = ManagerDesignTokens.AreaColor(_area);
        Color onCaption = ManagerDesignTokens.OnArea(caption);
        _captionBar.AddThemeStyleboxOverride("panel", ClassicChrome.Caption(caption));
        _bodyPanel.AddThemeStyleboxOverride("panel", ClassicChrome.CaptionBody());
        _title.AddThemeColorOverride("font_color", onCaption);
        _trailing.AddThemeColorOverride("font_color", onCaption);
    }
}
