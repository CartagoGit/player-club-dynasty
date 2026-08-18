---
id: f00044
title: "Adopt the classic PCFútbol grammar on every workspace screen"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00042
---

# f00044 — Adopt the classic PCFútbol grammar on every workspace screen

## Goal

Make every workspace screen — Squad, Market, History, Competitions, World, ClubPicker —
read like a classic PCFútbol panel: a titled **header panel** that gives the context, a
**list panel** that wraps the rows, **zebra-striped** rows with hover/select feedback, and
**action panels** at the bottom for the next decision. The grammar is the one the design
system already declares in [`docs/design/manager-ui-style-guide.md`](../../design/manager-ui-style-guide.md):
"a table compact, dense rows, bordered plates, persistent header band, status strip".

The new primitives are token-driven, so the swappable palette from f00043 re-skins them
for free and the tr() keys from f00043 reach them through the captions the user sees.

## why

The shell *as a whole* is converging on the classic grammar (f00042 S2 added the menu
plate, the workspace header, the status strip) but the **inside** of every screen is still
a loose stack of `HBoxContainer` rows over a free `VBoxContainer`. The style guide allows
"up to seven grouped areas" and forbids "ad-hoc styles" and the audit a00001 calls out
exactly this gap: each screen re-implements its own column widths, its own header bar,
its own separator. The result is a shell that looks the part at the chrome level but reads
as a worksheet inside.

The user explicitly asked for this: adopt the same UX grammar (panels, density,
interactivity) of a classic PCFútbol session on every screen. Similarity is one of
**grammar**, not of assets: layout, density, rhythm, the shape of a manager's day. Names,
artwork, palette, typography and copy stay original.

## non-goals

- Rebuilding the world projection or the SQLite store layer. That is p00002.
- Translating every cell caption with tr(). The captions this slice adds are the screen
  titles and the section headers; the column labels and the value renderers stay as they
  are until a follow-up proposal extracts them. f00043's catalogue already covers the
  locale switch.
- Adding new screens. The slice migrates the nine existing screens and the LineupSelection
  subscene's chrome.
- Copying PCFútbol assets, screenshots, names, fonts, palettes or trademarks. The visual
  grammar stays original.

## Slices

- global_gate: e2e

### S1 — Add the shared list-section primitive
- **Status**: ready
- **Files**: `game/godot/Scripts/Workspace/Components/ListSection.cs`, `game/godot/Scripts/Workspace/Components/HeaderPanel.cs`, `game/godot/Scripts/Workspace/Components/ZebraRow.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceUiFactory.cs`, `game/tests/PCDynasty.Presentation.Tests/ListSectionTests.cs`
- **Gate**: e2e
- acceptance:
  - "`ListSection<T>` is a Godot `PanelContainer` that holds a title, an optional summary
    line, a column-header row built from typed slots, and N zebra-striped data rows. Each
    row knows its index and paints a faint surface-raised background on odd rows."
  - "`HeaderPanel` is a bordered plate with an eyebrow, a title and an optional trailing
    button (e.g. 'Cambiar club', 'Ver historial'). It is the section header the style
    guide describes as 'a title plate'."
  - "`ZebraRow` is a token-driven `HBoxContainer` that exposes hover and selected states
    through the existing `ManagerDesignTokens`. Hover = surface-raised + accent border;
    selected = surface + gold border + bold text colour."
  - "Every primitive resolves its colours through `ManagerDesignTokens.ActivePalette` so
    f00043 re-skins them for free."
  - "The smoke-boot still passes."

### S2 — Migrate Squad to the new grammar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Tests/SmokeBoot.gd`
- **Gate**: e2e
- acceptance:
  - "Squad opens with a `HeaderPanel` (eyebrow 'GESTIÓN DE PLANTILLA', title with club +
    player count, optional trailing button 'Fichar')."
  - "The roster is a `ListSection<PlayerRow>` with zebra rows, a row-hover surface change,
    and a selected state that the controller can read out for the `DetailDrilldown`."
  - "Selecting a player emits a `PlayerSelected` event the controller wires to the
    existing detail panel; for the first slice the detail panel stays empty, the event is
    the contract future slices will fill."
  - "The smoke-boot path `Workspace/Layout/ContentMargin/ContentScroll/Content/SquadList`
    still resolves."

