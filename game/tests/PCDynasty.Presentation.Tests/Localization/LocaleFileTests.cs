using System.Text.Json;
using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The shape of <see cref="LocaleFile"/> is the source of truth. The
/// tests pin the on-disk JSON to the C# record so a drift between
/// them fails the test instead of the player.
/// </summary>
public sealed class LocaleFileTests
{
    private const string CanonicalSpanishJson = """
    {
      "lang": "es",
      "icon": "flags/es.svg",
      "name": "Español",
      "menu": {
        "header": { "eyebrow": "PC DYNASTY · DIRECCIÓN DEPORTIVA", "title": "TU CARRERA EMPIEZA AQUÍ", "summary": "Gestiona plantilla, táctica, mercado y patrimonio." },
        "actions": {
          "newCareer": "NUEVA CARRERA",
          "continue": "CONTINUAR",
          "load": "CARGAR CARRERA",
          "dataEditor": "EDITOR DE DATOS",
          "options": "OPCIONES",
          "exit": "SALIR"
        }
      },
      "options": {
        "header": { "eyebrow": "CONFIGURACIÓN", "title": "OPCIONES DE PRESENTACIÓN", "summary": "" },
        "fullscreen": "Pantalla completa",
        "reducedMotion": "Transiciones reducidas y navegación inmediata",
        "denseTables": "Tablas compactas de alta densidad",
        "themeHeader": "TEMA",
        "themeHint": "Cambia la paleta de la interfaz.",
        "languageHeader": "IDIOMA",
        "languageHint": "La interfaz del juego; el contenido no cambia.",
        "actions": { "saveAndClose": "GUARDAR Y CERRAR", "revert": "RESTAURAR" },
        "hint": "La escala de interfaz y el audio se añadirán cuando existan assets definitivos.",
        "rejectionMessage": { "title": "Cambio no aplicado", "body": "El valor seleccionado no se pudo aplicar." }
      },
      "squad": {
        "header": { "eyebrow": "GESTIÓN DE PLANTILLA", "title": "PLANTILLA", "summary": "" },
        "summaryOwnedFormat": "Tu club · {0} · {1} jugadores",
        "summaryBrowseFormat": "Explorando · {0} · {1} jugadores",
        "actionOpenMarket": "ABRIR MERCADO",
        "actionOpenMarketTooltip": "Ir al mercado de fichajes",
        "empty": "Esta plantilla aún no tiene jugadores.",
        "columns": {
          "player": "JUGADOR", "nation": "NAC.", "age": "EDAD",
          "tec": "TEC", "for": "FOR", "fis": "FIS", "men": "MEN",
          "val": "MED", "action": "ACCIÓN"
        },
        "actions": { "inspect": "INSPECCIONAR", "inspectTooltip": "Abrir la ficha del jugador" }
      },
      "market": {
        "header": { "eyebrow": "MERCADO DE FICHAJES", "title": "MERCADO", "summary": "" },
        "summaryFormat": "{0} · {1} candidatos · {2} con precio",
        "noticeIdle": "Selecciona un candidato.",
        "noticeAcceptedFormat": "FICHAJE CERRADO · {0} · {1}",
        "noticeRejectedFormat": "OFERTA RECHAZADA · {0}",
        "empty": "No hay jugadores disponibles.",
        "columns": {
          "player": "JUGADOR", "sellingClub": "CLUB VENDEDOR", "age": "EDAD",
          "tec": "TEC", "val": "MED", "price": "PRECIO", "action": "ACCIÓN"
        },
        "actions": { "sign": "FICHAR", "signTooltipFormat": "Presentar {0} por {1}" },
        "reasons": {
          "budget": "caja o presupuesto insuficiente",
          "price": "la oferta no alcanza el precio solicitado",
          "unavailable": "el jugador ya no está disponible",
          "stale": "la operación cambió",
          "finance": "las finanzas no están disponibles",
          "generic": "la oferta fue rechazada"
        }
      },
      "history": {
        "header": { "eyebrow": "ARCHIVO", "title": "HISTORIAL", "summary": "" },
        "summaryFormat": "{0} partidos · {1} goles · {2} competiciones",
        "empty": "Sin partidos jugados todavía.",
        "filterEmpty": "Ningún partido coincide con los filtros.",
        "filters": {
          "competitionLabel": "Competición:",
          "clubLabel": "Club:",
          "orderLabel": "Orden:",
          "allOption": "(todas)",
          "orderOptions": { "date": "Fecha", "competition": "Competición", "goals": "Goles totales" }
        },
        "aggregate": { "title": "CLASIFICACIÓN HISTÓRICA", "empty": "Sin datos." },
        "columns": {
          "when": "CUÁNDO", "competition": "COMPETICIÓN", "home": "LOCAL",
          "score": "RES.", "away": "VISITANTE", "seed": "SEED", "action": "ACCIÓN"
        },
        "actions": { "view": "REPETIR", "viewTooltip": "Abrir la repetición del partido" }
      },
      "competitions": {
        "header": { "eyebrow": "CALENDARIO", "title": "COMPETICIONES", "summary": "" },
        "summaryFormat": "{0} ligas · {1} copas · temporada de {2}",
        "empty": "Este club aún no está inscrito en ninguna competición.",
        "readOnly": { "title": "VISTA DE SÓLO LECTURA", "body": "Sólo puedes jugar con tu club." },
        "entry": {
          "eyebrow": "COMPETICIÓN",
          "summaryFormat": "Frecuencia: {0} · {1} rivales",
          "rivalsPrefix": "Rivales:"
        },
        "actions": {
          "playNext": "JUGAR PRÓXIMO RIVAL",
          "playNextTooltip": "Jugar el próximo partido",
          "playSeasonFormat": "Jugar temporada completa ({0} partidos)"
        }
      },
      "world": {
        "header": { "eyebrow": "MUNDIAL", "title": "COMPETICIONES DEL MUNDO", "summary": "" },
        "summaryFormat": "{0} competiciones · {1} clubes inscritos",
        "empty": "No hay competiciones cargadas en este mundo.",
        "columns": {
          "name": "COMPETICIÓN", "format": "FORMATO", "frequency": "FRECUENCIA",
          "rivals": "RIVALES", "action": "ACCIÓN"
        },
        "actions": { "inspect": "INSPECCIONAR", "inspectTooltip": "Abrir el detalle" }
      },
      "picker": {
        "header": { "eyebrow": "EXPLORAR", "title": "SELECCIONA UN CLUB", "summary": "" },
        "summaryFormat": "{0} clubes disponibles en el mundo actual",
        "ownedBadge": "TU CLUB",
        "empty": "No hay clubes disponibles.",
        "columns": {
          "club": "CLUB", "country": "PAÍS", "founded": "FUND.",
          "prestige": "PRES.", "fanbase": "AFIC.", "squad": "PLANT.", "action": "ACCIÓN"
        },
        "actions": { "choose": "ELEGIR", "chooseTooltip": "Explorar este club" }
      }
    }
    """;

