---
id: f00040
title: "Rebuild the workspace as an original classic decision loop"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: x00041, f00032, f00033, f00034, f00035, f00036, f00037, f00038, f00039
---

# f00040 — Rebuild the workspace as an original classic decision loop

## Goal

Rebuild the manager workspace as the decision loop of a classic manager, entirely original:
at most seven grouped areas, a persistent header with date, club and next fixture, an
agenda-inbox, a visible active section, keyboard and gamepad navigation, drill-down, a
visual formation, and reusable components with explicit empty, loading, ready, stale and
error states.

## why

The style guide limits main navigation to seven areas and the workspace shows nine plus the
menu, one of them disabled. There is no persistent header, no agenda that orchestrates the
manager's day, no active-section state, no visual formation, no bench or position, and the
squad rows do not open the existing dossier. Screens still carry rules and infrastructure —
history mixes filter state, query, ordering, node creation and a points rule — and options
such as density and motion are shown but neither applied nor persisted. The classic feel
comes from this grammar, not from imitating anyone else's assets.

## non-goals

- Copying PC Fútbol layout, naming, iconography or identity in any form.
- Adding a 3D or animated match view.
- Introducing new domain rules; screens compose read models only.

## Slices

- global_gate: e2e

### S1 — Group navigation into seven areas with a persistent header
- **Status**: ready
- **Files**: `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceHeader.cs`, `game/godot/Scripts/Components/SidebarNavigator.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: unit
- acceptance:
  - "Navigation exposes at most seven grouped areas and no disabled placeholder."
  - "The header always shows career date, managed club and next fixture."
  - "The active section is visually preserved across navigation."

### S2 — Add the agenda-inbox and the component state contract
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Components/AgendaInbox.cs`, `game/godot/Scripts/Workspace/Components/ComponentState.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/tests/PCDynasty.Presentation.Tests/ComponentStateTests.cs`
- **Gate**: unit
- acceptance:
  - "The agenda lists the decisions and events that require attention on the current date."
  - "Every component renders empty, loading, ready, stale and error explicitly."
  - "Screens contain no rules and no infrastructure; a structural test enforces composition and render only."

### S3 — Deliver drill-down, visual formation, accessibility and preferences
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs`, `game/godot/Scripts/Workspace/Components/FormationBoard.cs`, `game/godot/Scripts/Shell/PreferencesController.cs`, `game/godot/Theme/`, `game/tests/PCDynasty.Presentation.Tests/AccessibilityTests.cs`
- **Gate**: e2e
- acceptance:
  - "The formation is set visually with starters, bench and positions, and squad rows open the dossier."
  - "Full navigation works with keyboard, gamepad and escape."
  - "Contrast meets AA and 100, 125 and 150 percent scales pass golden captures."
  - "Density and motion preferences are applied and persisted."
  - "SmokeBoot walks the seven areas, the agenda and the five component states with no block over 8 ms."

## acceptance

- Navigation is seven grouped areas with a persistent header and a visible active section.
- Screens hold no rules and no infrastructure, enforced structurally.
- Keyboard, gamepad, contrast and scale requirements pass, and preferences persist.
- The visual identity stays original; no asset, name or layout is taken from another game.
