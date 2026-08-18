using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Pushes the active <see cref="ThemePalette"/> through a Godot scene tree.
/// The applier exists because the shell builds StyleBoxFlat overrides at
/// construction time but the palette can change at runtime via the options
/// panel. Walking the tree once replaces every colour the palette owns
/// without rebuilding any node.
/// </summary>
/// <remarks>
/// The applier is intentionally idempotent: calling it twice with the same
/// palette is a no-op. It only touches StyleBoxFlat and the colour overrides
/// the shell explicitly accepted; navigation chrome, the accent border and
/// the focus rule are the only things the palette governs. Layout numbers
/// stay untouched because the palette describes colour, not spacing.
/// </remarks>
public static class ThemePaletteApplier
{
    private static readonly string[] StyleKeys =
    [
        "panel",
        "normal",
        "hover",
        "pressed",
        "focus",
        "selected",
        "bg",
        "fill",
        "separator",
    ];

    private static readonly string[] ColourKeys =
    [
        "font_color",
        "font_hover_color",
        "font_pressed_color",
        "font_disabled_color",
        "font_uneditable_color",
        "font_placeholder_color",
        "font_outline_color",
        "color",
    ];

    /// <summary>
    /// Replaces the active palette and pushes it through the entire scene
    /// tree rooted at <paramref name="tree"/>'s root. If the tree has no
    /// root yet, the applier still updates <see cref="ManagerDesignTokens"/>
    /// so the next chrome that builds a StyleBoxFlat picks up the new
    /// palette.
    /// </summary>
    public static void Apply(SceneTree tree, ThemePalette palette)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(palette);
        ManagerDesignTokens.SetPalette(palette);
        Node? root = tree.Root;
        if (root is null)
        {
            return;
        }

        ApplyToNode(root, palette);
    }

    private static void ApplyToNode(Node node, ThemePalette palette)
    {
        if (node is Control control)
        {
            ApplyToControl(control, palette);
        }

        foreach (Node child in node.GetChildren())
        {
            ApplyToNode(child, palette);
        }
    }

    private static void ApplyToControl(Control control, ThemePalette palette)
    {
        foreach (string key in StyleKeys)
        {
            StyleBox? style = control.GetThemeStylebox(key);
            if (style is StyleBoxFlat flat)
            {
                RepaintStyleBox(flat, palette);
            }
        }

        foreach (string key in ColourKeys)
        {
            Color current = control.GetThemeColor(key);
            if (current == new Color(0f, 0f, 0f, 0f))
            {
                continue;
            }
            if (ReplacementFor(key, palette) is Color replacement)
            {
                control.AddThemeColorOverride(key, replacement);
            }
        }
    }

    private static void RepaintStyleBox(StyleBoxFlat flat, ThemePalette palette)
    {
        if (ClosestToken(flat.BgColor) is ThemeToken bg)
        {
            flat.BgColor = TokenColour(bg, palette);
        }
        if (ClosestToken(flat.BorderColor) is ThemeToken border)
        {
            flat.BorderColor = TokenColour(border, palette);
        }
    }

    private static Color? ReplacementFor(string key, ThemePalette palette)
    {
        if (key == "font_pressed_color" || key == "font_hover_color")
        {
            return ManagerDesignTokens.ToGodot(palette.TextPrimary);
        }

        return null;
    }

    private enum ThemeToken
    {
        Canvas = 0,
        Navigation = 1,
        Surface = 2,
        SurfaceRaised = 3,
        Border = 4,
        Accent = 5,
        Positive = 6,
        Warning = 7,
        Negative = 8,
        TextPrimary = 9,
        TextSecondary = 10,
        Focus = 11,
        Other = 99,
    }

    private static Color TokenColour(ThemeToken token, ThemePalette palette) => token switch
    {
        ThemeToken.Canvas => ManagerDesignTokens.ToGodot(palette.Canvas),
        ThemeToken.Navigation => ManagerDesignTokens.ToGodot(palette.Navigation),
        ThemeToken.Surface => ManagerDesignTokens.ToGodot(palette.Surface),
        ThemeToken.SurfaceRaised => ManagerDesignTokens.ToGodot(palette.SurfaceRaised),
        ThemeToken.Border => ManagerDesignTokens.ToGodot(palette.Border),
        ThemeToken.Accent => ManagerDesignTokens.ToGodot(palette.Accent),
        ThemeToken.Positive => ManagerDesignTokens.ToGodot(palette.Positive),
        ThemeToken.Warning => ManagerDesignTokens.ToGodot(palette.Warning),
        ThemeToken.Negative => ManagerDesignTokens.ToGodot(palette.Negative),
        ThemeToken.TextPrimary => ManagerDesignTokens.ToGodot(palette.TextPrimary),
        ThemeToken.TextSecondary => ManagerDesignTokens.ToGodot(palette.TextSecondary),
        ThemeToken.Focus => ManagerDesignTokens.ToGodot(palette.Focus),
        _ => ManagerDesignTokens.ToGodot(palette.Canvas),
    };

    private static ThemeToken? ClosestToken(Color colour)
    {
        const float tolerance = 1f / 255f;
        ThemePalette p = ManagerDesignTokens.ActivePalette;
        ThemeToken? Best(Color candidate, ThemeToken token)
        {
            float d = (candidate.R - colour.R) * (candidate.R - colour.R)
                      + (candidate.G - colour.G) * (candidate.G - colour.G)
                      + (candidate.B - colour.B) * (candidate.B - colour.B);
            return d <= tolerance ? token : null;
        }

        return Best(ManagerDesignTokens.ToGodot(p.Canvas), ThemeToken.Canvas)
               ?? Best(ManagerDesignTokens.ToGodot(p.Navigation), ThemeToken.Navigation)
               ?? Best(ManagerDesignTokens.ToGodot(p.Surface), ThemeToken.Surface)
               ?? Best(ManagerDesignTokens.ToGodot(p.SurfaceRaised), ThemeToken.SurfaceRaised)
               ?? Best(ManagerDesignTokens.ToGodot(p.Border), ThemeToken.Border)
               ?? Best(ManagerDesignTokens.ToGodot(p.Accent), ThemeToken.Accent)
               ?? Best(ManagerDesignTokens.ToGodot(p.Positive), ThemeToken.Positive)
               ?? Best(ManagerDesignTokens.ToGodot(p.Warning), ThemeToken.Warning)
               ?? Best(ManagerDesignTokens.ToGodot(p.Negative), ThemeToken.Negative)
               ?? Best(ManagerDesignTokens.ToGodot(p.TextPrimary), ThemeToken.TextPrimary)
               ?? Best(ManagerDesignTokens.ToGodot(p.TextSecondary), ThemeToken.TextSecondary)
               ?? Best(ManagerDesignTokens.ToGodot(p.Focus), ThemeToken.Focus);
    }
}
