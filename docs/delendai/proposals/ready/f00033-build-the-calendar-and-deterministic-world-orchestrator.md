---
id: f00033
title: "Build the calendar and deterministic world orchestrator"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00031, f00032
---

# f00033 — Build the calendar and deterministic world orchestrator

## Goal

Introduce a single `AdvanceWorld` entry point that owns the career date, the world
version, the interval, the rules hash, the root seed, the fidelity and the work budget,
and register the calendar and competitions as real scheduled units producing resumable
checkpoints and ordered facts.

## why

The game has no clock. There is no `AdvanceDay`, `AdvanceWeek` or `AdvanceToDate`, and
`ICompetitionSimulationService` receives neither world version, date, root seed, rules
hash, correlation nor checkpoint. `CompetitionSimulationService.ReserveSeedRange` derives
seeds from the wall clock and a mutable counter, and the instance is created once at
bootstrap and shared across careers, so reproducibility is not achievable on the visible
path. Matches, seasons, market, economy and development cannot participate in one
deterministic agenda until this orchestrator exists, which is why every later stage
depends on it.

## non-goals

- Connecting systems that later stages own; this slice registers the ports and connects
  only the calendar and competitions.
- Introducing stubs that pretend a later system is integrated.
- Changing the visual workspace.

## Slices

- global_gate: e2e

### S1 — Define the orchestration contract and the career clock
- **Status**: ready
- **Files**: `game/src/PCDynasty.Contracts/Simulation/AdvanceWorldRequest.cs`, `game/src/PCDynasty.Contracts/Simulation/IWorldOrchestrator.cs`, `game/src/PCDynasty.Application/Simulation/WorldOrchestrator.cs`, `game/tests/PCDynasty.Application.Tests/Simulation/WorldOrchestratorTests.cs`
- **Gate**: unit
- acceptance:
  - "A request carries date, world version, interval, rules hash, root seed, fidelity, correlation and work budget."
  - "Seeds derive from the request, never from the wall clock or a mutable counter."
  - "The orchestrator is created per career and holds no process-global state."

### S2 — Schedule the calendar and competitions as real units
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Simulation/CalendarAgenda.cs`, `game/src/PCDynasty.Application/Competitions/CompetitionSimulationService.cs`, `game/tests/PCDynasty.Application.Tests/Simulation/CalendarAgendaTests.cs`
- **Gate**: unit
- acceptance:
  - "The agenda emits ordered units for fixtures and competition transitions on the career calendar."
  - "The same input produces the same facts in the same order; serial and parallel runs agree."
  - "A failure inside a unit leaves the snapshot intact."

### S3 — Expose day, week and date advancement with resumable checkpoints
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Components/AdvanceControl.cs`, `game/src/PCDynasty.Application/Simulation/WorldCheckpoint.cs`, `game/tests/PCDynasty.Application.Tests/Simulation/AdvanceRoundTripTests.cs`, `game/benchmarks/budgets.json`
- **Gate**: e2e
- acceptance:
  - "`AdvanceDay`, `AdvanceWeek` and `AdvanceToDate` are reachable from the workspace and survive save and load."
  - "The five determinism gates of ADR 0003 pass, including macro and detailed fidelity preserving calendar, contracts and rules."
  - "Each background slice yields within 8 ms and publishes time and memory."
  - "SmokeBoot advances a week, saves, reloads and reproduces the same facts."

## acceptance

- One orchestrator owns time; no other component advances the world.
- The ADR 0003 determinism gates all pass, including serial versus parallel equality and
  intact snapshots after failure.
- Advancement survives save and load, and background slices yield within 8 ms.
