---
id: r00004
title: "Replace process-global histories with an explicit career session"
kind: refactor
status: review
type: proposal
track: architecture
date: 2026-08-18
---

# r00004 — Replace process-global histories with an explicit career session

## Goal

Scope match and season history to one injected career session so separate careers, tests and future save slots cannot share mutable global state.

## why

MatchHistory.Instance and SeasonHistory.Instance are service locators. They hide ownership, duplicate state across UI and persistence, and make multiple loaded careers unsafe.

## non-goals

- Move SQLite tables in this slice.
- Change match or season simulation rules.
- Add multiple save-slot selection UI.

## Slices

- global_gate: e2e

### S1 — Inject a career-scoped history owner
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/CareerSession.cs`, `game/src/PCDynasty.Presentation.Core/MatchHistory.cs`, `game/src/PCDynasty.Presentation.Core/SeasonHistory.cs`, `game/tests/PCDynasty.Presentation.Tests/CareerSessionTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchHistoryTests.cs`, `game/tests/PCDynasty.Presentation.Tests/SeasonHistoryTests.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Persistence/CareerHistoryPersistence.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "CareerSession is the single owner of match and season histories for one active career."
  - "MatchHistory and SeasonHistory have no singleton Instance or testing-only factory."
  - "Godot bootstrap composes one session and injects it into screens and persistence."
  - "No Godot workspace source accesses mutable global history state."
  - "Independent session tests prove histories cannot leak between careers."
  - "The complete .NET suite and Godot smoke journey remain green."

## acceptance

- CareerSession is the single owner of match and season histories for one active career.
- MatchHistory and SeasonHistory have no singleton Instance or testing-only factory.
- Godot bootstrap composes one session and injects it into screens and persistence.
- No Godot workspace source accesses mutable global history state.
- Independent session tests prove histories cannot leak between careers.
- The complete .NET suite and Godot smoke journey remain green.