### S3 — Migrate Market to the new grammar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`
- **Gate**: e2e
- acceptance:
  - "Market opens with a `HeaderPanel` (eyebrow 'SECRETARÍA TÉCNICA', title 'MERCADO DE
    FICHAJES', summary 'X candidatos · Y disponibles · Z no disponibles') and a trailing
    button 'Renovar plantilla' (disabled until a future slice wires it)."
  - "Candidates render as `ListSection<TransferCandidate>` rows with hover + selected state."
  - "The existing sign / offer flow keeps working: the smoke-boot assertion
    `market_notice_path = market_path + "/Notice"` still finds the notice inside a
    `HeaderPanel` notice slot."
  - "A successful sign mutates the in-memory state and a `TransferSettled` signal rebuilds
    only the affected rows (not the whole list)."

### S4 — Migrate History to the new grammar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`
- **Gate**: e2e
- acceptance:
  - "History opens with a `HeaderPanel` (eyebrow 'ARCHIVO', title 'HISTORIAL DE PARTIDOS',
    summary 'X partidos · Y goles · Z competiciones')."
  - "The three filters (Competición, Club, Orden) live inside a `HeaderPanel` notice slot,
    not as loose OptionButtons."
  - "The aggregate panel stays as the existing `WorkspaceUiFactory.CreatePanel('ACUMULADO
    HISTÓRICO', ...)`; the list of matches is a `ListSection<HistoryEntry>` with zebra
    rows and a clickable 'ver partido' trailing button per row."
  - "The smoke-boot path remains reachable."

### S5 — Migrate Competitions to the new grammar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`
- **Gate**: e2e
- acceptance:
  - "Competitions opens with a `HeaderPanel` (eyebrow 'CALENDARIO', title 'COMPETICIONES
    DE LA TEMPORADA', summary 'X ligas · Y copas')."
  - "Each competition renders as a `HeaderPanel` (eyebrow = competition format, title =
    competition name, summary = 'N rivales · Frecuencia · Formato') with a trailing 'Jugar
    partido' / 'Jugar temporada' action area."
  - "A rival's competitions show the same chrome in read-only mode (the existing 'Sólo
    puedes disputar los partidos de tu club' string moves into a `HeaderPanel` notice)."

### S6 — Migrate World, ClubPicker and finish the chrome
- **Status**: ready
- **DependsOn**: [S1, S2, S3, S4, S5]
- **Files**: `game/godot/Scripts/Workspace/Screens/WorldScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubPickerScreen.cs`, `game/godot/Scripts/Matches/LineupSelectionController.cs`
- **Gate**: e2e
- acceptance:
  - "World opens with a `HeaderPanel` (eyebrow 'MUNDIAL', title 'COMPETICIONES DEL MUNDO',
    summary 'X competiciones · Y clubes inscritos')."
  - "ClubPicker opens with a `HeaderPanel` (eyebrow 'EXPLORAR', title 'SELECCIONA UN CLUB')
    and the row of the currently managed club carries a 'TU CLUB' badge in the eyebrow
    slot. The 'Elegir' button is still the trailing action."
  - "The LineupSelection subscene header (club name, competition name) is wrapped in a
    `HeaderPanel` so the chrome matches the rest of the workspace."
  - "All smoke-boot paths still resolve."

## acceptance

- Every workspace screen reads as a sequence of `HeaderPanel`s, `ListSection`s and the
  existing `WorkspaceUiFactory.CreatePanel` panels — no ad-hoc `HBoxContainer` rows
  floating outside a plate.
- Every row is a `ZebraRow` with hover + selected states driven by `ManagerDesignTokens`.
- Every caption is wrapped in `tr()` so f00043's locale switch reaches the new strings.
- Every colour is resolved through `ManagerDesignTokens.ActivePalette` so f00043's palette
  switch re-skins the new chrome for free.
- The smoke-boot still passes; the path layout under `Workspace/Layout/...` is preserved.
- No asset, name, copy or screenshot from PCFútbol enters the repository.
