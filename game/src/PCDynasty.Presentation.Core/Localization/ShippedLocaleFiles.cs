using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The canonical <see cref="LocaleFile"/> records the generator
/// ships with the game. The strings live in C# so the JSON files
/// under <c>res://locale/langs/built/</c> are generated artefacts
/// that cannot drift from the source of truth: a missing property
/// here fails the build before the generator even runs.
/// </summary>
public static class ShippedLocaleFiles
{
    /// <summary>Every record the generator ships, ordered by id for deterministic output.</summary>
    public static IReadOnlyList<(string Id, LocaleFile File)> All { get; } = new[]
    {
        ("en", BuildEnglish()),
        ("es", BuildSpanish()),
    };

    /// <summary>The English (fallback) record.</summary>
    public static LocaleFile BuildEnglish() => new(
        Lang: "en",
        Icon: "flags/en.svg",
        Name: "English",
        Menu: new MenuSection(
            Header: new HeaderSection(
                Eyebrow: "PC DYNASTY · SPORTING DIRECTOR",
                Title: "YOUR CAREER STARTS HERE",
                Summary: "Run the squad, the tactics, the market and the books."),
            Actions: new MenuActionsSection(
                NewCareer: "NEW CAREER",
                Continue: "CONTINUE",
                Load: "LOAD CAREER",
                DataEditor: "DATA EDITOR",
                Options: "OPTIONS",
                Exit: "EXIT")),
        Options: new OptionsSection(
            Header: new HeaderSection("SETTINGS", "PRESENTATION OPTIONS", ""),
            Fullscreen: "Fullscreen",
            ReducedMotion: "Reduced motion and immediate navigation",
            DenseTables: "High-density compact tables",
            ThemeHeader: "THEME",
            ThemeHint: "Swap the interface palette.",
            LanguageHeader: "LANGUAGE",
            LanguageHint: "Game interface language; content does not change.",
            DisplayHeader: "GRAPHICS",
            DataHeader: "DATA",
            Audio: new OptionsAudioSection(
                Header: "AUDIO",
                Hint: "Music and effects, each with its own level.",
                Music: "Music",
                MusicVolume: "Music level",
                Effects: "Sound effects",
                EffectsVolume: "Effects level"),
            Video: new OptionsVideoSection(
                Header: "VIDEO",
                Hint: "Clips played at the moments below.",
                Enabled: "Play video clips",
                Intro: "Opening sequence",
                MatchIntro: "Run-up to a match",
                Celebrations: "Trophy and title celebrations"),
            Actions: new OptionsActionsSection(SaveAndClose: "SAVE AND CLOSE", Revert: "REVERT"),
            Hint: "Interface scaling lands once the assets are final.",
            RejectionMessage: new OptionsRejectionMessage(
                Title: "Change not applied",
                Body: "The selected value could not be applied.")),
        Squad: new SquadSection(
            Header: new HeaderSection("SQUAD MANAGEMENT", "SQUAD", ""),
            SummaryOwnedFormat: "Your club · {0} · {1} players",
            SummaryBrowseFormat: "Browsing · {0} · {1} players",
            ActionOpenMarket: "OPEN MARKET",
            ActionOpenMarketTooltip: "Go to the transfer market",
            Empty: "This squad has no players yet.",
            Columns: new SquadColumnsSection(
                Player: "PLAYER", Nation: "NAT.", Age: "AGE", Tec: "TEC", For: "FOR",
                Fis: "PHY", Men: "MEN", Val: "RAT", Action: "ACTION"),
            Actions: new SquadActionsSection(Inspect: "INSPECT", InspectTooltip: "Open the player dossier")),
        Market: new MarketSection(
            Header: new HeaderSection("TRANSFER MARKET", "MARKET", ""),
            SummaryFormat: "{0} · {1} candidates · {2} with a price",
            NoticeIdle: "Select a candidate.",
            NoticeAcceptedFormat: "DEAL CLOSED · {0} · {1}",
            NoticeRejectedFormat: "OFFER REJECTED · {0}",
            Empty: "No players available.",
            Columns: new MarketColumnsSection(
                Player: "PLAYER", SellingClub: "SELLING CLUB", Age: "AGE", Tec: "TEC",
                Val: "RAT", Price: "PRICE", Action: "ACTION"),
            Actions: new MarketActionsSection(
                Sign: "SIGN",
                SignTooltipFormat: "Submit {0} for {1}"),
            Reasons: new MarketReasonsSection(
                Budget: "cash or budget insufficient",
                Price: "the offer does not meet the asking price",
                Unavailable: "the player is no longer available",
                Stale: "the deal changed",
                Finance: "the finances are not available",
                Generic: "the offer was rejected")),
        History: new HistorySection(
            Header: new HeaderSection("ARCHIVE", "HISTORY", ""),
            SummaryFormat: "{0} matches · {1} goals · {2} competitions",
            Empty: "No matches played yet.",
            FilterEmpty: "No matches match the filters.",
            Filters: new HistoryFiltersSection(
                CompetitionLabel: "Competition:", ClubLabel: "Club:", OrderLabel: "Order:",
                AllOption: "(all)",
                OrderOptions: new HistoryOrderOptionsSection(
                    Date: "Date", Competition: "Competition", Goals: "Total goals")),
            Aggregate: new HistoryAggregateSection(Title: "ALL-TIME TABLE", Empty: "No data."),
            Columns: new HistoryColumnsSection(
                When: "WHEN", Competition: "COMPETITION", Home: "HOME", Score: "RES",
                Away: "AWAY", Seed: "SEED", Action: "ACTION"),
            Actions: new HistoryActionsSection(View: "REPLAY", ViewTooltip: "Open the match replay")),
        Competitions: new CompetitionsSection(
            Header: new HeaderSection("CALENDAR", "COMPETITIONS", ""),
            SummaryFormat: "{0} leagues · {1} cups · {2} season",
            Empty: "This club is not yet entered in any competition.",
            ReadOnly: new CompetitionsReadOnlySection(
                Title: "READ-ONLY VIEW",
                Body: "You can only play with your own club."),
            Entry: new CompetitionEntrySection(
                Eyebrow: "COMPETITION",
                SummaryFormat: "Frequency: {0} · {1} rivals",
                RivalsPrefix: "Rivals:"),
            Actions: new CompetitionActionsSection(
                PlayNext: "PLAY NEXT RIVAL",
                PlayNextTooltip: "Play the next match",
                PlaySeasonFormat: "Play full season ({0} matches)")),
        World: new WorldSection(
            Header: new HeaderSection("WORLD", "WORLD COMPETITIONS", ""),
            SummaryFormat: "{0} competitions · {1} clubs entered",
            Empty: "No competitions are loaded in this world.",
            Columns: new WorldColumnsSection(
                Name: "COMPETITION", Format: "FORMAT", Frequency: "FREQUENCY",
                Rivals: "RIVALS", Action: "ACTION"),
            Actions: new WorldActionsSection(Inspect: "INSPECT", InspectTooltip: "Open the detail")),
        Picker: new PickerSection(
            Header: new HeaderSection("BROWSE", "PICK A CLUB", ""),
            SummaryFormat: "{0} clubs available in the current world",
            OwnedBadge: "YOUR CLUB",
            Empty: "No clubs available.",
            Columns: new PickerColumnsSection(
                Club: "CLUB", Country: "COUNTRY", Founded: "FD.", Prestige: "PRE.",
                Fanbase: "FAN", Squad: "SQUAD", Action: "ACTION"),
            Actions: new PickerActionsSection(Choose: "CHOOSE", ChooseTooltip: "Explore this club")));

