---
id: x00042
title: "Restore the gates and the registry left broken by parallel work"
kind: fix
status: in-progress
type: proposal
track: product
date: 2026-08-18
---

# x00042 — Restore the gates and the registry left broken by parallel work

## Goal

Return every gate to green and make the proposal registry describe what actually shipped, after roughly thirty commits landed with CI red, the Godot smoke broken and several proposals closed without matching reality.

## why

CI has failed on every push since the shell grammar landed: `dotnet format` rejects tab-indented lines in CareerRuntime.cs and a missing final newline in HistoryScreen.cs, and because that step fails first, the Godot/SmokeBoot job and the benchmark comparison were skipped entirely and never ran. Locally the smoke is broken too: the workspace scene was restructured into Workspace/Frame, and SmokeBoot still resolves twenty node paths under the old Workspace/Layout, so it dies on the first navigation. The registry drifted as well: f00046 sits in done/ with all eleven slices still marked ready and declares eight files that do not exist (some renamed during implementation, some belonging to the .po pipeline that S11 deliberately retired), f00042 shipped but is still ready with all slices ready, f00043 and f00044 have closed slices while their proposals stay ready, f00044 leaves one slice partial, and two plugin files break the file-conventions lint.

## non-goals

- Reverting or rewriting the shell grammar, locale pipeline or tooling migration that landed in those commits
- Finishing the feature work those proposals still owe; this proposal only restores the gates and the truth of the registry
- Changing gate thresholds to make a failure disappear

## Slices

- global_gate: e2e

### S1 — Make the formatting gate green so CI runs again
- **Status**: done
- **Files**: `game/godot/Scripts/Careers/CareerRuntime.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`
- **Gate**: lint
- acceptance:
  - "`dotnet format game/PCDynasty.sln --verify-no-changes` passes."
  - "The Build job reaches its later steps, so the Godot and benchmark jobs stop being skipped."

### S2 — Repair the Godot smoke against the restructured workspace
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/godot/Tests/SmokeBoot.gd`
- **Gate**: e2e
- acceptance:
  - "Every node path resolves against the current scene, including the Workspace/Frame wrapper."
  - "The smoke reads the persistent header and the bottom status strip introduced with the classic grammar."
  - "`bun run game:smoke` passes headlessly from a clean checkout."

### S3 — Make the registry describe what shipped
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/done/f00046-self-contained-locale-json-with-strict-activation.md`, `docs/mcp-vertex/proposals/ready/f00042-adopt-the-classic-manager-interface-grammar-in-the-shell.md`, `docs/mcp-vertex/proposals/ready/f00043-extend-options-panel-with-language-and-theme-preferences.md`, `docs/mcp-vertex/proposals/ready/f00044-adopt-the-classic-pcfutbol-grammar-on-every-workspace-screen.md`
- **Gate**: lint
- acceptance:
  - "Every file a closed proposal declares exists, or the declaration is corrected to the path that shipped."
  - "Retired work, such as the .po pipeline dropped by f00046 S11, is recorded as retired rather than left as a missing file."
  - "A proposal whose slices are done no longer sits in ready/, and any slice left partial says so with what remains."
  - "`bun run check:visible-routes` passes."

### S4 — Bring the new plugin sources back under the file conventions
- **Status**: retired
- **Files**: `plugins/mcp-vertex_player-club-dynasty/src/lib/architecture-conventions.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/file-assistant.ts`
- **Gate**: lint
- acceptance:
  - "`bun run mcp-vertex:validate` reports no unmatched files."
  - "Retired: the parallel agent restructured the plugin into `conventions/` and `domain/` while this slice was open, so the two offending files no longer exist in the shape this slice described. The remaining suffix violation belongs to that agent's lane."

## acceptance

- `dotnet format game/PCDynasty.sln --verify-no-changes` passes.
- The Build job reaches its later steps, so the Godot and benchmark jobs stop being skipped.
- Every node path resolves against the current scene, including the Workspace/Frame wrapper.
- The smoke reads the persistent header and the bottom status strip introduced with the classic grammar.
- `bun run game:smoke` passes headlessly from a clean checkout.
- Every file a closed proposal declares exists, or the declaration is corrected to the path that shipped.
- Retired work, such as the .po pipeline dropped by f00046 S11, is recorded as retired rather than left as a missing file.
- A proposal whose slices are done no longer sits in ready/, and any slice left partial says so with what remains.
- `bun run check:visible-routes` passes.
- `bun run mcp-vertex:validate` reports no unmatched files.
- `bun run validate` passes end to end.

### S5 — Make the workspace screens show text instead of translation keys
- **Status**: done-progress
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/WorldScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubPickerScreen.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`
- **Gate**: e2e
- acceptance:
  - "Every screen reads its typed section from the active locale file, which is what the locale model was built for, instead of asking Godot's TranslationServer for a key nobody registers."
  - "No screen string is a SCREAMING_SNAKE key at runtime; the smoke reads real sentences."
  - "Changing the language updates the workspace, not only the menu."

### S6 — Rebuild the settings screen as a classic options menu
- **Status**: ready
- **DependsOn**: [S5]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Shell/OptionsMenu.cs`
- **Gate**: e2e
- acceptance:
  - "Settings is a menu of named sections the player walks into, not one stacked panel of every control at once."
  - "Each section follows the classic grammar: a title band, rows of label and control, and a single way back."
  - "The language, theme, content-source and display settings each live in their own section."
  - "SmokeBoot opens settings, enters a section, changes a setting and returns to the menu."

