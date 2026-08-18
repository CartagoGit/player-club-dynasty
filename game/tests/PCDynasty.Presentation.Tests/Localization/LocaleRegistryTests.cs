using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The registry is the bridge between the loader and the language
/// picker. The tests pin three contracts: built-in wins over external,
/// duplicate ids are reported (not silently dropped), and the list is
/// sorted so the picker is stable between sessions.
/// </summary>
public sealed class LocaleRegistryTests
{
    private const string SpanishJson = """
    {
      "lang": "es",
      "icon": "flags/es.svg",
      "name": "Español"
    }
    """;

    private const string EnglishJson = """
    {
      "lang": "en",
      "icon": "flags/en.svg",
      "name": "English"
    }
    """;

    private const string PortugueseJson = """
    {
      "lang": "pt",
      "icon": "flags/pt.svg",
      "name": "Português"
    }
    """;

    private static readonly string[] ExpectedOrder = new[] { "en", "es", "pt" };

    private static LocaleRegistry BuildRegistry(params (string path, string content)[] files)
    {
        Dictionary<string, string> map = new();
        foreach ((string path, string content) in files)
        {
            map[path] = content;
        }

        var source = new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/",
            builtInPrefix: "res://locale/langs/built/");
        var loader = new LocaleLoader(source);
        return new LocaleRegistry(source, loader);
    }

    [Fact]
    public void ABuiltInLocaleAppearsInTheResult()
    {
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/built/es.json", SpanishJson));

        LocaleRegistryResult result = registry.Scan();

        LocaleMetadata meta = Assert.Single(result.Locales);
        Assert.Equal("es", meta.Id);
        Assert.Equal("Español", meta.Name);
        Assert.Equal(LocaleSource.BuiltIn, meta.Source);
    }

    [Fact]
    public void AnExternalLocaleAppearsWithTheExternalTag()
    {
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/pt.json", PortugueseJson));

        LocaleRegistryResult result = registry.Scan();

        LocaleMetadata meta = Assert.Single(result.Locales);
        Assert.Equal(LocaleSource.External, meta.Source);
    }

    [Fact]
    public void BuiltInAndExternalBothAppearWhenTheirIdsDiffer()
    {
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/built/es.json", SpanishJson),
            ("res://locale/langs/pt.json", PortugueseJson));

        LocaleRegistryResult result = registry.Scan();

        Assert.Equal(2, result.Locales.Length);
        Assert.Equal("es", result.Locales[0].Id);
        Assert.Equal("pt", result.Locales[1].Id);
        Assert.Equal(LocaleSource.BuiltIn, result.Locales[0].Source);
        Assert.Equal(LocaleSource.External, result.Locales[1].Source);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void BuiltInWinsOverAnExternalWithTheSameId()
    {
        // Same id "es" on both sides; only the built-in survives.
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/built/es.json", SpanishJson),
            ("res://locale/langs/es.json", EnglishJson));

        LocaleRegistryResult result = registry.Scan();

        LocaleMetadata meta = Assert.Single(result.Locales);
        Assert.Equal("es", meta.Id);
        Assert.Equal("Español", meta.Name);
        Assert.Equal(LocaleSource.BuiltIn, meta.Source);

        LocaleRegistryError error = Assert.Single(result.Errors);
        Assert.Equal("es", error.Id);
        Assert.Contains("overridden", error.Reason);
    }

    [Fact]
    public void TheResultIsSortedById()
    {
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/built/pt.json", PortugueseJson),
            ("res://locale/langs/built/es.json", SpanishJson),
            ("res://locale/langs/en.json", EnglishJson));

        LocaleRegistryResult result = registry.Scan();

        Assert.Equal(ExpectedOrder, result.Locales.Select(m => m.Id).ToArray());
    }

    [Fact]
    public void AnUnreadableFileIsReportedAsAnError()
    {
        // "broken.json" exists in the source but Peek returns null
        // because its content has no metadata.
        LocaleRegistry registry = BuildRegistry(
            ("res://locale/langs/built/broken.json", "{ not json"));

        LocaleRegistryResult result = registry.Scan();

        Assert.Empty(result.Locales);
        LocaleRegistryError error = Assert.Single(result.Errors);
        Assert.Equal("broken", error.Id);
    }

    [Fact]
    public void AnEmptySourceReturnsAnEmptyResultWithNoErrors()
    {
        LocaleRegistry registry = BuildRegistry();

        LocaleRegistryResult result = registry.Scan();

        Assert.Empty(result.Locales);
        Assert.Empty(result.Errors);
    }
}
