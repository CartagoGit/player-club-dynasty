using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A row of buttons inside the options panel that lets the player pick a
/// locale. Each button pairs a procedurally drawn <see cref="FlagIcon"/>
/// with the locale's display name and a selection indicator.
/// </summary>
public sealed partial class LocaleSection : VBoxContainer
{
    private readonly LocaleCatalog _locales;
    private readonly ThemePaletteCatalog _palettes;
    private readonly Action<LocaleId> _onSelected;
    private readonly ButtonGroup _buttonGroup = new();
    private LocaleId _selected;

    public LocaleSection(
        LocaleCatalog locales,
        ThemePaletteCatalog palettes,
        LocaleId selected,
        Action<LocaleId> onSelected)
    {
        ArgumentNullException.ThrowIfNull(locales);
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(onSelected);
        _locales = locales;
        _palettes = palettes;
        _selected = locales.Resolve(selected).Id;
        _onSelected = onSelected;
        Name = "LocaleSection";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
    }

    public override void _Ready()
    {
        AddChild(Caption(Tr("OPTIONS_LANGUAGE_HEADER"), ManagerDesignTokens.Accent, 12));
        AddChild(Caption(
            Tr("OPTIONS_LANGUAGE_HINT"),
            ManagerDesignTokens.TextSecondary,
            12));

        var rows = new HBoxContainer { Name = "LocaleRows" };
        rows.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        foreach (LocaleEntry entry in _locales.Locales)
        {
            rows.AddChild(LocaleButton(entry));
        }

        AddChild(rows);
    }

    private Button LocaleButton(LocaleEntry entry)
    {
        var button = new Button
        {
            Name = $"Locale_{entry.Id.Value}",
            ToggleMode = true,
            ButtonGroup = _buttonGroup,
            TooltipText = Tr("LOCALE_COUNTRY_TOOLTIP", entry.CountryName),
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };

        var layout = new HBoxContainer { Name = "LocaleLayout" };
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        var flag = new FlagIcon { Name = "Flag" };
        flag.SetGeometry(entry.Flag);
        layout.AddChild(flag);

        var label = new Label { Name = "Name", Text = entry.NativeDisplayName };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        layout.AddChild(label);
        button.AddChild(layout);
        button.ButtonPressed = entry.Id == _selected;
        button.Pressed += () => Select(entry.Id);
        return button;
    }

    private void Select(LocaleId id)
    {
        _selected = _locales.Resolve(id).Id;
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
