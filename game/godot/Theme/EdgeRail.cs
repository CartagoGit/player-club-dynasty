using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The narrow strip along a screen edge that names the area, written
/// vertically.
/// </summary>
/// <remarks>
/// It is signage, not navigation: it anchors the player without spending any
/// of the canvas. The label is rotated a quarter turn and the strip carries the
/// area colour, so the rail and the plates it accompanies read as one block.
/// </remarks>
public sealed partial class EdgeRail : PanelContainer, IPaletteAware
{
    private readonly Label _label = new() { Name = "RailLabel" };
    private ShellArea _area;

    public EdgeRail(string text = "", ShellArea area = ShellArea.None)
    {
        Name = "EdgeRail";
        _area = area;
        CustomMinimumSize = new Vector2(ManagerDesignTokens.EdgeRailWidth, 0);

        // A rotated Control is not measured by its parent container, so the
        // label lives inside a plain Control that the rail sizes itself.
        var pivot = new Control { Name = "RailPivot", SizeFlagsVertical = SizeFlags.ExpandFill };
        AddChild(pivot);

        _label.RotationDegrees = -90f;
        _label.AddThemeFontSizeOverride("font_size", 12);
        _label.HorizontalAlignment = HorizontalAlignment.Center;
        _label.VerticalAlignment = VerticalAlignment.Center;
        pivot.AddChild(_label);
        pivot.Resized += LayOut;

        SetText(text);
        Repaint();
    }

    /// <summary>The area whose colour the rail carries.</summary>
    public ShellArea Area
    {
        get => _area;
        set
        {
            _area = value;
            Repaint();
        }
    }

    /// <summary>Sets the rail caption. Always rendered in caps.</summary>
    public void SetText(string text)
    {
        _label.Text = (text ?? string.Empty).ToUpperInvariant();
        LayOut();
    }

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    public override void _Ready() => LayOut();

    private void LayOut()
    {
        if (_label.GetParent() is not Control pivot)
        {
            return;
        }

        // Rotated by -90°, the label's own width runs up the rail: give it the
        // rail's height and place its origin at the bottom edge.
        Vector2 area = pivot.Size;
        _label.Size = new Vector2(area.Y, ManagerDesignTokens.EdgeRailWidth);
        _label.Position = new Vector2(0, area.Y);
    }

    private void Repaint()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.EdgeRail(_area));
        _label.AddThemeColorOverride(
            "font_color",
            ManagerDesignTokens.OnArea(ManagerDesignTokens.AreaColor(_area)));
    }
}
