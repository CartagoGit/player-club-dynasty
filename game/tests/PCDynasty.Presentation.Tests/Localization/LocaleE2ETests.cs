using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// End-to-end checks that wire every part of the locale pipeline
/// together. The loader, registry, activation service and picker
/// shape builder are exercised through the same interfaces the
/// bootstrap uses, only the filesystem is swapped for an in-memory
/// source so the tests stay fast and deterministic.
/// </summary>
public sealed class LocaleE2ETests
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
            return underscore > 0 ? trimmed.Substring(0, underscore) : trimmed;
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
    public void TheBootRegistryWithBothLanguagesReturnsActivatedForTheValidLocale()
    {
        LocaleActivationService service = BuildService(new StubProbe(null), ("es", SpanishJson), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate("es");

        LocaleActivationOutcome.Activated activated = Assert.IsType<LocaleActivationOutcome.Activated>(outcome);
        Assert.Equal("es", activated.File.Lang);
        Assert.Equal("Español", activated.File.Name);
        Assert.Equal("t", activated.File.Menu.Header.Title);
    }

    [Fact]
    public void TheBootRegistryWithBothLanguagesReturnsFatalWhenTheSavedAndFallbackAreBothBroken()
    {
        LocaleActivationService service = BuildService(new StubProbe(null));

        LocaleActivationOutcome outcome = service.Activate("es");

        LocaleActivationOutcome.Fatal fatal = Assert.IsType<LocaleActivationOutcome.Fatal>(outcome);
        Assert.Equal("es", fatal.SavedFailure.Id);
        Assert.Equal("en", fatal.FallbackFailure.Id);
        Assert.Equal(TranslationErrorPanelKind.Fatal, TranslationErrorPanelShapeBuilder.ForActivation(fatal).Kind);
    }

    [Fact]
    public void TheBootRegistryWithBothLanguagesReturnsRecoveredForEveryOtherFailure()
    {
        LocaleActivationService service = BuildService(new StubProbe(null), ("en", EnglishJson));

        LocaleActivationOutcome outcome = service.Activate("es");

        LocaleActivationOutcome.Recovered recovered = Assert.IsType<LocaleActivationOutcome.Recovered>(outcome);
        Assert.Equal("en", recovered.File.Lang);
        Assert.Equal("es", recovered.OriginalFailure.Id);
        Assert.Equal(TranslationErrorPanelKind.Recoverable, TranslationErrorPanelShapeBuilder.ForActivation(recovered).Kind);
    }

    [Fact]
    public void ThePickerRejectsATamperedFileWithPanelC()
    {
        // Tampered file: id matches but the menu section is missing.
        const string tampered = """
        {
          "lang": "es",
          "icon": "flags/es.svg",
          "name": "Español"
        }
        """;

        LocaleActivationService service = BuildService(new StubProbe(null), ("es", tampered), ("en", EnglishJson));

        LocaleLoadResult pick = service.TryLoad("es");
        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(pick);

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForPicker("es", invalid);

        Assert.Equal(TranslationErrorPanelKind.PickerRejected, shape.Kind);
        Assert.Equal("OK", shape.PrimaryButtonLabel);
        Assert.Contains("cannot be selected", shape.Body);
    }

    [Fact]
    public void GeneratingTwiceWithTheSameRecordsProducesByteIdenticalJson()
    {
        string folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"locale-e2e-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            var generator = new LocaleFileGenerator(folder);
            IReadOnlyList<string> first = generator.Generate(ShippedLocaleFiles.All);

            foreach (string path in first)
            {
                byte[] firstBytes = System.IO.File.ReadAllBytes(path);
                generator.Generate(ShippedLocaleFiles.All);
                byte[] secondBytes = System.IO.File.ReadAllBytes(path);
                Assert.Equal(firstBytes, secondBytes);
            }
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void TheGeneratedJsonRoundTripsThroughTheStrictLoader()
    {
        string folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"locale-e2e-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            new LocaleFileGenerator(folder).Generate(ShippedLocaleFiles.All);

            Dictionary<string, string> map = new();
            foreach (string path in System.IO.Directory.EnumerateFiles(folder, "*.json"))
            {
                map[$"res://locale/langs/built/{System.IO.Path.GetFileName(path)}"] =
                    System.IO.File.ReadAllText(path);
            }

            var source = new InMemoryLocaleFileSource(
                map,
                rootFolder: "res://locale/langs/built/",
                builtInPrefix: "res://locale/langs/built/");
            var loader = new LocaleLoader(source);
            var registry = new LocaleRegistry(source, loader);
            var service = new LocaleActivationService(
                registry,
                loader,
                new StubProbe("es_ES"));

            LocaleActivationOutcome outcome = service.Activate(savedLocaleId: null);

            LocaleActivationOutcome.Activated activated = Assert.IsType<LocaleActivationOutcome.Activated>(outcome);
            Assert.Equal("es", activated.File.Lang);
            Assert.Equal("Español", activated.File.Name);
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }
}
