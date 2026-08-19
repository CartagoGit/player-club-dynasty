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
    /// <summary>
    /// Plates are square. The classic look gets its relief from a bevel, not
    /// from a radius; the only rounded thing in the shell is a crest.
    /// </summary>
    public const int CornerRadius = 0;

    /// <summary>Radius used by the few genuinely round elements (crests, discs).</summary>
    public const int DiscRadius = 999;

    public const float MinimumTextContrast = 4.5f;

    /// <summary>Height of a caption bar: the coloured strip that names a plate.</summary>
    public const int CaptionHeight = 24;

    /// <summary>Height of the fixed command bar along the bottom of the screen.</summary>
    public const int CommandBarHeight = 52;

    /// <summary>Width of the rotated edge rail that names the area.</summary>
    public const int EdgeRailWidth = 30;

    /// <summary>The lit edge of a bevel: top and left.</summary>
    public static Color BevelLight => new(1f, 1f, 1f, 0.16f);

    /// <summary>The shadowed edge of a bevel: bottom and right.</summary>
    public static Color BevelShadow => new(0f, 0f, 0f, 0.42f);

    /// <summary>
    /// The colour that identifies a functional area. Derived from the active
    /// palette so a palette switch re-colours every area for free, and so no
    /// screen has to know a literal.
    /// </summary>
    public static Color AreaColor(ShellArea area) => area switch
    {
        ShellArea.Tracking => Focus,
        ShellArea.Technical => Mix(Focus, Navigation, 0.55f),
        ShellArea.Market => Positive,
        ShellArea.Finance => Warning,
        ShellArea.World => Accent,
        ShellArea.Settings => Mix(Focus, Accent, 0.5f),
        _ => Border,
    };

    /// <summary>
    /// The readable text colour to place on top of <paramref name="background"/>.
    /// Either the darkest ink or the lightest, never a shade of the background:
    /// a caption bar has to be read across the room, not squinted at.
    /// </summary>
    public static Color OnArea(Color background) =>
        Luminance(background) > 0.42f ? Darken(Canvas, 0.7f) : TextPrimary;

    /// <summary>Linear blend: 0 returns <paramref name="from"/>, 1 returns <paramref name="to"/>.</summary>
    public static Color Mix(Color from, Color to, float amount)
    {
        float t = Math.Clamp(amount, 0f, 1f);
        return new Color(
            from.R + ((to.R - from.R) * t),
            from.G + ((to.G - from.G) * t),
            from.B + ((to.B - from.B) * t),
            from.A + ((to.A - from.A) * t));
    }

    /// <summary>Multiplies the channels, keeping alpha. Used for pressed and disabled states.</summary>
    public static Color Darken(Color color, float factor) =>
        new(color.R * factor, color.G * factor, color.B * factor, color.A);

    /// <summary>Perceptual luminance, used to decide whether text on a colour must be dark.</summary>
    public static float Luminance(Color color) =>
        (0.2126f * color.R) + (0.7152f * color.G) + (0.0722f * color.B);

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
