using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// Test double for <see cref="ILocaleFileSource"/>. The tests pass a
/// dictionary of id → content and a "built-in" prefix; the loader
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

    public string ResolvePath(string id) => $"{_rootFolder}{id}.json";

    public bool FileExists(string path) => _contents.ContainsKey(path);

    public string ReadAllText(string path) =>
        _contents.TryGetValue(path, out string? content)
            ? content
            : throw new FileNotFoundException($"Locale file '{path}' was not registered in the test source.");

    public bool IsBuiltIn(string path) => path.StartsWith(_builtInPrefix, StringComparison.Ordinal);
}
