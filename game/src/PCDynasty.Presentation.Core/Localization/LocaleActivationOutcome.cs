namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The outcome of <see cref="LocaleActivationService.Activate"/>. The
/// activation service never throws past its boundary; every failure
/// mode is encoded as a value so the bootstrap can branch on it
/// without try/catch boilerplate.
/// </summary>
public abstract record LocaleActivationOutcome
{
    private LocaleActivationOutcome() { }

    /// <summary>The active locale is exactly what the player asked for.</summary>
    public sealed record Activated(LocaleFile File, LocaleSource Source) : LocaleActivationOutcome;

    /// <summary>
    /// The active locale is a fallback (typically English) because the
    /// requested locale could not be loaded. <see cref="Reason"/>
    /// carries the short description the error panel renders.
    /// </summary>
    public sealed record Recovered(LocaleFile File, LocaleSource Source, string Reason) : LocaleActivationOutcome;

    /// <summary>
    /// The game cannot run because every candidate locale (the
    /// requested one and the English fallback) failed to load. The
    /// bootstrap shows the fatal panel and quits on user input.
    /// </summary>
    public sealed record Fatal(string Reason) : LocaleActivationOutcome;
}
