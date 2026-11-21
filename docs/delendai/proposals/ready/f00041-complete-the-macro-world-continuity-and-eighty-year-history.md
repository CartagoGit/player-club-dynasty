---
id: f00041
title: "Complete the macro world continuity and eighty year history"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00033, f00034, f00035, f00036, f00037, f00038, f00039
---

# f00041 — Complete the macro world, continuity and eighty-year history

## Goal

Advance 320 leagues and 6,400 clubs at macro fidelity without freezing economy, squads,
employees or rules, preserve invariants through continental classification and
materialisation, and keep a compact historical memory of champions, tables, promotions,
relegations, titles, transfers, careers, records, major events and facilities for eighty
years.

## why

Macro advancement, materialisation and the deterministic protocol are implemented and
tested but unreachable from the game, so the world outside the managed league does not
exist yet. A manager career is only meaningful if the rest of the world lives at the same
time and if the past is remembered; this is also where the scale promises of the master
plan are actually proven.

## non-goals

- Rendering the whole world; the interface shows summaries and queries.
- Changing the fidelity contracts themselves.

## Slices

- global_gate: e2e

### S1 — Advance the macro world alongside the managed league
- **Status**: ready
- **Files**: `game/src/PCDynasty.World/Seasons/`, `game/src/PCDynasty.Application/Simulation/MacroAdvanceService.cs`, `game/tests/PCDynasty.World.Tests/MacroAdvanceServiceTests.cs`, `game/benchmarks/budgets.json`
- **Gate**: perf
- acceptance:
  - "320 leagues and 6,400 clubs advance without freezing economy, squads, employees or rules."
  - "Macro and detailed fidelity converge on the shared invariants."
  - "320 league closes complete within 1,000 ms and one club materialisation within 50 ms."

### S2 — Add continental classification and compact history
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.World/Materialization/`, `game/src/PCDynasty.History/Projections/`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/tests/PCDynasty.History.Tests/EightyYearMemoryTests.cs`
- **Gate**: e2e
- acceptance:
  - "Continental classification completes within 50 ms and preserves its invariants."
  - "An eighty-year golden career preserves every historical type."
  - "The save stays at or below 512 MiB on disk and 2 GiB resident."
  - "A thirty-year benchmark completes within 30 s outside the frame, in 8 ms slices."

## acceptance

- The macro world advances at the declared scale within budget while preserving invariants.
- An eighty-year career keeps every historical type within the disk and memory ceilings.
- No macro work blocks a frame beyond 8 ms.
