---
id: f00048
title: "Rebuild the shell chrome to the classic look and make its controls work"
kind: feat
status: in-progress
type: proposal
track: product
date: 2026-08-19
plan: q00001
audit-source: a00001
depends-on: f00047
---

# f00048 — Rebuild the shell chrome to the classic look and make its controls work

## Goal

Replace the current chrome — rounded dark plates that read as a generic dark web
app — with the interface grammar `docs/wiki/pcfutbol/04-gramatica-visual.md` declares:
square bevelled plates, coloured caption bars, area colour coding, dense tables, edge
rails, a persistent context band and a fixed command bar. In the same pass, fix the
three defects that make the shell feel broken rather than merely plain: controls that
cannot be clicked, panels that leave the window, and captions that render as raw keys.

## why

Three interface proposals have shipped and the shell still does not read as a classic
manager, because the previous work adopted the *vocabulary* (a plate, a header, a
zebra row) without the *grammar* (how a plate announces itself, how areas are
colour-coded, where a command lives, what relief looks like). The gap is now written
down and normative — f00047 — so this proposal is the first one held to it.

Three defects make it worse than plain:

1. **The language and theme pickers cannot be clicked at all.** Both build a `Button`
   and add an `HBoxContainer`/`VBoxContainer` as its child. `Button` is not a
   container: the child inherits `MouseFilter.Stop` and swallows every click, so
   `Pressed` never fires — and, because a button does not lay out children, that child
   also draws at its natural size outside the button's frame. That is both the "no
   deja pulsar" and part of the "sobresalen de donde deberían".
2. **The menu panels are pinned with absolute offsets** (`offset_right = 1224`) sized
   for one window. At any other size they overflow or get clipped.
3. **`ThemeSection` still calls `Tr()`**, so the theme area shows raw keys — the same
   defect `x00042` fixed everywhere else, missed in this one file.

## non-goals

- New screens or new game systems. This proposal changes chrome and fixes defects; the
  missing screens are listed in `docs/wiki/pc-dynasty/mapa-de-equivalencias.md` and
  need their own proposals.
- The match viewer. See `docs/wiki/pcfutbol/12-mejoras-para-pc-dynasty.md` B1.
- Copying any asset, font, artwork or trademark. Grammar only.

## Slices

- global_gate: e2e

### S1 — Rebuild the chrome primitives
- **Status**: done
- **Files**: `game/godot/Theme/ClassicChrome.cs`, `game/godot/Theme/ManagerDesignTokens.cs`, `game/godot/Theme/IPaletteAware.cs`, `game/godot/Theme/ShellArea.cs`, `game/godot/Theme/CaptionPlate.cs`, `game/godot/Theme/CommandBar.cs`, `game/godot/Theme/EdgeRail.cs`, `game/godot/Scripts/Shell/ThemePaletteApplier.cs`
- **Gate**: e2e
- acceptance:
  - "Plates are square (corner radius 0) and carry a bevel: a lit top/left edge and a
    shadowed bottom/right one, both derived from the active palette."
  - "`ClassicChrome` exposes a caption-bar style taking an area colour, a command
    style per intent and state, a table-header style, a row style per parity and
    selection, an inset well, a context band, a status strip and a command bar."
  - "`ManagerDesignTokens.AreaColor(ShellArea)` derives one colour per functional area
    from the active palette, so a palette switch re-colours the areas and no screen
    ever names a literal colour."
  - "Components that build colours the applier cannot infer implement `IPaletteAware`;
    `ThemePaletteApplier` calls them after a palette change so nothing goes stale."

### S2 — Make composite controls clickable
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/godot/Theme/SelectableTile.cs`, `game/godot/Scripts/Shell/LocaleSection.cs`, `game/godot/Scripts/Shell/ThemeSection.cs`
- **Gate**: e2e
- acceptance:
  - "`SelectableTile` is a `PanelContainer` that lays out arbitrary children through a
    real container, handles its own mouse and keyboard input, and exposes
    `Pressed`, `Selected` and group behaviour. Every descendant is forced to
    `MouseFilter.Ignore` when added, so nothing can swallow the click again."
  - "The language picker and the theme picker are built from `SelectableTile`; no
    `Button` in the shell has a `Control` child."
  - "`ThemeSection` renders the strings it is handed and no longer calls `Tr()`."
  - "The smoke-boot presses a language tile and a theme tile and asserts the
    selection changed."

### S3 — Rebuild settings as a configuration board
- **Status**: done
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Shell/OptionsBoard.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Tests/SmokeBoot.gd`
- **Gate**: e2e
- acceptance:
  - "Settings is one screen: every group — language, look, display, data — is a caption
    plate, all visible at once, laid out in a grid that reflows with the window."
  - "The board owns navigation and chrome only; callers still register a group with a
    title and a `Control`, so the four groups stay independent of each other and of
    this file."
  - "The board carries an edge rail with the screen name and a command bar whose only
    exit is `VOLVER`."
  - "Nothing in the board is addressed by index; every group keeps a stable node name."

