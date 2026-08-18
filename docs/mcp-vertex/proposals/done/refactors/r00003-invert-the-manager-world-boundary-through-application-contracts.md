---
id: r00003
title: "Invert the manager world boundary through Application contracts"
kind: refactor
status: done
type: proposal
track: architecture
date: 2026-08-18
---

# r00003 — Invert the manager world boundary through Application contracts

## Goal

Make Godot manager screens depend on an immutable Application query port while retaining content loading as a replaceable Presentation.Core adapter.

## why

WorldService is currently a process-global singleton and its concrete type plus read models leak through every screen, contradicting the documented UI-to-Application dependency rule and making alternate databases or engines difficult to substitute.

## non-goals

- Change starter-world JSON schemas.
- Replace match or season simulation in this slice.
- Move career history persistence.

## Slices

- global_gate: e2e

### S1 — Introduce the manager world query port and remove the singleton
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/Workspace/IManagerWorld.cs`, `game/tests/PCDynasty.Application.Tests/Workspace/ManagerWorldContractTests.cs`, `game/src/PCDynasty.Presentation.Core/PCDynasty.Presentation.Core.csproj`, `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/src/PCDynasty.Presentation.Core/MatchSimulator.cs`, `game/src/PCDynasty.Presentation.Core/SeasonSimulator.cs`, `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Presentation/ClubOverviewProjector.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubPickerScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/godot/Scripts/Workspace/Screens/SeasonReportView.cs`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scripts/Workspace/Screens/WorldScreen.cs`, `game/tests/PCDynasty.Presentation.Tests/PCDynasty.Presentation.Tests.csproj`, `game/tests/PCDynasty.Presentation.Tests/WorldServiceTests.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldOverviewTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchSimulatorTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchStatsTests.cs`, `game/tests/PCDynasty.Presentation.Tests/SeasonSimulatorTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PCDynasty.Architecture.Tests.csproj`, `game/tests/PCDynasty.Architecture.Tests/PresentationCoreDependencyTests.cs`
- **Gate**: e2e
- acceptance:
  - "IManagerWorld and its immutable rows live in Application and reference no presentation, Godot, content-format or persistence assembly."
  - "Every Godot screen and menu accepts IManagerWorld rather than WorldService."
  - "WorldService is a normal instantiable adapter with no static Instance or Initialize service locator."
  - "Presentation.Core no longer references the concrete Content.Importers assembly."
  - "Architecture and behavior tests prove the dependency direction and independent WorldService instances."
  - "The complete .NET suite and Godot smoke journey remain green."
- review-state: done
- review-implementer: root
- review-reviewer: fresh-delivery-verifier
- review-log: requested_changes by delivery-verifier — CompetitionRow retains the caller IReadOnlyList<string> without a defensive copy, so its Rivals can mutate after construction. Add an aliasing test and store an immutable copy.
- review-log: approved by fresh-delivery-verifier — Fresh independent re-review approved: defensive immutable snapshot and aliasing regression are correct; Application and Presentation tests, Godot Debug build, API compatibility and diff check pass.
## acceptance

- IManagerWorld and its immutable rows live in Application and reference no presentation, Godot, content-format or persistence assembly.
- Every Godot screen and menu accepts IManagerWorld rather than WorldService.
- WorldService is a normal instantiable adapter with no static Instance or Initialize service locator.
- Presentation.Core no longer references the concrete Content.Importers assembly.
- Architecture and behavior tests prove the dependency direction and independent WorldService instances.
- The complete .NET suite and Godot smoke journey remain green.
