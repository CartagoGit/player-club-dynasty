---
id: r00002
title: "Decompose the Godot manager workspace into single-responsibility screens"
kind: refactor
status: done
type: proposal
track: architecture
date: 2026-08-18
---

# r00002 — Decompose the Godot manager workspace into single-responsibility screens

## Goal

Replace the 1,500-line Godot controller with a thin workspace orchestrator, focused screen renderers, shared UI components and an isolated persistence adapter without changing manager behavior.

## why

The current MainController mixes navigation, layout construction, filtering, simulation, persistence and history state. That violates SRP, makes reuse difficult and leaves future engine work coupled to one high-risk file.

## non-goals

- Change simulation rules or starter-world content.
- Copy visual assets or layouts from an existing commercial football manager.
- Move engine-neutral application use cases in the same slice.

## Slices

- global_gate: e2e

### S1 — Extract the manager workspace composition and screens
- **Status**: done
- **Files**: `game/godot/Scripts/Presentation/MainController.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs.uid`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs.uid`, `game/godot/Scripts/Workspace/Components/WorkspaceUiFactory.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceUiFactory.cs.uid`, `game/godot/Scripts/Workspace/Persistence/CareerHistoryPersistence.cs`, `game/godot/Scripts/Workspace/Persistence/CareerHistoryPersistence.cs.uid`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/MatchReportView.cs`, `game/godot/Scripts/Workspace/Screens/MatchReportView.cs.uid`, `game/godot/Scripts/Workspace/Screens/SeasonReportView.cs`, `game/godot/Scripts/Workspace/Screens/SeasonReportView.cs.uid`, `game/godot/Scripts/Workspace/Screens/ClubPickerScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubPickerScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/WorldScreen.cs`, `game/godot/Scripts/Workspace/Screens/WorldScreen.cs.uid`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "The active Godot workspace controller only owns composition, navigation and selected-club state."
  - "Overview, squad, competitions, match report, season report, club picker, history and world rendering each live in a focused class."
  - "Shared Godot table, panel, color and text construction is reused through one component factory."
  - "Career-history SQLite loading and saving no longer live in the workspace controller."
  - "No mutable static UI state is required for history filtering."
  - "Every authored C# file under game/godot/Scripts stays below 500 lines and the architecture test enforces that limit."
  - "The existing Godot smoke journey and the complete .NET suite remain green."
- review-state: done
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: approved by delivery-verifier — Independent review verified thin workspace orchestration, focused renderers, shared UI factory, contract-based history persistence, instance filtering, file limits, architecture tests and Godot smoke.
## acceptance

- The active Godot workspace controller only owns composition, navigation and selected-club state.
- Overview, squad, competitions, match report, season report, club picker, history and world rendering each live in a focused class.
- Shared Godot table, panel, color and text construction is reused through one component factory.
- Career-history SQLite loading and saving no longer live in the workspace controller.
- No mutable static UI state is required for history filtering.
- Every authored C# file under game/godot/Scripts stays below 500 lines and the architecture test enforces that limit.
- The existing Godot smoke journey and the complete .NET suite remain green.
