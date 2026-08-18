namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Runs the activation algorithm from the f00046 proposal:
///
/// <list type="number">
/// <item>If the player's saved choice loads, return <see cref="LocaleActivationOutcome.Activated"/>.</item>
/// <item>If it fails, try the English fallback. OK returns
///       <see cref="LocaleActivationOutcome.Recovered"/> with the reason.</item>
/// <item>If English itself fails, return
///       <see cref="LocaleActivationOutcome.Fatal"/>.</item>
/// <item>When no choice is saved, mirror the OS locale. Missing or
///       broken OS locale falls back to English exactly like the saved path.</item>
/// </list>
///
/// The service never throws past its boundary: every I/O path goes
/// through the typed loader, which already returns
/// <see cref="LocaleLoadResult"/> values.
/// </summary>
public sealed class LocaleActivationService
{
    /// <summary>The id the service falls back to when everything else fails.</summary>
    public const string FallbackLocaleId = "en";

    private readonly LocaleRegistry _registry;
    private readonly LocaleLoader _loader;
    private readonly ISystemLocaleProbe _probe;

    public LocaleActivationService(
        LocaleRegistry registry,
        LocaleLoader loader,
        ISystemLocaleProbe probe)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
    }

    /// <summary>
    /// Activates the locale for the supplied preference snapshot. A
    /// <c>null</c> or empty <paramref name="savedLocaleId"/> means the
    /// player has never chosen a language; the service consults the
    /// OS locale in that case.
    /// </summary>
    public LocaleActivationOutcome Activate(string? savedLocaleId)
    {
        // A missing saved id is not the same as a broken saved id: the
        // activation algorithm reads the OS locale on the first launch
        // and the player's choice on every subsequent one.
        string? candidateId = string.IsNullOrWhiteSpace(savedLocaleId)
            ? _probe.CurrentLanguageCode()
            : savedLocaleId;

        LocaleLoadResult requested = TryLoad(candidateId);
        if (requested is LocaleLoadResult.Ok ok)
        {
            return new LocaleActivationOutcome.Activated(ok.File, ok.Source);
        }

        string reason = DescribeFailure(candidateId, requested);

        LocaleLoadResult fallback = TryLoad(FallbackLocaleId);
        if (fallback is LocaleLoadResult.Ok fallbackOk)
        {
            return new LocaleActivationOutcome.Recovered(fallbackOk.File, fallbackOk.Source, reason);
        }

        string fallbackReason = DescribeFailure(FallbackLocaleId, fallback);
        return new LocaleActivationOutcome.Fatal(
            $"Neither the saved locale nor the fallback '{FallbackLocaleId}' could be loaded. " +
            $"Saved: {reason}. Fallback: {fallbackReason}.");
    }

    /// <summary>
    /// Strict-loads a locale by id; returns the typed result. Exposed
    /// for the picker (S7) which calls the same load path on every
    /// tap without persisting.
    /// </summary>
    public LocaleLoadResult TryLoad(string? localeId)
    {
        if (string.IsNullOrWhiteSpace(localeId))
        {
            return LocaleLoadFailureResult(localeId ?? string.Empty);
        }

        return _loader.Load(localeId);
    }

    /// <summary>
    /// Returns the metadata list the picker renders. Exposed so the
    /// picker does not need its own reference to the registry.
    /// </summary>
    public LocaleRegistryResult Scan() => _registry.Scan();

    private static LocaleLoadResult.Invalid LocaleLoadFailureResult(string id) =>
        new LocaleLoadResult.Invalid(
            Id: id,
            Path: string.Empty,
            Failure: LocaleLoadFailure.NotFound,
            Diagnostics: System.Collections.Immutable.ImmutableArray<LocaleDiagnostic>.Empty);

    private static string DescribeFailure(string? localeId, LocaleLoadResult result) =>
        result switch
        {
            LocaleLoadResult.Invalid invalid => BuildFailureMessage(localeId ?? invalid.Id, invalid),
            _ => $"locale '{localeId}' could not be activated.",
        };

    private static string BuildFailureMessage(string id, LocaleLoadResult.Invalid invalid)
    {
        if (invalid.Diagnostics.IsDefaultOrEmpty)
        {
            return $"locale '{id}' failed: {invalid.Failure}.";
        }

        LocaleDiagnostic first = invalid.Diagnostics[0];
        return $"locale '{id}' failed: {invalid.Failure} at {first.Path}: {first.Message}";
    }
}
