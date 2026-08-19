# Mapa de equivalencias — el clásico contra lo que tenemos

Tabla de trabajo: qué pantalla nuestra corresponde a cuál del clásico, qué falta y
dónde está el código. Se actualiza cada vez que se cierra una propuesta de interfaz.

La referencia es **PC Fútbol 6.0/7.0** — ver
[`00-objetivo-y-alcance.md`](../pcfutbol/00-objetivo-y-alcance.md). Las pantallas
descritas a fondo, con wireframe, están en
[`13-version-objetivo-6-0-y-7-0.md`](../pcfutbol/13-version-objetivo-6-0-y-7-0.md).

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

## Pantallas propias de la versión objetivo que no existen aquí

Descritas con wireframe en [`13`](../pcfutbol/13-version-objetivo-6-0-y-7-0.md).

| Pantalla de 6.0/7.0 | Sección | Estado |
|---|---|---|
| Introduce nombre y equipo (20 ranuras + rejilla de escudos) | §4.1 | `✖` |
| Pretemporada con amistosos de pago | §4.2 | `✖` |
| Alineación como pantalla eje con raíl reactivo y dos vistas | §4.3 | `~` — tabla sí, raíl y vistas no |
| Entrenamiento / Lesionados / Estadísticas | §4.4 | `✖` |
| Estado de las ofertas (ranuras + diario fechado + placa de empleado) | §4.5 | `✖` |
| Ficha de jugador con dos tiras de pestañas y notas propias | §4.6 | `~` |
| Comparativa histórica de hasta 4 clubes | §4.7 | `✖` |
| Ficha de club (estadio, socios, patrocinador, equipador) | §4.8 | `✖` |
| Salida al campo | §4.9 | `✖` |

## Componentes: lo que la gramática pide y lo que hay

| Componente ([`04`](../pcfutbol/04-gramatica-visual.md) §3) | Hoy | Fichero |
|---|---|---|
| `Plate` | ✔ cuadrada y biselada | `Theme/ClassicChrome.cs` |
| `CaptionBar` | ✔ `CaptionPlate`, con color de área | `Theme/CaptionPlate.cs` |
| `DataTable` | Parcial — `ListSection` + `ZebraRow` con cabecera coloreada; sin cabecera fija, sin celdas pinchables, sin filas de sección | `Workspace/Components/` |
| `CommandButton` | ✔ con intención y estados | `Theme/ClassicChrome.cs`, `Theme/CommandBar.cs` |
| `AreaButton` | ✔ `ClassicChrome.AreaEntry` por `ShellArea` | `Theme/ClassicChrome.cs` |
| `BreakdownWindow` | **No existe** | — |
| `Dialog` | ✔ `ConfirmDialog` — fondo modal, Escape a la respuesta segura, foco en ella | `Theme/ConfirmDialog.cs` |
| `Stepper` | **No existe** | — |
| `StarRating` | **No existe** | — |
| `PitchDiagram` | Parcial en selección de once | `Matches/` |
| `DatePlate` | ✔ en la banda de contexto | `Theme/DatePlate.cs` |
| `CrestGrid` | **No existe** (hay `CrestPlaceholder` suelto) | `Theme/ClassicChrome.cs` |
| `EdgeRail` | ✔ rotado, con color de área | `Theme/EdgeRail.cs` |
| `DestinationTile` | ✔ placa con barra de rótulo, cifra de familia, nombre, línea de qué es, y estado «en camino» | `Theme/DestinationTile.cs`, `Scripts/Shell/DestinationBoard.cs` |
| `TabStrip` | ✔ usado por los ajustes; falta aplicarlo fuera de ellos | `Theme/TabStrip.cs` |
| `RowTab` | **No existe** | — |
| `ViewSwitch` | **No existe** | — |
| `StaffPlate` | **No existe** | — |
| `SlotList` | **No existe** | — |
| `SegmentIndicator` | **No existe** | — |
| `FieldPair` **(normativo)** | **No existe** — y es el patrón de dato más repetido del original | — |
| `HeatCell` **(normativo)** | **No existe** — las tablas no tiñen ni la celda por valor ni la fila por demarcación | — |
| `NewsTicker` | **No existe** — la noticia más reciente debería alcanzar al jugador en el eje | — |
| `SkillGrid` | **No existe** — seis destrezas nombradas en 2×3, número y estrellas a la vez | — |
| `NextFixturePlate` | **No existe** — el centro del despacho es el próximo partido, no un menú | — |
| `ModeBadge` | **No existe** — el modo se declara permanentemente en pantalla | — |
| `HoverReadout` | **No existe** — renglón de estado dedicado a lo que señala el puntero | — |

> Los siete últimos salen de leer las capturas originales una por una en
> [`16`](../pcfutbol/16-lectura-de-capturas.md). `FieldPair` y `HeatCell` son
> los que más distancia explican entre nuestras pantallas y las suyas: no es que
> nos falte adorno, es que escribimos los datos en una línea donde el original
> los ponía en dos barras, y dejamos las tablas en gris donde el original las
> teñía.

## Defectos abiertos de interfaz

| # | Defecto | Dónde | Referencia |
|---|---|---|---|
| I1 | ~~Los botones de idioma y de tema no se pueden pulsar~~ — corregido en `f00048 S2` (`SelectableTile`) | `Theme/SelectableTile.cs` | [`05` §5.1](../pcfutbol/05-interaccion-y-controles.md) |
| I2 | ~~Paneles con `offset` absolutos~~ — corregido en `f00048 S4`; la prueba de humo lo vigila | `Scenes/Main.tscn` | [`04` §2](../pcfutbol/04-gramatica-visual.md) |
| I3 | ~~`ThemeSection` llama a `Tr()`~~ — corregido en `f00048 S2` | `Shell/ThemeSection.cs` | [`05` §5.4](../pcfutbol/05-interaccion-y-controles.md) |
| I4 | ~~Ajustes no agrupa por tema~~ — corregido en `f00048 S3` (`OptionsBoard`) | `Shell/OptionsBoard.cs` | [`03` §Q](../pcfutbol/03-mapa-de-pantallas.md) |
| I5 | **Abierto**: cada refresco reconstruye el árbol completo ⇒ parpadeo y lentitud percibida | `Workspace/ManagerWorkspaceController.cs` | [`05` §5.5](../pcfutbol/05-interaccion-y-controles.md) |
| I6 | ~~Sin banda de contexto ni barra de mando~~ — corregido en `f00048 S5` | `Scenes/Main.tscn` | [`04` §2](../pcfutbol/04-gramatica-visual.md) |
| I7 | ~~Todas las tablas renderizaban vacías~~ (scroll anidado con altura mínima cero) — corregido en `f00048` | `Workspace/Components/ListSection.cs` | [`05` §5.5](../pcfutbol/05-interaccion-y-controles.md) |
| I8 | ~~El cambio de paleta no reconocía los colores anteriores~~ — corregido en `f00048` | `Shell/ThemePaletteApplier.cs` | — |
