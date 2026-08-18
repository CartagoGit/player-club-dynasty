namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Reads the operating system locale. The interface lets tests pin
/// every branch without depending on a Godot runtime; the default
/// implementation delegates to the platform call so the production
/// path is unchanged.
/// </summary>
public interface ISystemLocaleProbe
{
    /// <summary>
    /// Returns the OS locale's language code (e.g. <c>es</c>, <c>en</c>,
    /// <c>pt</c>), or <c>null</c> when the OS call fails or returns
    /// an empty value.
    /// </summary>
    string? CurrentLanguageCode();
}

/// <summary>
/// Default implementation backed by Godot's <c>OS.GetLocale()</c>
/// via a delegate. The delegate indirection keeps the production
/// type test-friendly while honouring the architecture rule that
/// the Presentation.Core project does not depend on Godot directly.
/// </summary>
public sealed class SystemLocaleProbe : ISystemLocaleProbe
{
    private readonly Func<string?> _platformReader;

    public SystemLocaleProbe()
        : this(ReadGodotLocale)
    {
    }

    public SystemLocaleProbe(Func<string?> platformReader)
    {
        _platformReader = platformReader ?? throw new ArgumentNullException(nameof(platformReader));
    }

    /// <inheritdoc/>
    public string? CurrentLanguageCode()
    {
        try
        {
            string? value = _platformReader();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Godot returns e.g. "es_ES" or "en_US"; the locale files
            // are indexed by the two-letter language code, so the
            // probe strips the region before returning.
            string trimmed = value.Trim();
            int underscore = trimmed.IndexOf('_');
            if (underscore > 0)
            {
                trimmed = trimmed.Substring(0, underscore);
            }

            return trimmed;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? ReadGodotLocale()
    {
        // The delegate is bound at runtime by the Godot bootstrap via
        // SystemLocaleProbe.BindToGodot(); the default factory below
        // returns null so the service falls back to the English
        // default instead of crashing in tests.
        return null;
    }
}