### S4 — Anchor and re-dress the main menu
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`
- **Gate**: e2e
- acceptance:
  - "The main menu is a stack of anchored bands — context band, canvas, status strip,
    command bar — with no absolute offset anywhere in the scene."
  - "Nothing leaves its container at 1024×600 or at 2560×1440."
  - "Destinations render as caption-plated tiles; `SALIR` lives in the command bar."
  - "The career setup and the settings board share the same frame, so switching
    between them never changes the position of the bands."

### S5 — Colour-code the workspace and give it a command bar
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceHeader.cs`, `game/godot/Theme/DatePlate.cs`, `game/godot/Scripts/Shell/LocaleSession.cs`
- **Gate**: e2e
- acceptance:
  - "Every area in the rail carries its area colour, and the active one carries it at
    full strength; the same colour appears in the caption bar of the screen it opens."
  - "The context band shows crest, club, a date plate and the treasury, and stays put
    across every screen."
  - "A command bar sits under the status strip with the exit to the main menu.
    The advance command it reserves on the right lands with the calendar
    orchestrator (f00033); until then the slot says so rather than offering a
    button that does nothing."

### S6 — Prove it in the smoke test
- **Status**: done
- **DependsOn**: [S2, S3, S4]
- **Files**: `game/godot/Tests/SmokeBoot.gd`, `game/godot/Tests/CaptureShots.gd`
- **Gate**: e2e
- acceptance:
  - "The smoke asserts that no panel in the main menu exceeds the viewport rectangle."
  - "The smoke presses a language tile and a theme tile through the new controls and
    asserts the selection actually changed."
  - "The smoke reaches the data group of the settings board by its stable node name."

## acceptance

- Every checklist item in `docs/wiki/pcfutbol/04-gramatica-visual.md` §6 holds for the
  main menu, the settings board and the workspace chrome.
- No `Button` in the shell has a `Control` child; no scene node uses absolute offsets
  for layout; no shared component calls `tr()`.
- A palette switch re-colours the new chrome, including the area colours.
- The smoke-boot passes and the three CI jobs stay green.

## notes

Found while doing the work, fixed here, and worth recording because none of it
was visible from the code alone:

- **Every table in the game rendered empty.** `ListSection` wrapped its rows in
  a `ScrollContainer`, and the workspace canvas is itself a `ScrollContainer`: a
  nested one reports a minimum height of zero, so the rows were in the tree,
  laid out at zero pixels. The squad screen said "22 players" over a header with
  nothing under it. The smoke never caught it because it counted children rather
  than measuring them; it now asserts a row has real height.
- **The palette applier could not change a palette.** It set the new palette
  first and then matched every StyleBox against `ActivePalette` — which by then
  was the new one — so it only recognised colours that were already correct. It
  now matches against the palette being replaced.
- **The career setup lined its columns up with padded strings**
  (`{club.DisplayName,-32}`), which cannot align in a proportional font. It is a
  dense table now.
- **The hero panel stayed visible under the settings board**, because
  `OpenOptions` hid the menu frame and the career setup but never the hero.
- `CaptureShots.gd` renders the shell to PNG per route, so a change of chrome can
  be reviewed by looking at it rather than by describing it:
  `godot --path game/godot --script res://Tests/CaptureShots.gd`.
- The exit moved out of the area rail and into the command bar. It was never an
  area, and the classic desk always kept leaving on the bottom strip.
- The date plate exposed a second defect: weekday and month names came out in
  the operating system's language, because nothing ever pointed .NET's culture
  at the chosen locale. `LocaleSession` now adopts it, so every framework-
  formatted value — dates, separators, currency — follows the game's language.