    /// <summary>The Spanish record (the canonical fixture used by the unit tests).</summary>
    public static LocaleFile BuildSpanish() => new(
        Lang: "es",
        Icon: "flags/es.svg",
        Name: "Español",
        Menu: new MenuSection(
            Header: new HeaderSection(
                Eyebrow: "PC DYNASTY · DIRECCIÓN DEPORTIVA",
                Title: "TU CARRERA EMPIEZA AQUÍ",
                Summary: "Gestiona plantilla, táctica, mercado y patrimonio."),
            Actions: new MenuActionsSection(
                NewCareer: "NUEVA CARRERA", Continue: "CONTINUAR", Load: "CARGAR CARRERA",
                DataEditor: "EDITOR DE DATOS", Options: "OPCIONES", Exit: "SALIR")),
        Options: new OptionsSection(
            Header: new HeaderSection("CONFIGURACIÓN", "OPCIONES DE PRESENTACIÓN", ""),
            Fullscreen: "Pantalla completa",
            ReducedMotion: "Transiciones reducidas y navegación inmediata",
            DenseTables: "Tablas compactas de alta densidad",
            ThemeHeader: "TEMA",
            ThemeHint: "Cambia la paleta de la interfaz.",
            LanguageHeader: "IDIOMA",
            LanguageHint: "La interfaz del juego; el contenido no cambia.",
            DisplayHeader: "GRÁFICOS",
            DataHeader: "DATOS",
            Audio: new OptionsAudioSection(
                Header: "AUDIO",
                Hint: "Música y efectos, cada uno con su nivel.",
                Music: "Música",
                MusicVolume: "Nivel de música",
                Effects: "Efectos de sonido",
                EffectsVolume: "Nivel de efectos"),
            Video: new OptionsVideoSection(
                Header: "VÍDEO",
                Hint: "Secuencias que se reproducen en los momentos de abajo.",
                Enabled: "Reproducir vídeos",
                Intro: "Secuencia de apertura",
                MatchIntro: "Antesala del partido",
                Celebrations: "Celebraciones de trofeo y título"),
            Actions: new OptionsActionsSection(SaveAndClose: "GUARDAR Y CERRAR", Revert: "RESTAURAR"),
            Hint: "La escala de interfaz se añadirá cuando existan assets definitivos.",
            RejectionMessage: new OptionsRejectionMessage(
                Title: "Cambio no aplicado",
                Body: "El valor seleccionado no se pudo aplicar.")),
        Squad: new SquadSection(
            Header: new HeaderSection("GESTIÓN DE PLANTILLA", "PLANTILLA", ""),
            SummaryOwnedFormat: "Tu club · {0} · {1} jugadores",
            SummaryBrowseFormat: "Explorando · {0} · {1} jugadores",
            ActionOpenMarket: "ABRIR MERCADO",
            ActionOpenMarketTooltip: "Ir al mercado de fichajes",
            Empty: "Esta plantilla aún no tiene jugadores.",
            Columns: new SquadColumnsSection(
                Player: "JUGADOR", Nation: "NAC.", Age: "EDAD", Tec: "TEC", For: "FOR",
                Fis: "FIS", Men: "MEN", Val: "MED", Action: "ACCIÓN"),
            Actions: new SquadActionsSection(Inspect: "INSPECCIONAR", InspectTooltip: "Abrir la ficha del jugador")),
        Market: new MarketSection(
            Header: new HeaderSection("MERCADO DE FICHAJES", "MERCADO", ""),
            SummaryFormat: "{0} · {1} candidatos · {2} con precio",
            NoticeIdle: "Selecciona un candidato.",
            NoticeAcceptedFormat: "FICHAJE CERRADO · {0} · {1}",
            NoticeRejectedFormat: "OFERTA RECHAZADA · {0}",
            Empty: "No hay jugadores disponibles.",
            Columns: new MarketColumnsSection(
                Player: "JUGADOR", SellingClub: "CLUB VENDEDOR", Age: "EDAD", Tec: "TEC",
                Val: "MED", Price: "PRECIO", Action: "ACCIÓN"),
            Actions: new MarketActionsSection(
                Sign: "FICHAR",
                SignTooltipFormat: "Presentar {0} por {1}"),
            Reasons: new MarketReasonsSection(
                Budget: "caja o presupuesto insuficiente",
                Price: "la oferta no alcanza el precio solicitado",
                Unavailable: "el jugador ya no está disponible",
                Stale: "la operación cambió",
                Finance: "las finanzas no están disponibles",
                Generic: "la oferta fue rechazada")),
        History: new HistorySection(
            Header: new HeaderSection("ARCHIVO", "HISTORIAL", ""),
            SummaryFormat: "{0} partidos · {1} goles · {2} competiciones",
            Empty: "Sin partidos jugados todavía.",
            FilterEmpty: "Ningún partido coincide con los filtros.",
            Filters: new HistoryFiltersSection(
                CompetitionLabel: "Competición:", ClubLabel: "Club:", OrderLabel: "Orden:",
                AllOption: "(todas)",
                OrderOptions: new HistoryOrderOptionsSection(
                    Date: "Fecha", Competition: "Competición", Goals: "Goles totales")),
            Aggregate: new HistoryAggregateSection(Title: "CLASIFICACIÓN HISTÓRICA", Empty: "Sin datos."),
            Columns: new HistoryColumnsSection(
                When: "CUÁNDO", Competition: "COMPETICIÓN", Home: "LOCAL", Score: "RES",
                Away: "VISITANTE", Seed: "SEED", Action: "ACCIÓN"),
            Actions: new HistoryActionsSection(View: "REPETIR", ViewTooltip: "Abrir la repetición del partido")),
        Competitions: new CompetitionsSection(
            Header: new HeaderSection("CALENDARIO", "COMPETICIONES", ""),
            SummaryFormat: "{0} ligas · {1} copas · temporada de {2}",
            Empty: "Este club aún no está inscrito en ninguna competición.",
            ReadOnly: new CompetitionsReadOnlySection(
                Title: "VISTA DE SÓLO LECTURA",
                Body: "Sólo puedes jugar con tu club."),
            Entry: new CompetitionEntrySection(
                Eyebrow: "COMPETICIÓN",
                SummaryFormat: "Frecuencia: {0} · {1} rivales",
                RivalsPrefix: "Rivales:"),
            Actions: new CompetitionActionsSection(
                PlayNext: "JUGAR PRÓXIMO RIVAL",
                PlayNextTooltip: "Jugar el próximo partido",
                PlaySeasonFormat: "Jugar temporada completa ({0} partidos)")),
        World: new WorldSection(
            Header: new HeaderSection("MUNDIAL", "COMPETICIONES DEL MUNDO", ""),
            SummaryFormat: "{0} competiciones · {1} clubes inscritos",
            Empty: "No hay competiciones cargadas en este mundo.",
            Columns: new WorldColumnsSection(
                Name: "COMPETICIÓN", Format: "FORMATO", Frequency: "FRECUENCIA",
                Rivals: "RIVALES", Action: "ACCIÓN"),
            Actions: new WorldActionsSection(Inspect: "INSPECCIONAR", InspectTooltip: "Abrir el detalle")),
        Picker: new PickerSection(
            Header: new HeaderSection("EXPLORAR", "SELECCIONA UN CLUB", ""),
            SummaryFormat: "{0} clubes disponibles en el mundo actual",
            OwnedBadge: "TU CLUB",
            Empty: "No hay clubes disponibles.",
            Columns: new PickerColumnsSection(
                Club: "CLUB", Country: "PAÍS", Founded: "FUND.", Prestige: "PRES.",
                Fanbase: "AFIC.", Squad: "PLANT.", Action: "ACCIÓN"),
            Actions: new PickerActionsSection(Choose: "ELEGIR", ChooseTooltip: "Explorar este club")));
}
