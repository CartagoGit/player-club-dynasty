---
id: f00005
title: "Implement deterministic instant contest simulation"
kind: feat
status: ready
type: proposal
track: simulation
date: 2026-08-11
---

# f00005 — Implement deterministic instant contest simulation

## Goal

Build a seed-driven, sport-neutral contest pipeline and a football adapter capable of producing an inspectable instant-match timeline.

## why

Fast, reproducible match simulation is central to the manager experience and establishes the deterministic engine contract.

## non-goals

- 3D match rendering, complete tactical AI, global season simulation.

## Slices

- global_gate: type

### S1 — Define contest snapshots and event contracts
- **Status**: pending
- **Files**: `game/src/PCDynasty.Core/Contests/ContestSnapshot.cs`, `game/src/PCDynasty.Core/Contests/ContestEvent.cs`, `game/tests/PCDynasty.Core.Tests/Contests/ContestSnapshotTests.cs`
- **Gate**: type
- acceptance:
  - "Snapshots are immutable, serializable through ports and sport-neutral."

### S2 — Implement deterministic contest runner
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Simulation/PCDynasty.Simulation.csproj`, `game/src/PCDynasty.Simulation/Contests/ContestSimulator.cs`, `game/tests/PCDynasty.Simulation.Tests/ContestSimulatorTests.cs`
- **Gate**: type
- acceptance:
  - "Same inputs and seed always produce the same ordered events."

### S3 — Add football contest adapter and replay view
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Football/Simulation/FootballContestPack.cs`, `game/src/PCDynasty.Application/Matches/MatchTimelineQuery.cs`, `game/tests/PCDynasty.Football.Tests/FootballContestPackTests.cs`
- **Gate**: type
- acceptance:
  - "Football mapping stays in the pack and produces a read-only timeline."

## acceptance

- Snapshots are immutable, serializable through ports and sport-neutral.
- Same inputs and seed always produce the same ordered events.
- Football mapping stays in the pack and produces a read-only timeline.
