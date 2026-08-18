using Godot;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Applies the chosen locale to the Godot runtime. The applier registers the
/// translation resources with <see cref="TranslationServer"/> and triggers
/// a redraw of the current scene so every <c>tr()</c> call re-renders in
/// the chosen language without a restart.
/// </summary>
public static class LocaleApplier
{
    /// <summary>
    /// Sets the active locale on the <see cref="TranslationServer"/>. The
    /// function is idempotent: calling it twice with the same locale is a
    /// no-op.
    /// </summary>
    public static void Apply(string localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode))
        {
            return;
        }

        TranslationServer.SetLocale(localeCode);
    }

    /// <summary>
    /// Loads the project's translation resources from the supplied
    /// <paramref name="localeDirectory"/> (a Godot resource path). Each
    /// <c>.po</c> file becomes a <see cref="Translation"/> resource the
    /// server can consult. Files that fail to load are skipped silently so
    /// a missing translation never blocks the game.
    /// </summary>
    public static void RegisterTranslations(string localeDirectory)
    {
        if (string.IsNullOrWhiteSpace(localeDirectory))
        {
            return;
        }

        string globalised = ProjectSettings.GlobalizePath(localeDirectory);
        if (!System.IO.Directory.Exists(globalised))
        {
            return;
        }

        foreach (string path in System.IO.Directory.EnumerateFiles(globalised, "*.po"))
        {
            Resource? resource = ResourceLoader.Load(path);
            if (resource is Translation translation)
            {
                TranslationServer.AddTranslation(translation);
            }
        }
    }
}
