namespace PCDynasty.Presentation.Core;

/// <summary>
/// The bundle of choices the player makes in the settings board. Immutable:
/// a new value is created on every change. The store persists it; this type
/// only knows how to validate itself against the catalogs.
/// </summary>
/// <remarks>
/// A <c>record</c> with init-only properties rather than a class with a
/// hand-written constructor: every <c>WithX</c> below is one <c>with</c>
/// expression, so adding a preference no longer means editing a dozen
/// call sites that each re-list every field in order — which is exactly how
/// that kind of code starts silently swapping two booleans.
/// </remarks>
public sealed record PresentationPreferences
{
    /// <summary>Quietest level a bus can be set to without being muted outright.</summary>
    public const int MinimumVolume = 0;

    /// <summary>Loudest level; the scale is 0–100 so it survives a JSON round trip exactly.</summary>
    public const int MaximumVolume = 100;

    public bool Fullscreen { get; init; }
    public bool ReducedMotion { get; init; }
    public bool DenseTables { get; init; }
    public ThemePaletteId ThemeId { get; init; }
    public LocaleId LocaleId { get; init; }

    /// <summary>Whether the music bus plays at all.</summary>
    public bool MusicEnabled { get; init; }

    /// <summary>Music level, 0–100.</summary>
    public int MusicVolume { get; init; }

    /// <summary>Whether the effects bus plays at all.</summary>
    public bool EffectsEnabled { get; init; }

    /// <summary>Effects level, 0–100.</summary>
    public int EffectsVolume { get; init; }

    /// <summary>Master switch for every video clip.</summary>
    public bool VideoEnabled { get; init; }

    /// <summary>Whether the opening sequence plays on launch.</summary>
    public bool IntroVideoEnabled { get; init; }

    /// <summary>Whether the run-up to a match plays.</summary>
    public bool MatchVideoEnabled { get; init; }

    /// <summary>Whether trophy and title celebrations play.</summary>
    public bool CelebrationVideoEnabled { get; init; }

    /// <summary>The defaults the shell uses on first launch.</summary>
    public static PresentationPreferences Default { get; } = new()
    {
        Fullscreen = false,
        ReducedMotion = true,
        DenseTables = true,
        ThemeId = ThemePaletteId.Default,
        LocaleId = LocaleId.Default,
        MusicEnabled = true,
        MusicVolume = 70,
        EffectsEnabled = true,
        EffectsVolume = 80,
        VideoEnabled = true,
        IntroVideoEnabled = true,
        MatchVideoEnabled = true,
        CelebrationVideoEnabled = true,
    };

    public PresentationPreferences WithFullscreen(bool value) => this with { Fullscreen = value };

    public PresentationPreferences WithReducedMotion(bool value) => this with { ReducedMotion = value };

    public PresentationPreferences WithDenseTables(bool value) => this with { DenseTables = value };

    public PresentationPreferences WithTheme(ThemePaletteId value) => this with { ThemeId = value };

    public PresentationPreferences WithLocale(LocaleId value) => this with { LocaleId = value };

    public PresentationPreferences WithMusicEnabled(bool value) => this with { MusicEnabled = value };

    public PresentationPreferences WithMusicVolume(int value) => this with { MusicVolume = ClampVolume(value) };

    public PresentationPreferences WithEffectsEnabled(bool value) => this with { EffectsEnabled = value };

    public PresentationPreferences WithEffectsVolume(int value) => this with { EffectsVolume = ClampVolume(value) };

    public PresentationPreferences WithVideoEnabled(bool value) => this with { VideoEnabled = value };

    public PresentationPreferences WithIntroVideoEnabled(bool value) => this with { IntroVideoEnabled = value };

    public PresentationPreferences WithMatchVideoEnabled(bool value) => this with { MatchVideoEnabled = value };

    public PresentationPreferences WithCelebrationVideoEnabled(bool value) =>
        this with { CelebrationVideoEnabled = value };

    /// <summary>
    /// Whether a clip for the given moment should play. The master switch wins
    /// over the per-moment one, so turning video off is one click and needs no
    /// explanation.
    /// </summary>
    public bool PlaysVideo(VideoMoment moment) => VideoEnabled && moment switch
    {
        VideoMoment.Intro => IntroVideoEnabled,
        VideoMoment.Match => MatchVideoEnabled,
        VideoMoment.Celebration => CelebrationVideoEnabled,
        _ => true,
    };

    /// <summary>Validates the theme, the locale and the volumes against their bounds.</summary>
    public PresentationPreferences Resolve(ThemePaletteCatalog palettes, LocaleCatalog locales)
    {
        ArgumentNullException.ThrowIfNull(palettes);
        ArgumentNullException.ThrowIfNull(locales);
        return this with
        {
            ThemeId = palettes.Resolve(ThemeId).Id,
            LocaleId = locales.Resolve(LocaleId).Id,
            MusicVolume = ClampVolume(MusicVolume),
            EffectsVolume = ClampVolume(EffectsVolume),
        };
    }

    /// <summary>Equality that matches the persisted JSON shape: every field compared.</summary>
    /// <remarks>
    /// Kept as a named method because callers read better with it, but the
    /// record's own structural equality is what implements it now — so a new
    /// preference can never be forgotten here.
    /// </remarks>
    public bool ValueEquals(PresentationPreferences other) => Equals(other);

    private static int ClampVolume(int value) => Math.Clamp(value, MinimumVolume, MaximumVolume);
}

/// <summary>A moment in the game that can play a video clip.</summary>
public enum VideoMoment
{
    /// <summary>The opening sequence on launch.</summary>
    Intro = 0,

    /// <summary>The run-up to a match.</summary>
    Match = 1,

    /// <summary>Winning a trophy or a league.</summary>
    Celebration = 2,
}
