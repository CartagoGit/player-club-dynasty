using System.Globalization;
using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// The runtime view of the active locale. The session is built once
/// during bootstrap activation and survives every screen until the
/// player quits. Screens read strings through the session's
/// <see cref="ActiveFile"/>; the picker (S7) writes through the same
/// handle so a language switch is one assignment.
/// </summary>
public sealed class LocaleSession
{
    private LocaleFile _activeFile = null!;
    private LocaleSource _activeSource = LocaleSource.BuiltIn;

    /// <summary>The loaded <see cref="LocaleFile"/> the screens read from.</summary>
    public LocaleFile ActiveFile => _activeFile;

    /// <summary>Where the active file came from (built-in or external mod).</summary>
    public LocaleSource ActiveSource => _activeSource;

    /// <summary>The id the session last activated.</summary>
    public string ActiveId => _activeFile.Lang;

    /// <summary>
    /// Applies an activation outcome. Activated and Recovered both
    /// commit the file; Fatal is rejected because the bootstrap never
    /// reaches this point when Fatal happens.
    /// </summary>
    public void Apply(LocaleActivationOutcome outcome)
    {
        switch (outcome)
        {
            case LocaleActivationOutcome.Activated activated:
                _activeFile = activated.File;
                _activeSource = activated.Source;
                AdoptCulture();
                break;
            case LocaleActivationOutcome.Recovered recovered:
                _activeFile = recovered.File;
                _activeSource = recovered.Source;
                AdoptCulture();
                break;
            default:
                throw new ArgumentException(
                    $"Outcome '{outcome.GetType().Name}' cannot activate a session.",
                    nameof(outcome));
        }
    }

    /// <summary>
    /// Applies a fresh load result from the picker. Used by the
    /// language picker (S7) when the player taps a flag.
    /// </summary>
    public void Apply(LocaleLoadResult.Ok ok)
    {
        _activeFile = ok.File;
        _activeSource = ok.Source;
        AdoptCulture();
    }

    /// <summary>
    /// Points .NET's current culture at the active locale, so anything the
    /// framework formats — weekday and month names on the date plate, decimal
    /// separators, currency — follows the language the player chose instead of
    /// the language the operating system happens to run in.
    /// </summary>
    private void AdoptCulture()
    {
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(_activeFile.Lang);
        }
        catch (CultureNotFoundException)
        {
            // A locale id the runtime does not know is still a valid game
            // language; it just formats with the invariant rules.
            culture = CultureInfo.InvariantCulture;
        }

        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }
}
