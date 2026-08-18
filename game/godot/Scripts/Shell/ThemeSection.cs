using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A grid of buttons inside the options panel that lets the player pick a
/// theme palette. Each button pairs a procedurally drawn
/// <see cref="PaletteSwatch"/> with the palette's display name and a
/// selection indicator.
/// </summary>
public sealed partial class ThemeSection : VBoxContainer
{
    private readonly ThemePaletteCatalog _palettes;
    private readonly Action<ThemePaletteId> _onSelected;
    private readonly ButtonGroup _buttonGroup = new();
    private ThemePaletteId _selected;

    public ThemeSection(
        ThemePaletteCatalog palettes,
        ThemePaletteId selected,
        Action<ThemePaletteId> onSelected)
    {
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(onSelected);
        _palettes = palettes;
        _selected = palettes.Resolve(selected).Id;
        _onSelected = onSelected;
        Name = "ThemeSection";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
    }

    public override void _Ready()
    {
        AddChild(Caption(Tr("OPTIONS_THEME_HEADER"), ManagerDesignTokens.Accent, 12));
        AddChild(Caption(
            Tr("OPTIONS_THEME_HINT"),
            ManagerDesignTokens.TextSecondary,
            12));

        var grid = new GridContainer { Name = "ThemeGrid", Columns = 3 };
        grid.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space12);
        grid.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space12);
        foreach (ThemePalette palette in _palettes.Palettes)
        {
            grid.AddChild(PaletteCard(palette));
        }

        AddChild(grid);
    }

    private Button PaletteCard(ThemePalette palette)
    {
        var button = new Button
        {
            Name = $"Theme_{palette.Id.Value}",
            ToggleMode = true,
            ButtonGroup = _buttonGroup,
            TooltipText = palette.DisplayName,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget * 2),
        };

        var layout = new VBoxContainer { Name = "ThemeCardLayout" };
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var swatch = new PaletteSwatch { Name = "Swatch" };
        swatch.SetPalette(palette);
        layout.AddChild(swatch);

        var label = new Label { Name = "Name", Text = palette.DisplayName, HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        layout.AddChild(label);
        button.AddChild(layout);
        button.ButtonPressed = palette.Id == _selected;
        button.Pressed += () => Select(palette.Id);
        return button;
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
