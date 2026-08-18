---
id: r00006
title: "Move competition orchestration out of Godot"
kind: refactor
status: done
type: proposal
track: architecture
date: 2026-08-18
---

# r00006 — Move competition orchestration out of Godot

## Goal

Make Godot a pure presentation adapter by moving match and season orchestration, clock ownership and seed generation into an injected application service.

## why

CompetitionsScreen currently selects seeds, reads the system clock, calls simulation helpers and assembles persistence records. Those responsibilities couple UI code to manager-engine policy and make interaction flows harder to test, reuse and benchmark.

## non-goals

- Change contest simulation mathematics.
- Add asynchronous season progression.
- Redesign the competition screen visuals.

## Slices

- global_gate: e2e

### S1 — Introduce the competition simulation use case
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/src/PCDynasty.Application/Competitions/Simulation/ICompetitionSimulationService.cs`, `game/src/PCDynasty.Application/Competitions/Simulation/CompetitionSimulationService.cs`, `game/src/PCDynasty.Application/Competitions/Simulation/MatchSimulator.cs`, `game/src/PCDynasty.Application/Competitions/Simulation/SeasonSimulator.cs`, `game/src/PCDynasty.Presentation.Core/MatchSimulator.cs`, `game/src/PCDynasty.Presentation.Core/SeasonSimulator.cs`, `game/src/PCDynasty.Presentation.Core/MatchStats.cs`, `game/src/PCDynasty.Presentation.Core/PCDynasty.Presentation.Core.csproj`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/MatchReportView.cs`, `game/godot/Scripts/Workspace/Screens/SeasonReportView.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`, `game/tests/PCDynasty.Application.Tests/Competitions/CompetitionSimulationServiceTests.cs`, `game/tests/PCDynasty.Application.Tests/Competitions/MatchSimulatorTests.cs`, `game/tests/PCDynasty.Application.Tests/Competitions/SeasonSimulatorTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchSimulatorTests.cs`, `game/tests/PCDynasty.Presentation.Tests/SeasonSimulatorTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchStatsTests.cs`
- **Gate**: e2e
- acceptance:
  - "Godot workspace code does not read DateTime, generate seeds or call MatchSimulator or SeasonSimulator."
  - "ICompetitionSimulationService is owned by Application and returns complete match and season executions including persistence records."
  - "CompetitionSimulationService owns an injected TimeProvider and deterministic replay behavior."
  - "Pure match and season engines live outside Presentation.Core."
  - "The composition root injects the application service into the competition screen."
  - "Application tests cover deterministic replay, timestamps, unique seeds and season history projection."
  - "The complete .NET suite and Godot smoke journey remain green."
- review-state: done
- review-implementer: root
- review-reviewer: fresh-delivery-verifier
- review-log: requested_changes by delivery-verifier — PlaySeason reserves only seed S while SeasonSimulator consumes S..S+n-1; the next operation receives S+1 and collides with persisted fixture history. Snapshot participants, atomically reserve the full fixture range with overflow handling, and test season-to-match plus parallel disjoint ranges.
- review-log: approved by fresh-delivery-verifier — Fresh independent re-review approved: immutable participant snapshot, checked n*(n-1)/2 fixture count and atomic full-range reservation eliminate sequential and concurrent seed collisions while replay and Godot consumers remain unchanged; all focused gates pass.
## acceptance

- Godot workspace code does not read DateTime, generate seeds or call MatchSimulator or SeasonSimulator.
- ICompetitionSimulationService is owned by Application and returns complete match and season executions including persistence records.
- CompetitionSimulationService owns an injected TimeProvider and deterministic replay behavior.
- Pure match and season engines live outside Presentation.Core.
- The composition root injects the application service into the competition screen.
- Application tests cover deterministic replay, timestamps, unique seeds and season history projection.
- The complete .NET suite and Godot smoke journey remain green.
