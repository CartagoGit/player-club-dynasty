# Mapa de equivalencias — el clásico contra lo que tenemos

Tabla de trabajo: qué pantalla nuestra corresponde a cuál del clásico, qué falta y
dónde está el código. Se actualiza cada vez que se cierra una propuesta de interfaz.

Leyenda de estado: `✔` cumple la gramática · `~` existe pero no cumple · `✖` no existe

| Clásico ([`03`](../pcfutbol/03-mapa-de-pantallas.md)) | Nuestra pantalla | Código | Estado | Hueco principal |
|---|---|---|---|---|
| A · Menú principal (mosaicos) | Menú principal | `Scenes/Main.tscn` → `MainMenu`, `Scripts/Shell/MainMenuController.cs` | `~` | Posiciones absolutas; sin rejilla de destinos; sin barra de mando |
| B · Selección de partida | Nueva carrera → elegir club | `MainMenu/CareerSetup`, `MainMenuController.PopulateClubs` | `~` | Lista de botones en vez de rejilla de escudos + filtro por categoría |
| C · Pretemporada | — | — | `✖` | No existe fase de pretemporada ni amistosos |
| **D · Despacho del club** | Workspace (raíl lateral + lienzo) | `Scripts/Workspace/ManagerWorkspaceController.cs` | `~` | **Es un raíl de navegación, no un despacho.** Faltan las 4 áreas × 3 destinos visibles a la vez |
| E · Alineación | Plantilla / Selección de once | `Screens/SquadScreen.cs`, `Matches/LineupSelectionController.cs` | `~` | Sin raíl reactivo, sin ficha lateral, sin filas de sección, sin estado embebido |
| F · Tácticas | Táctica | `Screens/TacticsScreen.cs` | `~` | Sin campo manipulable ni áreas de actuación ni roles asignables |
| G · Ver rival | — | — | `✖` | No existe la comparación de onces |
| H · Ficha de jugador | Perfil de jugador | `Scenes/Players/PlayerProfile.tscn`, `Players/PlayerProfileController.cs` | `~` | No es un póster: faltan bloques de color, media grande, estrellas y demarcación |
| I · Comprar jugador | Mercado | `Screens/MarketScreen.cs` | `~` | Sin diálogo de oferta con términos de club y jugador; sin cláusulas |
| J · Vender jugador | — | — | `✖` | No se puede declarar transferible ni recibir ofertas |
| K · Caja | Finanzas | `Screens/FinancesScreen.cs` | `~` | Falta el espejo ingresos/gastos desglosado y el balance |
| L · Decisiones económicas | — | — | `✖` | Sin precios, crédito ni primas |
| M · Estadio | — | — | `✖` | Sin obra, sin aforo, sin simulación de presupuesto |
| N · Clasificación y resultados | Competiciones | `Screens/CompetitionsScreen.cs` | `~` | Sin tabla clasificatoria, sin tendencia, sin celdas pinchables |
| O · Goleadores | — | — | `✖` | — |
| P · Comienzo / fin de partido | Informe de partido | `Screens/MatchReportView.cs` | `~` | Falta la pantalla de comienzo con opciones |
| Q · **Opciones de configuración** | Ajustes | `Scripts/Shell/OptionsMenu.cs`, `LocaleSection.cs`, `ThemeSection.cs` | `~` | Sin placas agrupadas por tema; controles no pulsables; sin `CANCELAR`/`SEGUIR` |
| R · Celebración | — | `Screens/SeasonReportView.cs` (esbozo) | `✖` | — |
| — · Base de datos / Historia | Mundo · Historial | `Screens/WorldScreen.cs`, `Screens/HistoryScreen.cs` | `~` | Sin fichas de club, sin palmarés, sin comparativas, sin notas |

## Componentes: lo que la gramática pide y lo que hay

| Componente ([`04`](../pcfutbol/04-gramatica-visual.md) §3) | Hoy | Fichero |
|---|---|---|
| `Plate` | Parcial — esquinas redondeadas, sin bisel | `Theme/ClassicChrome.cs` |
| `CaptionBar` | **No existe** | — |
| `DataTable` | Parcial — `ListSection` + `ZebraRow`, sin cabecera fija ni celdas pinchables ni filas de sección | `Workspace/Components/` |
| `CommandButton` | **No existe** — se usa `Button` liso | — |
| `AreaButton` | Parcial — `ClassicChrome.RailEntry` | `Theme/ClassicChrome.cs` |
| `BreakdownWindow` | **No existe** | — |
| `Dialog` | **No existe** | — |
| `Stepper` | **No existe** | — |
| `StarRating` | **No existe** | — |
| `PitchDiagram` | Parcial en selección de once | `Matches/` |
| `DatePlate` | **No existe** | — |
| `CrestGrid` | **No existe** (hay `CrestPlaceholder` suelto) | `Theme/ClassicChrome.cs` |
| `EdgeRail` | **No existe** | — |
| `DestinationTile` | **No existe** | — |

## Defectos abiertos de interfaz

| # | Defecto | Dónde | Referencia |
|---|---|---|---|
| I1 | Los botones de idioma y de tema **no se pueden pulsar**: llevan un contenedor hijo que intercepta el ratón | `Shell/LocaleSection.cs`, `Shell/ThemeSection.cs` | [`05` §5.1](../pcfutbol/05-interaccion-y-controles.md) |
| I2 | Paneles con `offset` absolutos: **se salen del marco** fuera de 1280×720 | `Scenes/Main.tscn` | [`04` §2](../pcfutbol/04-gramatica-visual.md) |
| I3 | `ThemeSection` sigue llamando a `Tr()`: muestra claves en crudo | `Shell/ThemeSection.cs` | [`05` §5.4](../pcfutbol/05-interaccion-y-controles.md) |
| I4 | Ajustes no agrupa por tema ni ofrece `CANCELAR` | `Shell/OptionsMenu.cs` | [`03` §Q](../pcfutbol/03-mapa-de-pantallas.md) |
| I5 | Cada refresco reconstruye el árbol completo ⇒ parpadeo y lentitud percibida | `Workspace/ManagerWorkspaceController.cs` | [`05` §5.5](../pcfutbol/05-interaccion-y-controles.md) |
| I6 | Sin banda de contexto real (club · calendario · tesorería) ni barra de mando | `Scenes/Main.tscn` | [`04` §2](../pcfutbol/04-gramatica-visual.md) |
