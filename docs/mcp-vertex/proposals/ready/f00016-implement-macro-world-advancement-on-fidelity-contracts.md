---
id: f00016
title: "Implement macro world advancement on fidelity contracts"
kind: feat
status: ready
type: proposal
track: world
date: 2026-08-11
---

# f00016 — Implement macro world advancement on fidelity contracts

## Goal

Implement annual macro-world advancement and lazy materialization using the pre-defined fidelity commitments, without duplicating their ownership rules.

## why

The world must evolve at scale while the dedicated fidelity protocol remains the single authority for irreversible outcomes.

## non-goals

- Redefining HistoricalCommitment or implementing all country content.

## Slices

- global_gate: e2e

### S1 — Create compact macro world state
- **Status**: pending
- **Files**: `game/src/PCDynasty.World/State/WorldCheckpoint.cs`, `game/src/PCDynasty.World/State/ClubSeasonState.cs`, `game/tests/PCDynasty.World.Tests/WorldCheckpointTests.cs`
- **Gate**: type
- acceptance:
  - "World state has explicit date, version and compact annual club state."

### S2 — Implement macro season scheduler
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.World/Seasons/MacroSeasonTransition.cs`, `game/src/PCDynasty.World/Seasons/MacroSeasonScheduler.cs`, `game/tests/PCDynasty.World.Tests/MacroSeasonTransitionTests.cs`
- **Gate**: type
- acceptance:
  - "Transitions emit ordered facts through the deterministic simulation protocol."

### S3 — Materialize from committed macro history
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.World/Materialization/EntityMaterializer.cs`, `game/src/PCDynasty.World/Materialization/MaterializationResult.cs`, `game/tests/PCDynasty.World.Tests/EntityMaterializerTests.cs`
- **Gate**: e2e
- acceptance:
  - "Materialization consumes fidelity contracts and never rewrites commitments."

## acceptance

- World state has explicit date, version and compact annual club state.
- Transitions emit ordered facts through the deterministic simulation protocol.
- Materialization consumes fidelity contracts and never rewrites commitments.
