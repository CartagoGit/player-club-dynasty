namespace PCDynasty.Presentation.Core;

/// <summary>
/// One named collection of colours the shell can render with. The palette is
/// the only place where a colour is defined; <c>ManagerDesignTokens</c> in
/// the Godot layer reads from the active instance.
/// </summary>
/// <remarks>
/// Minimum contrast between <see cref="TextPrimary"/> and <see cref="Canvas"/>
/// is enforced at construction time so a poorly authored palette can never
/// make the menu unreadable. The design system targets WCAG 4.5:1 for body
/// text; the constructor throws when a palette falls short.
/// </remarks>
public sealed class ThemePalette
{
    public ThemePaletteId Id { get; }
    public string DisplayName { get; }

    public ThemeColor Canvas { get; }
    public ThemeColor Navigation { get; }
    public ThemeColor Surface { get; }
    public ThemeColor SurfaceRaised { get; }
    public ThemeColor Border { get; }
    public ThemeColor Accent { get; }
    public ThemeColor Positive { get; }
    public ThemeColor Warning { get; }
    public ThemeColor Negative { get; }
    public ThemeColor TextPrimary { get; }
    public ThemeColor TextSecondary { get; }
    public ThemeColor Focus { get; }

    public ThemePalette(
        ThemePaletteId id,
        string displayName,
        ThemeColor canvas,
        ThemeColor navigation,
        ThemeColor surface,
        ThemeColor surfaceRaised,
        ThemeColor border,
        ThemeColor accent,
        ThemeColor positive,
        ThemeColor warning,
        ThemeColor negative,
        ThemeColor textPrimary,
        ThemeColor textSecondary,
        ThemeColor focus,
        float minimumContrast = 4.5f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Id = id;
        DisplayName = displayName;

        Canvas = canvas;
        Navigation = navigation;
        Surface = surface;
        SurfaceRaised = surfaceRaised;
        Border = border;
        Accent = accent;
        Positive = positive;
        Warning = warning;
        Negative = negative;
        TextPrimary = textPrimary;
        TextSecondary = textSecondary;
        Focus = focus;

        // The accent is the colour that draws the eye; it must remain legible
        // on both the canvas and the surface it normally sits on.
        float accentOnCanvas = Accent.ContrastRatio(Canvas);
        if (accentOnCanvas < minimumContrast)
        {
            throw new ArgumentException(
                $"Palette '{id}' accent contrast against canvas is {accentOnCanvas:F2}:1, below the {minimumContrast}:1 minimum.",
                nameof(accent));
        }

        float textOnCanvas = TextPrimary.ContrastRatio(Canvas);
        if (textOnCanvas < minimumContrast)
        {
            throw new ArgumentException(
                $"Palette '{id}' primary text contrast against canvas is {textOnCanvas:F2}:1, below the {minimumContrast}:1 minimum.",
                nameof(textPrimary));
        }
    }
}
