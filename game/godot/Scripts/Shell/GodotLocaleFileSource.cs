using System.Collections.Generic;
using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Production <see cref="ILocaleFileSource"/> for the shell. Reads
/// from <c>res://locale/langs/</c> via Godot's project-localised
/// resource paths; built-in locales live in the <c>built/</c>
/// subfolder, external mods drop a file directly under the root.
/// </summary>
/// <remarks>
/// The split between built-in and external folders is the dedupe
/// rule the registry applies: built-in wins. The
/// <see cref="LocaleSourceListing"/> carries the source per id so
/// the registry does not re-query the filesystem for the precedence
/// decision.
/// </remarks>
public sealed class GodotLocaleFileSource : ILocaleFileSource
{
    private readonly string _rootFolder;
    private readonly string _builtInFolder;

    public GodotLocaleFileSource(string rootFolder, string builtInFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(builtInFolder);
        _rootFolder = rootFolder;
        _builtInFolder = builtInFolder;
    }

    public string RootFolder => _rootFolder;

    public IEnumerable<LocaleSourceListing> ListIds()
    {
        foreach (LocaleSourceListing listing in EnumerateFolder(_builtInFolder, LocaleSource.BuiltIn))
        {
            yield return listing;
        }

        foreach (LocaleSourceListing listing in EnumerateFolder(_rootFolder, LocaleSource.External))
        {
            yield return listing;
        }
    }

    public string ResolvePath(string id) => $"{_rootFolder}{id}.json";

    public bool TryResolvePath(string id, out string path)
    {
        string builtIn = $"{_builtInFolder}{id}.json";
        if (Exists(builtIn))
        {
            path = builtIn;
            return true;
        }

        string external = $"{_rootFolder}{id}.json";
        if (Exists(external))
        {
            path = external;
            return true;
        }

        path = string.Empty;
        return false;
    }

    public bool FileExists(string path) => Exists(path);

    public string ReadAllText(string path) =>
        System.IO.File.ReadAllText(Godot.ProjectSettings.GlobalizePath(path));

    public bool IsBuiltIn(string path) => path.StartsWith(_builtInFolder, StringComparison.Ordinal);

    private static IEnumerable<LocaleSourceListing> EnumerateFolder(string folder, LocaleSource source)
    {
        string physical = Godot.ProjectSettings.GlobalizePath(folder);
        if (!System.IO.Directory.Exists(physical))
        {
            yield break;
        }

        foreach (string physicalPath in System.IO.Directory.EnumerateFiles(physical, "*.json"))
        {
            string filename = System.IO.Path.GetFileNameWithoutExtension(physicalPath);
            if (string.IsNullOrEmpty(filename))
            {
                continue;
            }

            yield return new LocaleSourceListing(filename, source);
        }
    }

    private static bool Exists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return System.IO.File.Exists(Godot.ProjectSettings.GlobalizePath(path));
    }
}
