using Godot;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The shell's design tokens: colours, spacing, sizes. The static surface
/// stays read-only for backwards compatibility (225 callers across the
/// project use <c>ManagerDesignTokens.Surface</c> etc.), but the colour
/// tokens are now backed by a mutable <see cref="ActivePalette"/> so the
/// options panel can re-skin the shell at runtime without rebuilding every
/// StyleBox by hand.
/// </summary>
/// <remarks>
/// Spacing and sizes are immutable because they describe the layout grid,
/// not the palette. Only the colour tokens delegate to the palette. The
/// default palette is the same one the shell shipped with before this
/// proposal; the rest of the shell keeps working because the colour values
/// match the bytes the old constants held.
/// </remarks>
public static class ManagerDesignTokens
{
    /// <summary>The palette every colour token currently resolves through.</summary>
    public static ThemePalette ActivePalette { get; private set; } = ThemePaletteCatalog.Shipped.Resolve(ThemePaletteId.ClassicDark);

    /// <summary>The twelve colour tokens the shell applies. Backed by the active palette.</summary>
    public static Color Canvas => ToGodot(ActivePalette.Canvas);

    public static Color Navigation => ToGodot(ActivePalette.Navigation);
    public static Color Surface => ToGodot(ActivePalette.Surface);
    public static Color SurfaceRaised => ToGodot(ActivePalette.SurfaceRaised);
    public static Color Border => ToGodot(ActivePalette.Border);
    public static Color Accent => ToGodot(ActivePalette.Accent);
    public static Color Positive => ToGodot(ActivePalette.Positive);
    public static Color Warning => ToGodot(ActivePalette.Warning);
    public static Color Negative => ToGodot(ActivePalette.Negative);
    public static Color TextPrimary => ToGodot(ActivePalette.TextPrimary);
    public static Color TextSecondary => ToGodot(ActivePalette.TextSecondary);
    public static Color Focus => ToGodot(ActivePalette.Focus);

    public const int Space2 = 2;
    public const int Space4 = 4;
    public const int Space8 = 8;
    public const int Space12 = 12;
    public const int Space16 = 16;
    public const int Space24 = 24;
    public const int Space32 = 32;
    public const int Space48 = 48;

    public const int SidebarWidth = 248;
    public const int ContentMaximumWidth = 1440;
    public const int MinimumPointerTarget = 40;

    /// <summary>Height of the persistent club header.</summary>
    public const int HeaderHeight = 64;

    /// <summary>Height of the status strip along the bottom of the screen.</summary>
    public const int StatusBarHeight = 30;

    /// <summary>Width of the area rail.</summary>
    public const int RailWidth = 232;

    /// <summary>Table row heights; the dense one is the classic default.</summary>
    public const int RowHeightDense = 26;

    public const int RowHeightComfortable = 34;
    public const int CornerRadius = 6;
    public const float MinimumTextContrast = 4.5f;

    /// <summary>
    /// Replaces the active palette. The shell's <see cref="ThemePaletteApplier"/>
    /// is responsible for pushing the new tokens through the scene tree; this
    /// method only swaps the source. Returns the previous palette for callers
    /// that want to assert before / after.
    /// </summary>
    public static ThemePalette SetPalette(ThemePalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ThemePalette previous = ActivePalette;
        ActivePalette = palette;
        return previous;
    }

    /// <summary>Converts the domain <see cref="ThemeColor"/> into a Godot <see cref="Color"/>.</summary>
    public static Color ToGodot(ThemeColor color) => new(color.R, color.G, color.B, color.A);
}
