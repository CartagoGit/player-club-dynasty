using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The strict loader is the only thing standing between a broken
/// third-party JSON and a player who can't read the menu. The tests
/// pin every failure mode to a concrete diagnostic so the error
/// panel never has to guess.
/// </summary>
public sealed class LocaleLoaderTests
{
    private const string CanonicalSpanishJson = """
    {
      "lang": "es",
      "icon": "flags/es.svg",
      "name": "Español",
      "menu": {
        "header": { "eyebrow": "PC DYNASTY · DIRECCIÓN DEPORTIVA", "title": "TU CARRERA EMPIEZA AQUÍ", "summary": "Gestiona plantilla, táctica, mercado y patrimonio." },
        "actions": {
          "newCareer": "NUEVA CARRERA", "continue": "CONTINUAR", "load": "CARGAR CARRERA",
          "dataEditor": "EDITOR DE DATOS", "options": "OPCIONES", "exit": "SALIR"
        }
      },
      "options": {
        "header": { "eyebrow": "CONFIGURACIÓN", "title": "OPCIONES DE PRESENTACIÓN", "summary": "" },
        "fullscreen": "Pantalla completa", "reducedMotion": "Transiciones reducidas y navegación inmediata",
        "denseTables": "Tablas compactas de alta densidad",
        "themeHeader": "TEMA", "themeHint": "Cambia la paleta de la interfaz.",
        "languageHeader": "IDIOMA", "languageHint": "La interfaz del juego; el contenido no cambia.",
        "actions": { "saveAndClose": "GUARDAR Y CERRAR", "revert": "RESTAURAR" },
        "hint": "La escala de interfaz y el audio se añadirán cuando existan assets definitivos.",
        "rejectionMessage": { "title": "Cambio no aplicado", "body": "El valor seleccionado no se pudo aplicar." }
      },
      "squad": {
        "header": { "eyebrow": "GESTIÓN DE PLANTILLA", "title": "PLANTILLA", "summary": "" },
        "summaryOwnedFormat": "Tu club · {0} · {1} jugadores",
        "summaryBrowseFormat": "Explorando · {0} · {1} jugadores",
        "actionOpenMarket": "ABRIR MERCADO", "actionOpenMarketTooltip": "Ir al mercado de fichajes",
        "empty": "Esta plantilla aún no tiene jugadores.",
        "columns": { "player": "JUGADOR", "nation": "NAC.", "age": "EDAD", "tec": "TEC", "for": "FOR", "fis": "FIS", "men": "MEN", "val": "MED", "action": "ACCIÓN" },
        "actions": { "inspect": "INSPECCIONAR", "inspectTooltip": "Abrir la ficha del jugador" }
      },
      "market": {
        "header": { "eyebrow": "MERCADO DE FICHAJES", "title": "MERCADO", "summary": "" },
        "summaryFormat": "{0} · {1} candidatos · {2} con precio",
        "noticeIdle": "Selecciona un candidato.",
        "noticeAcceptedFormat": "FICHAJE CERRADO · {0} · {1}",
        "noticeRejectedFormat": "OFERTA RECHAZADA · {0}",
        "empty": "No hay jugadores disponibles.",
        "columns": { "player": "JUGADOR", "sellingClub": "CLUB VENDEDOR", "age": "EDAD", "tec": "TEC", "val": "MED", "price": "PRECIO", "action": "ACCIÓN" },
        "actions": { "sign": "FICHAR", "signTooltipFormat": "Presentar {0} por {1}" },
        "reasons": { "budget": "caja o presupuesto insuficiente", "price": "la oferta no alcanza el precio solicitado", "unavailable": "el jugador ya no está disponible", "stale": "la operación cambió", "finance": "las finanzas no están disponibles", "generic": "la oferta fue rechazada" }
      },
      "history": {
        "header": { "eyebrow": "ARCHIVO", "title": "HISTORIAL", "summary": "" },
        "summaryFormat": "{0} partidos · {1} goles · {2} competiciones",
        "empty": "Sin partidos jugados todavía.",
        "filterEmpty": "Ningún partido coincide con los filtros.",
        "filters": { "competitionLabel": "Competición:", "clubLabel": "Club:", "orderLabel": "Orden:", "allOption": "(todas)", "orderOptions": { "date": "Fecha", "competition": "Competición", "goals": "Goles totales" } },
        "aggregate": { "title": "CLASIFICACIÓN HISTÓRICA", "empty": "Sin datos." },
        "columns": { "when": "CUÁNDO", "competition": "COMPETICIÓN", "home": "LOCAL", "score": "RES.", "away": "VISITANTE", "seed": "SEED", "action": "ACCIÓN" },
        "actions": { "view": "REPETIR", "viewTooltip": "Abrir la repetición del partido" }
      },
      "competitions": {
        "header": { "eyebrow": "CALENDARIO", "title": "COMPETICIONES", "summary": "" },
        "summaryFormat": "{0} ligas · {1} copas · temporada de {2}",
        "empty": "Este club aún no está inscrito en ninguna competición.",
        "readOnly": { "title": "VISTA DE SÓLO LECTURA", "body": "Sólo puedes jugar con tu club." },
        "entry": { "eyebrow": "COMPETICIÓN", "summaryFormat": "Frecuencia: {0} · {1} rivales", "rivalsPrefix": "Rivales:" },
        "actions": { "playNext": "JUGAR PRÓXIMO RIVAL", "playNextTooltip": "Jugar el próximo partido", "playSeasonFormat": "Jugar temporada completa ({0} partidos)" }
      },
      "world": {
        "header": { "eyebrow": "MUNDIAL", "title": "COMPETICIONES DEL MUNDO", "summary": "" },
        "summaryFormat": "{0} competiciones · {1} clubes inscritos",
        "empty": "No hay competiciones cargadas en este mundo.",
        "columns": { "name": "COMPETICIÓN", "format": "FORMATO", "frequency": "FRECUENCIA", "rivals": "RIVALES", "action": "ACCIÓN" },
        "actions": { "inspect": "INSPECCIONAR", "inspectTooltip": "Abrir el detalle" }
      },
      "picker": {
        "header": { "eyebrow": "EXPLORAR", "title": "SELECCIONA UN CLUB", "summary": "" },
        "summaryFormat": "{0} clubes disponibles en el mundo actual",
        "ownedBadge": "TU CLUB",
        "empty": "No hay clubes disponibles.",
        "columns": { "club": "CLUB", "country": "PAÍS", "founded": "FUND.", "prestige": "PRES.", "fanbase": "AFIC.", "squad": "PLANT.", "action": "ACCIÓN" },
        "actions": { "choose": "ELEGIR", "chooseTooltip": "Explorar este club" }
      }
    }
    """;

