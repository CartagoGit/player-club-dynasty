---
id: f00008
title: "Build macro world advancement and materialization"
kind: feat
status: retired
type: proposal
track: world
date: 2026-08-11
---

# f00008 — Build macro world advancement and materialization

## Goal

Implement the annual macro-world pipeline, lazy entity advancement, historical commitments and deterministic materialization for non-observed competitions.

## why

The project must evolve many countries for decades without simulating every match.

## non-goals

- Full eighty-country content, daily AI for every person, or UI screens.

## Slices

- global_gate: e2e

### S1 — Define world state and annual checkpoints
- **Status**: pending
- **Files**: `game/src/PCDynasty.World/PCDynasty.World.csproj`, `game/src/PCDynasty.World/State/WorldCheckpoint.cs`, `game/tests/PCDynasty.World.Tests/WorldCheckpointTests.cs`
- **Gate**: type
- acceptance:
  - "World state has explicit version, date and compact annual state."

### S2 — Implement macro season transition
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.World/Seasons/MacroSeasonTransition.cs`, `game/src/PCDynasty.World/Seasons/HistoricalCommitment.cs`, `game/tests/PCDynasty.World.Tests/MacroSeasonTransitionTests.cs`
- **Gate**: type
- acceptance:
  - "Macro transitions emit facts and respect pending commitments."

### S3 — Implement lazy materialization
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.World/Materialization/EntityMaterializer.cs`, `game/src/PCDynasty.World/Materialization/MaterializationResult.cs`, `game/tests/PCDynasty.World.Tests/EntityMaterializerTests.cs`
- **Gate**: type
- acceptance:
  - "Materialization produces a coherent state without rewriting confirmed history."

## acceptance

- World state has explicit version, date and compact annual state.
- Macro transitions emit facts and respect pending commitments.
- Materialization produces a coherent state without rewriting confirmed history.
