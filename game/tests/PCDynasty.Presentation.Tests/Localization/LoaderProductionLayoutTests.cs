using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// Regression: in production the root folder is <c>res://locale/langs/</c>
/// and the built-in files live under <c>res://locale/langs/built/</c>.
/// The loader used to consult only the root folder through
/// <see cref="ILocaleFileSource.ResolvePath"/>, so every Load failed
/// with NotFound. These tests pin the production layout: the file
/// under built/ resolves and the file under the root resolves (for
/// external mods).
/// </summary>
public sealed class LoaderProductionLayoutTests
{
    private const string SpanishJson = """
    {
      "lang": "es",
      "icon": "flags/es.svg",
      "name": "Español",
      "menu": {
        "header": { "eyebrow": "e", "title": "t", "summary": "s" },
        "actions": { "newCareer": "a", "continue": "a", "load": "a", "dataEditor": "a", "options": "a", "exit": "a" }
      },
      "options": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "fullscreen": "a", "reducedMotion": "a", "denseTables": "a", "themeHeader": "a", "themeHint": "a", "languageHeader": "a", "languageHint": "a", "actions": { "saveAndClose": "a", "revert": "a" }, "hint": "a", "rejectionMessage": { "title": "a", "body": "a" } },
      "squad": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryOwnedFormat": "a", "summaryBrowseFormat": "a", "actionOpenMarket": "a", "actionOpenMarketTooltip": "a", "empty": "a", "columns": { "player": "a", "nation": "a", "age": "a", "tec": "a", "for": "a", "fis": "a", "men": "a", "val": "a", "action": "a" }, "actions": { "inspect": "a", "inspectTooltip": "a" } },
      "market": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "noticeIdle": "a", "noticeAcceptedFormat": "a", "noticeRejectedFormat": "a", "empty": "a", "columns": { "player": "a", "sellingClub": "a", "age": "a", "tec": "a", "val": "a", "price": "a", "action": "a" }, "actions": { "sign": "a", "signTooltipFormat": "a" }, "reasons": { "budget": "a", "price": "a", "unavailable": "a", "stale": "a", "finance": "a", "generic": "a" } },
      "history": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "filterEmpty": "a", "filters": { "competitionLabel": "a", "clubLabel": "a", "orderLabel": "a", "allOption": "a", "orderOptions": { "date": "a", "competition": "a", "goals": "a" } }, "aggregate": { "title": "a", "empty": "a" }, "columns": { "when": "a", "competition": "a", "home": "a", "score": "a", "away": "a", "seed": "a", "action": "a" }, "actions": { "view": "a", "viewTooltip": "a" } },
      "competitions": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "readOnly": { "title": "a", "body": "a" }, "entry": { "eyebrow": "a", "summaryFormat": "a", "rivalsPrefix": "a" }, "actions": { "playNext": "a", "playNextTooltip": "a", "playSeasonFormat": "a" } },
      "world": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "columns": { "name": "a", "format": "a", "frequency": "a", "rivals": "a", "action": "a" }, "actions": { "inspect": "a", "inspectTooltip": "a" } },
      "picker": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "ownedBadge": "a", "empty": "a", "columns": { "club": "a", "country": "a", "founded": "a", "prestige": "a", "fanbase": "a", "squad": "a", "action": "a" }, "actions": { "choose": "a", "chooseTooltip": "a" } }
    }
    """;

    private static LocaleLoader BuildProductionLayoutLoader(string content, string id)
    {
        // Mirrors the production layout: root is res://locale/langs/,
        // built-in lives in res://locale/langs/built/. ResolvePath
        // (which the old Load relied on) returns the external path;
        // the file the production loader must read lives one folder
        // deeper.
        Dictionary<string, string> map = new()
        {
            [$"res://locale/langs/built/{id}.json"] = content,
        };
        var source = new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/",
            builtInPrefix: "res://locale/langs/built/");
        return new LocaleLoader(source);
    }

    [Fact]
    public void LoadFindsTheBuiltInFileWhenTheRootFolderDoesNotContainIt()
    {
        LocaleLoader loader = BuildProductionLayoutLoader(SpanishJson, "es");

        LocaleLoadResult result = loader.Load("es");

        LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
        Assert.Equal("es", ok.File.Lang);
        Assert.Equal("Español", ok.File.Name);
        Assert.Equal(LocaleSource.BuiltIn, ok.Source);
    }

    [Fact]
    public void LoadFromRootFolderAlsoWorksForExternalMods()
    {
        Dictionary<string, string> map = new()
        {
            [$"res://locale/langs/pt.json"] = SpanishJson.Replace("\"es\"", "\"pt\"").Replace("Español", "Português"),
        };
        var source = new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/",
            builtInPrefix: "res://locale/langs/built/");
        var loader = new LocaleLoader(source);

        LocaleLoadResult result = loader.Load("pt");

        LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
        Assert.Equal(LocaleSource.External, ok.Source);
    }

    [Fact]
    public void LoadReturnsNotFoundWhenNeitherFolderContainsTheFile()
    {
        var source = new InMemoryLocaleFileSource(
            new Dictionary<string, string>(),
            rootFolder: "res://locale/langs/",
            builtInPrefix: "res://locale/langs/built/");
        var loader = new LocaleLoader(source);

        LocaleLoadResult result = loader.Load("zz");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.NotFound, invalid.Failure);
        Assert.Equal("zz", invalid.Id);
    }
}