    private static LocaleLoader BuildLoader(params (string id, string content)[] files)
    {
        Dictionary<string, string> map = new();
        foreach ((string id, string content) in files)
        {
            map[$"res://locale/langs/{id}.json"] = content;
        }

        return new LocaleLoader(new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/",
            builtInPrefix: "res://locale/langs/built/"));
    }

    private static LocaleLoader BuildBuiltInLoader(params (string id, string content)[] files)
    {
        Dictionary<string, string> map = new();
        foreach ((string id, string content) in files)
        {
            map[$"res://locale/langs/built/{id}.json"] = content;
        }

        return new LocaleLoader(new InMemoryLocaleFileSource(
            map,
            rootFolder: "res://locale/langs/built/",
            builtInPrefix: "res://locale/langs/built/"));
    }

    [Fact]
    public void AValidSpanishFileLoadsWithEveryPropertyPopulated()
    {
        LocaleLoader loader = BuildBuiltInLoader(("es", CanonicalSpanishJson));

        LocaleLoadResult result = loader.Load("es");

        LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
        Assert.Equal("es", ok.File.Lang);
        Assert.Equal("Español", ok.File.Name);
        Assert.Equal("NUEVA CARRERA", ok.File.Menu.Actions.NewCareer);
        Assert.Equal(LocaleSource.BuiltIn, ok.Source);
    }

    [Fact]
    public void AMissingFileReturnsANotFoundInvalid()
    {
        LocaleLoader loader = BuildLoader();

        LocaleLoadResult result = loader.Load("es");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.NotFound, invalid.Failure);
        Assert.Equal("es", invalid.Id);
        Assert.Single(invalid.Diagnostics);
    }

    [Fact]
    public void AnEmptyIdReturnsAnInvalid()
    {
        LocaleLoader loader = BuildLoader();

        LocaleLoadResult result = loader.Load("");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.NotFound, invalid.Failure);
        Assert.Empty(invalid.Path);
    }

    [Fact]
    public void AMalformedJsonReturnsAMalformedJsonFailure()
    {
        LocaleLoader loader = BuildLoader(("fr", "{ this is not valid json"));

        LocaleLoadResult result = loader.Load("fr");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.MalformedJson, invalid.Failure);
        Assert.Single(invalid.Diagnostics);
        Assert.Equal("root", invalid.Diagnostics[0].Path);
    }

    [Fact]
    public void ALangThatDoesNotMatchTheIdReturnsInvalidShape()
    {
        const string mismatched = """
        {
          "lang": "es",
          "icon": "flags/es.svg",
          "name": "Español"
        }
        """;

        LocaleLoader loader = BuildLoader(("fr", mismatched));

        LocaleLoadResult result = loader.Load("fr");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.InvalidShape, invalid.Failure);
        Assert.Contains(invalid.Diagnostics, d => d.Path == "lang");
    }

    [Fact]
    public void AnEmptyIconIsReportedAsMissing()
    {
        const string noIcon = """
        {
          "lang": "es",
          "icon": "",
          "name": "Español"
        }
        """;

        LocaleLoader loader = BuildLoader(("es", noIcon));

        LocaleLoadResult result = loader.Load("es");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.InvalidShape, invalid.Failure);
        Assert.Contains(invalid.Diagnostics, d => d.Path == "icon");
    }

    [Fact]
    public void AMissingSectionIsReportedWithItsPath()
    {
        // No "market" key in the JSON. The loader reports it as a
        // missing path so the player sees "market" in the error panel.
        const string noMarket = """
        {
          "lang": "es",
          "icon": "flags/es.svg",
          "name": "Español",
          "menu": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "actions": { "newCareer": "a", "continue": "a", "load": "a", "dataEditor": "a", "options": "a", "exit": "a" } },
          "options": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "fullscreen": "a", "reducedMotion": "a", "denseTables": "a", "themeHeader": "a", "themeHint": "a", "languageHeader": "a", "languageHint": "a", "actions": { "saveAndClose": "a", "revert": "a" }, "hint": "a", "rejectionMessage": { "title": "a", "body": "a" } },
          "squad": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryOwnedFormat": "a", "summaryBrowseFormat": "a", "actionOpenMarket": "a", "actionOpenMarketTooltip": "a", "empty": "a", "columns": { "player": "a", "nation": "a", "age": "a", "tec": "a", "for": "a", "fis": "a", "men": "a", "val": "a", "action": "a" }, "actions": { "inspect": "a", "inspectTooltip": "a" } },
          "history": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "filterEmpty": "a", "filters": { "competitionLabel": "a", "clubLabel": "a", "orderLabel": "a", "allOption": "a", "orderOptions": { "date": "a", "competition": "a", "goals": "a" } }, "aggregate": { "title": "a", "empty": "a" }, "columns": { "when": "a", "competition": "a", "home": "a", "score": "a", "away": "a", "seed": "a", "action": "a" }, "actions": { "view": "a", "viewTooltip": "a" } },
          "competitions": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "readOnly": { "title": "a", "body": "a" }, "entry": { "eyebrow": "a", "summaryFormat": "a", "rivalsPrefix": "a" }, "actions": { "playNext": "a", "playNextTooltip": "a", "playSeasonFormat": "a" } },
          "world": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "empty": "a", "columns": { "name": "a", "format": "a", "frequency": "a", "rivals": "a", "action": "a" }, "actions": { "inspect": "a", "inspectTooltip": "a" } },
          "picker": { "header": { "eyebrow": "e", "title": "t", "summary": "s" }, "summaryFormat": "a", "ownedBadge": "a", "empty": "a", "columns": { "club": "a", "country": "a", "founded": "a", "prestige": "a", "fanbase": "a", "squad": "a", "action": "a" }, "actions": { "choose": "a", "chooseTooltip": "a" } }
        }
        """;

        LocaleLoader loader = BuildLoader(("es", noMarket));

        LocaleLoadResult result = loader.Load("es");

        LocaleLoadResult.Invalid invalid = Assert.IsType<LocaleLoadResult.Invalid>(result);
        Assert.Equal(LocaleLoadFailure.InvalidShape, invalid.Failure);
        Assert.Contains(invalid.Diagnostics, d => d.Path == "market" && d.Kind == LocaleDiagnosticKind.Missing);
    }

    [Fact]
    public void APeekReturnsMetadataForAValidFile()
    {
        LocaleLoader loader = BuildLoader(("es", CanonicalSpanishJson));

        LocaleMetadata? meta = loader.Peek("es");

        Assert.NotNull(meta);
        Assert.Equal("es", meta!.Id);
        Assert.Equal("Español", meta.Name);
        Assert.Equal("flags/es.svg", meta.Icon);
    }

    [Fact]
    public void APeekReturnsNullForAMissingFile()
    {
        LocaleLoader loader = BuildLoader();

        Assert.Null(loader.Peek("es"));
    }

    [Fact]
    public void APeekReturnsNullWhenMetadataIsIncomplete()
    {
        const string incomplete = """
        {
          "lang": "es",
          "icon": "flags/es.svg"
        }
        """;

        LocaleLoader loader = BuildLoader(("es", incomplete));

        Assert.Null(loader.Peek("es"));
    }

    [Fact]
    public void APeekDoesNotThrowOnMalformedJson()
    {
        LocaleLoader loader = BuildLoader(("fr", "definitely not json {"));

        Assert.Null(loader.Peek("fr"));
    }

    [Fact]
    public void AnExternalFileIsTaggedAsExternal()
    {
        // Same content, but the file lives under the root folder and
        // the built-in prefix points at a sibling folder, so the loader
        // sees it as External — the registry uses this to dedupe.
        LocaleLoader loader = BuildLoader(("pt", CanonicalSpanishJson.Replace("\"es\"", "\"pt\"").Replace("Español", "Português")));

        LocaleLoadResult result = loader.Load("pt");

        LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
        Assert.Equal(LocaleSource.External, ok.Source);
    }
}
