using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// Test double for <see cref="ILocaleFileSource"/>. The tests pass a
/// dictionary of path → content and a "built-in" prefix; the loader
/// reads exactly what the dictionary holds.
/// </summary>
internal sealed class InMemoryLocaleFileSource : ILocaleFileSource
{
    private readonly Dictionary<string, string> _contents;
    private readonly string _rootFolder;
    private readonly string _builtInPrefix;

    public InMemoryLocaleFileSource(
        Dictionary<string, string> contents,
        string rootFolder = "res://locale/langs/",
        string builtInPrefix = "res://locale/langs/built/")
    {
        _contents = contents;
        _rootFolder = rootFolder;
        _builtInPrefix = builtInPrefix;
    }

    public string RootFolder => _rootFolder;

    public IEnumerable<LocaleSourceListing> ListIds()
    {
        foreach (string path in _contents.Keys)
        {
            LocaleSource source = IsBuiltIn(path) ? LocaleSource.BuiltIn : LocaleSource.External;
            string? id = ExtractId(path);
            if (id is null)
            {
                continue;
            }

            yield return new LocaleSourceListing(id, source);
        }
    }

    private string? ExtractId(string path)
    {
        string baseFolder = IsBuiltIn(path) ? _builtInPrefix : _rootFolder;
        if (!path.StartsWith(baseFolder, StringComparison.Ordinal))
        {
            return null;
        }

        string remainder = path.Substring(baseFolder.Length);
        int dot = remainder.LastIndexOf(".json", StringComparison.Ordinal);
        if (dot <= 0)
        {
            return null;
        }

        return remainder.Substring(0, dot);
    }

    public string ResolvePath(string id) => $"{_rootFolder}{id}.json";

    public bool TryResolvePath(string id, out string path)
    {
        string builtIn = $"{_builtInPrefix}{id}.json";
        if (_contents.ContainsKey(builtIn))
        {
            path = builtIn;
            return true;
        }

        string external = $"{_rootFolder}{id}.json";
        if (_contents.ContainsKey(external))
        {
            path = external;
            return true;
        }

        path = string.Empty;
        return false;
    }

    public bool FileExists(string path) => _contents.ContainsKey(path);

    public string ReadAllText(string path) =>
        _contents.TryGetValue(path, out string? content)
            ? content
            : throw new FileNotFoundException($"Locale file '{path}' was not registered in the test source.");

    public bool IsBuiltIn(string path) => path.StartsWith(_builtInPrefix, StringComparison.Ordinal);
}
