using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A grid of cards inside the settings board that lets the player pick a theme
/// palette. Each card pairs a procedurally drawn <see cref="PaletteSwatch"/>
/// with the palette's display name.
/// </summary>
/// <remarks>
/// The section renders the captions it is handed and never calls <c>tr()</c>:
/// a shared control that translates on its own either duplicates keys or — as
/// happened here — renders the raw key when nothing has registered a catalogue.
/// </remarks>
public sealed partial class ThemeSection : VBoxContainer
{
    private readonly ThemePaletteCatalog _palettes;
    private readonly Action<ThemePaletteId> _onSelected;
    private readonly List<SelectableTile> _tiles = [];
    private readonly string _hint;
    private ThemePaletteId _selected;

    public ThemeSection(
        ThemePaletteCatalog palettes,
        ThemePaletteId selected,
        Action<ThemePaletteId> onSelected,
        string hint)
    {
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(onSelected);
        _palettes = palettes;
        _selected = palettes.Resolve(selected).Id;
        _onSelected = onSelected;
        _hint = hint ?? string.Empty;
        Name = "ThemeSection";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
    }

    public override void _Ready()
    {
        // The hosting plate carries the group's name; only the hint belongs here.
        if (!string.IsNullOrWhiteSpace(_hint))
        {
            AddChild(Caption(_hint, ManagerDesignTokens.TextSecondary, 12));
        }

        var grid = new HFlowContainer { Name = "ThemeGrid" };
        grid.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space8);
        grid.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space8);
        foreach (ThemePalette palette in _palettes.Palettes)
        {
            grid.AddChild(PaletteCard(palette));
        }

        AddChild(grid);
    }

    /// <remarks>
    /// See <see cref="SelectableTile"/>: a card is a swatch plus a caption, and
    /// a <see cref="Button"/> cannot hold either without losing its own click.
    /// </remarks>
    private SelectableTile PaletteCard(ThemePalette palette)
    {
        var tile = new SelectableTile(_tiles)
        {
            Name = $"Theme_{palette.Id.Value}",
            TooltipText = palette.DisplayName,
            Area = ShellArea.Settings,
            CustomMinimumSize = new Vector2(120, 0),
        };

        var swatch = new PaletteSwatch { Name = "Swatch" };
        swatch.SetPalette(palette);
        tile.AddContent(swatch);

        var label = new Label
        {
            Name = "Name",
            Text = palette.DisplayName,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        tile.AddContent(label);

        tile.Selected = palette.Id == _selected;
        tile.Pressed += () => Select(palette.Id);
        return tile;
    }

    private void Select(ThemePaletteId id)
    {
        _selected = _palettes.Resolve(id).Id;
        _onSelected(_selected);
    }

    private static Label Caption(string text, Color color, int fontSize)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        return label;
    }
}
