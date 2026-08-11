---
id: f00013
title: "Define macro and detailed fidelity commitments"
kind: feat
status: ready
type: proposal
track: world
date: 2026-08-11
---

# f00013 — Define macro and detailed fidelity commitments

## Goal

Define the irreversible commitments, promotion rules and differential tests that keep Macro, Detailed and Materialize simulation coherent.

## why

World scale is unsafe unless low-fidelity summaries can become detailed without rewriting confirmed history.

## non-goals

- Implementing every macro subsystem or country dataset.

## Slices

- global_gate: type

### S1 — Define fidelity ledger contracts
- **Status**: done
- **Files**: `game/src/PCDynasty.World/PCDynasty.World.csproj`, `game/src/PCDynasty.World/Commitments/HistoricalCommitment.cs`, `game/src/PCDynasty.World/Commitments/FidelityOwnership.cs`, `game/tests/PCDynasty.World.Tests/PCDynasty.World.Tests.csproj`, `game/tests/PCDynasty.World.Tests/FidelityOwnershipTests.cs`, `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Committed outcomes, ownership intervals and promotion boundaries are explicit."

### S2 — Implement fidelity promotion protocol
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.World/Materialization/FidelityPromotion.cs`, `game/src/PCDynasty.World/Materialization/MaterializationLedger.cs`, `game/tests/PCDynasty.World.Tests/FidelityPromotionTests.cs`
- **Gate**: type
- acceptance:
  - "Materialization never rewrites confirmed history."

### S3 — Add cross-fidelity differential suite
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/tests/PCDynasty.World.Tests/CrossFidelitySimulationTests.cs`, `game/tests/PCDynasty.World.Tests/Fixtures/FidelityScenario.json`
- **Gate**: e2e
- acceptance:
  - "Macro, Detailed and Materialize agree on commitments and invariant outcomes."

## acceptance

- Committed outcomes, ownership intervals and promotion boundaries are explicit.
- Materialization never rewrites confirmed history.
- Macro, Detailed and Materialize agree on commitments and invariant outcomes.
