using Godot;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Applies the chosen locale to the Godot runtime. The applier registers
/// the translation resources with <see cref="TranslationServer"/> and
/// triggers a redraw of the current scene so every <c>tr()</c> call
/// re-renders in the chosen language without a restart.
/// </summary>
public static class LocaleApplier
{
    private static readonly string[] ResourceExtensions = [".po", ".translation"];

    /// <summary>
    /// Loads the translations for the given <paramref name="localeCode"/>
    /// from the project's <c>locale/</c> directory and asks the
    /// <see cref="TranslationServer"/> to use them. The function is
    /// idempotent: calling it twice with the same locale is a no-op.
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
    /// <paramref name="localeDirectory"/> (a project-relative path). Each
    /// <c>.po</c> file becomes a <see cref="Translation"/> resource the
    /// server can consult. Files that fail to load are skipped silently so
    /// a translation error never blocks the game.
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

        foreach (string path in System.IO.Directory.EnumerateFiles(globalised))
        {
            if (ResourceLoader.Exists(path))
            {
                ResourceLoader.Load(path);
            }
        }
    }
}
