---
id: f00012
title: "Specify deterministic simulation protocol"
kind: feat
status: ready
type: proposal
track: simulation
date: 2026-08-11
---

# f00012 — Specify deterministic simulation protocol

## Goal

Define and implement a versioned deterministic protocol for random generation, canonical inputs, arithmetic, ordering and golden replay verification.

## why

Reproducibility must be an executable contract, not merely an intention, for long-lived saves and reliable debugging.

## non-goals

- Gameplay balancing or parallel performance optimization beyond correctness.

## Slices

- global_gate: type

### S1 — Define deterministic protocol contracts
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/PCDynasty.Contracts.csproj`, `game/src/PCDynasty.Contracts/Simulation/DeterministicSimulationSpec.cs`, `game/src/PCDynasty.Contracts/Simulation/EventSequence.cs`, `game/tests/PCDynasty.Contracts.Tests/PCDynasty.Contracts.Tests.csproj`, `game/tests/PCDynasty.Contracts.Tests/DeterministicSimulationSpecTests.cs`, `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Protocol versions PRNG, canonical serialization, time, precision and total event ordering."

### S2 — Implement deterministic seed and arithmetic services
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Simulation/PCDynasty.Simulation.csproj`, `game/src/PCDynasty.Simulation/Determinism/SeedDeriver.cs`, `game/src/PCDynasty.Simulation/Determinism/DeterministicNumber.cs`, `game/tests/PCDynasty.Simulation.Tests/DeterminismTests.cs`
- **Gate**: type
- acceptance:
  - "Derived seeds and numeric outcomes are stable across runs."

### S3 — Add golden replay fixtures
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/tests/PCDynasty.Simulation.Tests/PCDynasty.Simulation.Tests.csproj`, `game/tests/PCDynasty.Simulation.Tests/Fixtures/GoldenReplay.json`, `game/tests/PCDynasty.Simulation.Tests/GoldenReplayTests.cs`, `game/tests/PCDynasty.Simulation.Tests/ParallelReplayTests.cs`
- **Gate**: e2e
- acceptance:
  - "Independent process and series/parallel execution reproduce canonical facts."

## acceptance

- Protocol versions PRNG, canonical serialization, time, precision and total event ordering.
- Derived seeds and numeric outcomes are stable across runs.
- Independent process and series/parallel execution reproduce canonical facts.
