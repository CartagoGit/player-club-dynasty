using System.Collections.Immutable;
using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The activation service is the only thing between a broken saved
/// choice and a player staring at a blank menu. The tests pin every
/// branch of the algorithm:
///   - valid saved id → Activated;
///   - broken saved id + valid English → Recovered;
///   - no saved id + matching OS locale → Activated;
///   - no saved id + broken OS locale + valid English → Recovered;
///   - broken saved id + broken English → Fatal.
/// </summary>
public sealed class LocaleActivationServiceTests
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

    private const string EnglishJson = """
    {
      "lang": "en",
      "icon": "flags/en.svg",
      "name": "English",
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

    private sealed class StubProbe : ISystemLocaleProbe
    {
        public StubProbe(string? value) { Value = value; }
        public string? Value { get; set; }
        public string? CurrentLanguageCode()
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                return null;
            }

            string trimmed = Value.Trim();
            int underscore = trimmed.IndexOf('_');
            if (underscore > 0)
            {
                trimmed = trimmed.Substring(0, underscore);
            }

            return trimmed;
        }
    }

    private static LocaleActivationService BuildService(
        ISystemLocaleProbe probe,
        params (string id, string content)[] files)
    {
        Dictionary<string, string> map = new();
        foreach ((string id, string content) in files)
        {
            map[$"res://locale/langs/built/{id}.json"] = content;
        }

        var source = new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/built/",
            builtInPrefix: "res://locale/langs/built/");
        var loader = new LocaleLoader(source);
        var registry = new LocaleRegistry(source, loader);
        return new LocaleActivationService(registry, loader, probe);
    }

    [Fact]
    public void AValidSavedLocaleReturnsActivated()
    {
        LocaleActivationService service = BuildService(new StubProbe(null), ("es", SpanishJson), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate("es");

        LocaleActivationOutcome.Activated activated = Assert.IsType<LocaleActivationOutcome.Activated>(outcome);
        Assert.Equal("es", activated.File.Lang);
        Assert.Equal(LocaleSource.BuiltIn, activated.Source);
    }

    [Fact]
    public void ABrokenSavedLocaleWithAWorkingEnglishFallbackReturnsRecovered()
    {
        // No "fr" file in the source: saved choice cannot be loaded,
        // English can, so the service recovers to English.
        LocaleActivationService service = BuildService(new StubProbe(null), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate("fr");

        LocaleActivationOutcome.Recovered recovered = Assert.IsType<LocaleActivationOutcome.Recovered>(outcome);
        Assert.Equal("en", recovered.File.Lang);
        Assert.Equal("fr", recovered.OriginalFailure.Id);
        Assert.Equal(LocaleLoadFailure.NotFound, recovered.OriginalFailure.Failure);
    }

    [Fact]
    public void NoSavedLocaleWithAMatchingOsLocaleActivatesThatLocale()
    {
        LocaleActivationService service = BuildService(new StubProbe("es_ES"), ("es", SpanishJson), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate(savedLocaleId: null);

        LocaleActivationOutcome.Activated activated = Assert.IsType<LocaleActivationOutcome.Activated>(outcome);
        Assert.Equal("es", activated.File.Lang);
    }

    [Fact]
    public void NoSavedLocaleWithAMissingOsLocaleFallsBackToEnglish()
    {
        // OS returns a language the source does not ship. The service
        // walks the activation algorithm: saved id empty → OS "ko"
        // fails to load → English recovers.
        LocaleActivationService service = BuildService(new StubProbe("ko_KR"), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate(savedLocaleId: null);

        LocaleActivationOutcome.Recovered recovered = Assert.IsType<LocaleActivationOutcome.Recovered>(outcome);
        Assert.Equal("en", recovered.File.Lang);
        Assert.Equal("ko", recovered.OriginalFailure.Id);
    }

    [Fact]
    public void ABrokenSavedLocaleWithABrokenEnglishReturnsFatal()
    {
        // No Spanish, no English → no file at all → fatal.
        LocaleActivationService service = BuildService(new StubProbe(null));

        LocaleActivationOutcome outcome = service.Activate("fr");

        LocaleActivationOutcome.Fatal fatal = Assert.IsType<LocaleActivationOutcome.Fatal>(outcome);
        Assert.Equal("fr", fatal.SavedFailure.Id);
        Assert.Equal("en", fatal.FallbackFailure.Id);
    }

    [Fact]
    public void AnEmptySavedLocaleUsesTheOsLocale()
    {
        LocaleActivationService service = BuildService(new StubProbe("es"), ("es", SpanishJson), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate(savedLocaleId: string.Empty);

        LocaleActivationOutcome.Activated activated = Assert.IsType<LocaleActivationOutcome.Activated>(outcome);
        Assert.Equal("es", activated.File.Lang);
    }

    [Fact]
    public void TryLoadReturnsTheTypedResultForTheGivenId()
    {
        LocaleActivationService service = BuildService(new StubProbe(null), ("es", SpanishJson), ("en", EnglishJson));

        LocaleLoadResult result = service.TryLoad("es");

        LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
        Assert.Equal("es", ok.File.Lang);
    }

    [Fact]
    public void TryLoadReturnsInvalidForAMissingId()
    {
        LocaleActivationService service = BuildService(new StubProbe(null));

        LocaleLoadResult result = service.TryLoad("fr");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.NotFound, invalid.Failure);
    }

    [Fact]
    public void ScanDelegatesToTheRegistry()
    {
        LocaleActivationService service = BuildService(new StubProbe(null), ("es", SpanishJson), ("en", EnglishJson));

        LocaleRegistryResult result = service.Scan();

        Assert.Equal(2, result.Locales.Length);
    }

    [Fact]
    public void TheSystemLocaleProbeStripsTheRegionCode()
    {
        var probe = new SystemLocaleProbe(() => "es_ES");

        Assert.Equal("es", probe.CurrentLanguageCode());
    }

    [Fact]
    public void TheSystemLocaleProbeReturnsNullWhenThePlatformThrows()
    {
        var probe = new SystemLocaleProbe(() => throw new InvalidOperationException("no platform"));

        Assert.Null(probe.CurrentLanguageCode());
    }

    [Fact]
    public void TheSystemLocaleProbeReturnsNullWhenThePlatformReturnsEmpty()
    {
        var probe = new SystemLocaleProbe(() => string.Empty);

        Assert.Null(probe.CurrentLanguageCode());
    }
}
