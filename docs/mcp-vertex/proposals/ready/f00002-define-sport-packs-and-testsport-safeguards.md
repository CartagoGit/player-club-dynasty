---
id: f00002
title: "Define sport packs and TestSport safeguards"
kind: feat
status: ready
type: proposal
track: core
date: 2026-08-11
---

# f00002 — Define sport packs and TestSport safeguards

## Goal

Implement the minimal sport-neutral pack contract and an artificial TestSport that proves the Core never requires football concepts.

## why

The core extensibility invariant needs an executable guard before FootballPack grows.

## non-goals

- A general-purpose mod scripting runtime.
- A complete FootballPack or competition system.

## Slices

- global_gate: type

### S1 — Create sport contract primitives
- **Status**: done
- **Files**: `game/src/PCDynasty.Packs.Abstractions/PCDynasty.Packs.Abstractions.csproj`, `game/src/PCDynasty.Packs.Abstractions/Sports/ISportPack.cs`, `game/src/PCDynasty.Packs.Abstractions/Sports/SportDefinition.cs`
- **Gate**: type
- acceptance:
  - "Defines stable, football-free contracts using compiled IDs."

### S2 — Implement TestSport definitions
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.TestSport/PCDynasty.TestSport.csproj`, `game/src/PCDynasty.TestSport/TestSportPack.cs`, `game/tests/PCDynasty.TestSport.Tests/TestSportPackTests.cs`
- **Gate**: type
- acceptance:
  - "A one-versus-one sport loads without clubs, positions, goals or bench."

### S3 — Add core independence guard
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/tests/PCDynasty.Architecture.Tests/PCDynasty.Architecture.Tests.csproj`, `game/tests/PCDynasty.Architecture.Tests/CoreDependencyTests.cs`
- **Gate**: type
- acceptance:
  - "Tests reject forbidden Godot and football dependencies from Core."

## acceptance

- Defines stable, football-free contracts using compiled IDs.
- A one-versus-one sport loads without clubs, positions, goals or bench.
- Tests reject forbidden Godot and football dependencies from Core.