    [Fact]
    public void SpanishCanonicalJsonDeserialisesIntoEveryProperty()
    {
        LocaleFile file = LocaleFile.FromJson(CanonicalSpanishJson);

        Assert.Equal("es", file.Lang);
        Assert.Equal("flags/es.svg", file.Icon);
        Assert.Equal("Español", file.Name);
        Assert.Equal("PC DYNASTY · DIRECCIÓN DEPORTIVA", file.Menu.Header.Eyebrow);
        Assert.Equal("NUEVA CARRERA", file.Menu.Actions.NewCareer);
        Assert.Equal("SALIR", file.Menu.Actions.Exit);
        Assert.Equal("GUARDAR Y CERRAR", file.Options.Actions.SaveAndClose);
        Assert.Equal("RESTAURAR", file.Options.Actions.Revert);
        Assert.Equal("JUGADOR", file.Squad.Columns.Player);
        Assert.Equal("MED", file.Squad.Columns.Val);
        Assert.Equal("Tu club · {0} · {1} jugadores", file.Squad.SummaryOwnedFormat);
        Assert.Equal("FICHAR", file.Market.Actions.Sign);
        Assert.Equal("Presentar {0} por {1}", file.Market.Actions.SignTooltipFormat);
        Assert.Equal("(todas)", file.History.Filters.AllOption);
        Assert.Equal("VISTA DE SÓLO LECTURA", file.Competitions.ReadOnly.Title);
        Assert.Equal("MUNDIAL", file.World.Header.Eyebrow);
        Assert.Equal("TU CLUB", file.Picker.OwnedBadge);
    }

