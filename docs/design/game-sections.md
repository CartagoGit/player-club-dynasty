# PC Dynasty — Secciones del juego

> **Fuente de verdad canónica** de las pantallas / modos que el juego
> expone al jugador. Cualquier escena, sidebar, tab o vista debe
> corresponderse con **una y solo una** entrada de esta lista. Si estás
> añadiendo una pantalla nueva y no encaja en ninguna sección, **primero
> edita este documento y luego implementa**.

Esta lista la mantiene el equipo de diseño. Cuando un agente extienda el
juego (UI, audio, simulaciones, etc.) **debe** referirse a este documento
para evitar inventar secciones nuevas o duplicar trabajo.

---

## Las 8 secciones canónicas

| # | Sección | Subdominio Godot | Música dedicada | SFX clave | Estado |
|---|---------|------------------|-----------------|-----------|--------|
| 1 | **Menú general** | `MainMenu` (Main.tscn) | `music.menu.menu_main` | ui_* | ✅ shipped (music) |
| 2 | **Menú jugando** (workspace hub) | `Workspace/*` | `music.workspace.workspace_hub` | section_*, section_hover | 🟡 pending audio |
| 3 | **Mercado** (transferencias, ojeadores, fichajes, ventas) | `Workspace/.../Market` | `music.workspace.market_browsing` | transfer_* | 🟡 pending audio |
| 4 | **Estadio** (merchandising, ampliaciones, instalaciones) | `Workspace/.../Stadium` | `music.workspace.stadium_management` | stadium_* | 🟡 pending audio |
| 5 | **Partido** (antes, durante simulación, resultado) | `Matches/*` | `music.matchday.*` | match_* (whistle, gol, tarjeta, etc.) | ✅ shipped (music) |
| 6 | **Resultados y clasificaciones** | `Workspace/.../Competitions` | `music.workspace.standings_review` | standings_* | 🟡 pending audio |
| 7 | **Editor de juego** (pack editor) | `Editor/PackEditor.tscn` | `music.editor.editor_focus` | editor_tile_*, editor_* | 🟡 pending audio |
| 8 | **Pantalla de ganadores** (torneos, ascensos/descensos, mejores del mes) | `Matches/Trophy*` (futuro) | `music.trophy.trophy_lift` | trophy_* | 🟡 pending audio |

---

## Definición de cada sección

### 1. Menú general

El menú de arranque. El jugador todavía no ha elegido carrera ni club.
Tres vistas: **Nueva carrera**, **Continuar**, **Cargar carrera**.
Música contemplativa, invitar a empezar. Slots Godot: `MainMenu/*`.

### 2. Menú jugando (workspace hub)

Una vez elegida carrera + club, el jugador aterriza en el sidebar de 8
botones (Overview, Squad, Tactics, Competitions, Market, Finances,
World, MainMenu). La vista **Overview** es la canónica de esta sección;
el resto de tabs son sub-vistas. Música de "estás en casa, gestionando".
Slots Godot: `Workspace/Frame/Layout/ContentMargin/ContentScroll/Content`
con `currentView == "Overview"`.

### 3. Mercado

Sub-vista del workspace para transferencias, ojeadores y ventas. Vista
de listas + sliders de presupuesto + ventanas modales de oferta/
contraoferta. Música más tensa, de "negociación" — el manager sabe que
cada movimiento cuenta. Slots Godot: `Workspace/.../Content`
`currentView == "Market"`.

### 4. Estadio

Sub-vista del workspace dedicada a merchandising, ampliaciones de
grada, mejora de instalaciones (césped, vestuarios, academia, etc.).
Visual de estadio, animaciones de grúas y andamios. Música industrial-
ligera pero esperanzada: "estás construyendo tu casa". Slots Godot:
`Workspace/.../Content` `currentView == "Stadium"` (pendiente de
implementar la vista; ver propuesta `arena-001`).

### 5. Partido

Pre-match (alineación, táctica), live match (simulación 2D o resultado
instantáneo) y post-match (resumen). Sección con la música más cargada
de tensión. 6 slots de música ya shipped (`warmup`, `kickoff`, `goal`,
`fulltime`, `victory`, `defeat`). Slots Godot:
`Matches/LineupSelection.tscn` (pre) + `Matches/MatchLive.tscn` (durante,
futuro) + `Matches/MatchSummary.tscn` (post, futuro).

### 6. Resultados y clasificaciones

Sub-vista del workspace. Tabla de posiciones, goleadores, asistencia,
rachas. Lectura tranquila. Slots Godot: `Workspace/.../Content`
`currentView == "Competitions"` (la tabla es la vista por defecto).

### 7. Editor de juego

Modo creativo. Editor de packs / cromos / tácticas avanzadas. UI
minimalista (lienzo + paleta). Música concentrada pero no agresiva:
"estás en modo craft". Slots Godot: `Editor/PackEditor.tscn`.

### 8. Pantalla de ganadores

Estadios llenos, confeti, copas. Una sección para celebrar: títulos
de liga, ascensos/descensos, manager del mes, mejor once, etc. Una sola
canción de fondo (épica contenida — no grandilocuente) y stingers
cortos para cada tipo de logro. Slots Godot: futuro
`Matches/TrophyLift.tscn`, `Matches/Promotion.tscn`, `Matches/Relegation.tscn`.

---

## Cómo añadir una sección nueva

1. **Edita este documento PRIMERO.** Añade fila a la tabla, número
   correlativo, descripción, ruta Godot y slots de audio asociados.
2. Crea la escena Godot bajo `game/godot/Scenes/<dominio>/`.
3. Añade entradas en `game/content/audio/audio-spec.json` siguiendo
   el anchor musical global (`globalMusicAnchor`).
4. Ejecuta `bun run game:audio:validate` para confirmar coherencia.
5. Lanza `bun run game:audio:stage <slot>` por cada slot nuevo.

Si tu nueva sección es una **sub-vista** de una existente (p. ej. un
nuevo tab en el workspace), NO añadas fila a la tabla — usa el campo
`subdominio Godot` para clarificar la ruta. Esta tabla es solo para
**secciones de primer nivel**.

## Cómo mapear este documento al código

| Documento | Fichero |
|-----------|---------|
| Esta lista canónica (fuente de verdad) | `docs/design/game-sections.md` |
| Slots de audio declarativos | `game/content/audio/audio-spec.json` |
| Scripts de UI por sección | `game/godot/Scripts/<Dominio>/` |
| Escenas Godot por sección | `game/godot/Scenes/<Dominio>/` |
| Reglas de crossfade musical | `docs/audios/spec-format.md` (anchor global) |

---

## Historial de cambios

| Fecha | Autor | Cambio |
|-------|-------|--------|
| 2026-08-18 | Cartago | Creación inicial con las 8 secciones identificadas. Música de menú + matchday ya shipped; resto pendiente. |