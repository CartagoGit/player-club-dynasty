namespace PCDynasty.Presentation.Core;

/// <summary>
/// The bundle of choices the player makes in the options panel. The record
/// is immutable: a new value is created on every change. The store is
/// responsible for persisting it; this type only knows how to validate the
/// values it carries against the catalogs.
/// </summary>
public sealed class PresentationPreferences
{
    public bool Fullscreen { get; }
    public bool ReducedMotion { get; }
    public bool DenseTables { get; }
    public ThemePaletteId ThemeId { get; }
    public LocaleId LocaleId { get; }

    public PresentationPreferences(
        bool fullscreen,
        bool reducedMotion,
        bool denseTables,
        ThemePaletteId themeId,
        LocaleId localeId)
    {
        Fullscreen = fullscreen;
        ReducedMotion = reducedMotion;
        DenseTables = denseTables;
        ThemeId = themeId;
        LocaleId = localeId;
    }

    /// <summary>The defaults the shell uses on first launch.</summary>
    public static PresentationPreferences Default { get; } = new(
        fullscreen: false,
        reducedMotion: true,
        denseTables: true,
        themeId: ThemePaletteId.Default,
        localeId: LocaleId.Default);

    /// <summary>Returns a new instance with the same values plus a fullscreen override.</summary>
    public PresentationPreferences WithFullscreen(bool value) =>
        new(value, ReducedMotion, DenseTables, ThemeId, LocaleId);

    /// <summary>Returns a new instance with the same values plus a reduced-motion override.</summary>
    public PresentationPreferences WithReducedMotion(bool value) =>
        new(Fullscreen, value, DenseTables, ThemeId, LocaleId);

    /// <summary>Returns a new instance with the same values plus a dense-tables override.</summary>
    public PresentationPreferences WithDenseTables(bool value) =>
        new(Fullscreen, ReducedMotion, value, ThemeId, LocaleId);

    /// <summary>Returns a new instance with the same values plus a theme palette override.</summary>
    public PresentationPreferences WithTheme(ThemePaletteId value) =>
        new(Fullscreen, ReducedMotion, DenseTables, value, LocaleId);

    /// <summary>Returns a new instance with the same values plus a locale override.</summary>
    public PresentationPreferences WithLocale(LocaleId value) =>
        new(Fullscreen, ReducedMotion, DenseTables, ThemeId, value);

    /// <summary>Validates both the theme and the locale against the supplied catalogs.</summary>
    public PresentationPreferences Resolve(ThemePaletteCatalog palettes, LocaleCatalog locales)
    {
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(locales);
        ThemePaletteId theme = palettes.Resolve(ThemeId).Id;
        LocaleId locale = locales.Resolve(LocaleId).Id;
        return new PresentationPreferences(Fullscreen, ReducedMotion, DenseTables, theme, locale);
    }

    /// <summary>Equality that matches the persisted JSON shape: every field compared.</summary>
    public bool ValueEquals(PresentationPreferences other) =>
        other is not null
        && Fullscreen == other.Fullscreen
        && ReducedMotion == other.ReducedMotion
        && DenseTables == other.DenseTables
        && ThemeId == other.ThemeId
        && LocaleId == other.LocaleId;
}