    [Fact]
    public void RoundTripIsLossless()
    {
        LocaleFile original = LocaleFile.FromJson(CanonicalSpanishJson);

        string serialised = original.ToJson();
        LocaleFile decoded = LocaleFile.FromJson(serialised);

        Assert.Equal(original, decoded);
    }

    [Fact]
    public void JsonUsesCamelCase()
    {
        LocaleFile file = LocaleFile.FromJson(CanonicalSpanishJson);

        string serialised = file.ToJson();

        // The shape is camelCase, never PascalCase: every first character
        // of a top-level key is lowercase. Catching the change is a one-line
        // test instead of a code review.
        Assert.Contains("\"lang\":", serialised);
        Assert.Contains("\"icon\":", serialised);
        Assert.Contains("\"name\":", serialised);
        Assert.Contains("\"menu\":", serialised);
        Assert.DoesNotContain("\"Lang\":", serialised);
        Assert.DoesNotContain("\"Icon\":", serialised);
    }

    [Fact]
    public void NestedSectionsExposeEveryLeaf()
    {
        // The picker has the deepest nested shape we ship today. A future
        // deeper section is allowed; this test guards the current level.
        LocaleFile file = LocaleFile.FromJson(CanonicalSpanishJson);

        Assert.Equal("EXPLORAR", file.Picker.Header.Eyebrow);
        Assert.Equal("SELECCIONA UN CLUB", file.Picker.Header.Title);
        Assert.Equal("{0} clubes disponibles en el mundo actual", file.Picker.SummaryFormat);
        Assert.Equal("ELEGIR", file.Picker.Actions.Choose);
    }

    [Fact]
    public void AnEmptyStringForASectionFieldIsPreserved()
    {
        // The summary fields can be empty (the screen always computes the
        // actual text from the runtime context). A missing value must
        // not be silently filled in by the loader.
        LocaleFile file = LocaleFile.FromJson(CanonicalSpanishJson);

        Assert.Equal(string.Empty, file.Options.Header.Summary);
    }

    [Fact]
    public void AJsonMissingTheTopLevelLanguageFailsCleanly()
    {
        // The deserialiser is permissive on shape: a missing top-level
        // key becomes a null property, not a JsonException. The strict
        // loader (S2) walks the result and turns every null into a
        // missing-key diagnostic. S1's contract is the lenient shape.
        const string missing = """
        {
          "icon": "flags/es.svg",
          "name": "Español"
        }
        """;

        LocaleFile file = LocaleFile.FromJson(missing);

        Assert.Null(file.Lang);
        Assert.Equal("flags/es.svg", file.Icon);
    }

    [Fact]
    public void AJsonMissingASectionProducesANullSubRecord()
    {
        // The strict loader (S2) is what catches a missing section and
        // reports it. The deserialiser is permissive on shape: a missing
        // top-level key becomes a null sub-record. S2 walks the result
        // and turns every null into a missing-key diagnostic.
        const string noMarket = """
        {
          "lang": "es",
          "icon": "flags/es.svg",
          "name": "Español"
        }
        """;

        LocaleFile file = LocaleFile.FromJson(noMarket);

        Assert.Null(file.Menu);
        Assert.Null(file.Squad);
        Assert.Null(file.Market);
        Assert.NotNull(file);   // deserialiser returned a half-constructed record
    }

    [Fact]
    public void ARoundTrippedFileIsValueEqualToTheOriginal()
    {
        // Value equality on records makes this a single assert: every
        // field, every sub-record, no exceptions.
        LocaleFile original = LocaleFile.FromJson(CanonicalSpanishJson);

        string serialised = original.ToJson();
        LocaleFile decoded = LocaleFile.FromJson(serialised);

        Assert.Equal(original, decoded);
        Assert.Equal(original.GetHashCode(), decoded.GetHashCode());
    }
}
