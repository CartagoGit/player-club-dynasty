---
id: f00042
title: "Adopt the classic manager interface grammar in the shell"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: x00041, f00030
---

# f00042 — Adopt the classic manager interface grammar in the shell

## Goal

Make the three surfaces the player meets first — the main menu, the club
workspace and the options panel — read like a classic desktop football manager:
a title plate over a full-bleed backdrop, a persistent club header with date and
money, a compact area rail, dense bordered panels and a status strip along the
bottom.

## why

The owner asked for menus that feel like the classic PC-era managers they grew
up with, and the current shell is a modern two-column layout: no club header, no
status strip, nine navigation entries plus the menu (the style guide allows
seven), one of them permanently disabled, and options that are shown but neither
applied nor remembered. Audit `a00001` lists the same gaps from the product
side.

The similarity is one of **grammar**, not of assets: layout, density, rhythm and
the shape of a manager's day. Names, artwork, palette, typography and copy stay
original — nothing is lifted from another game, and no trademark or protected
material enters the repository.

## non-goals

- Copying artwork, screenshots, fonts, icon sets, sounds, trademarks or names
  from any commercial game.
- The agenda-inbox, drill-down and formation board; `f00040` owns those on top
  of the systems that do not exist yet.
- Re-skinning the domain: screens keep composing read models.

## Slices

- global_gate: e2e

### S1 — Give the main menu a classic title plate and action stack
- **Status**: ready
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Theme/ManagerDesignTokens.cs`, `game/godot/Scripts/Shell/ShellChrome.cs`, `docs/design/manager-ui-style-guide.md`
- **Gate**: e2e
- acceptance:
  - "The menu presents a title plate, a stacked list of chunky uppercase actions with keyboard hints, and a status strip across the bottom."
  - "Panels use the shared bordered-plate treatment rather than ad-hoc styles."
  - "No action is a permanently disabled placeholder."
  - "SmokeBoot still starts a career from the menu."

### S2 — Give the club workspace a persistent header, area rail and status bar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceHeader.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Tests/SmokeBoot.gd`
- **Gate**: e2e
- acceptance:
  - "A header always shows the managed club with its crest placeholder, the career date and the money the club has."
  - "Navigation is at most seven grouped areas plus the exit to the menu, and the active area is visually held."
  - "A status strip along the bottom carries the last message instead of hiding it in the rail."
  - "SmokeBoot walks every area and reads the header and the status strip."

### S3 — Make the options panel a settings list that is applied and remembered
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/src/PCDynasty.Presentation.Core/PresentationPreferences.cs`, `game/tests/PCDynasty.Presentation.Tests/PresentationPreferencesTests.cs`
- **Gate**: e2e
- acceptance:
  - "Options are rows of label and control inside the same bordered plate as the rest of the shell."
  - "Full screen, reduced motion and dense tables are applied when toggled and restored on the next launch."
  - "A preference file that is missing or corrupt falls back to the defaults instead of blocking the game."
  - "SmokeBoot toggles a preference, returns to the menu and finds it still set."

## acceptance

- The menu, the club workspace and the options panel share one classic grammar:
  title plate, bordered panels, dense rows, persistent header and status strip.
- Navigation is seven areas or fewer with no disabled placeholder.
- Preferences are applied and survive a restart.
- Every asset, name and string in the shell is original to this project.
