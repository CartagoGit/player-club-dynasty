---
id: f00023
title: "Implement training chemistry and match motivation"
kind: feat
status: ready
type: proposal
track: simulation
date: 2026-08-11
---

# f00023 — Implement training chemistry and match motivation

## Goal

Implement team training effects, chemistry, contextual motivation and pressure as reusable simulation inputs and explained read models.

## why

Team performance cannot be reduced to player averages, and the manager needs understandable levers.

## non-goals

- A full tactical editor, real-time training minigames or scripted story events.

## Slices

- global_gate: type

### S1 — Define training workload and response
- **Status**: pending
- **Files**: `game/src/PCDynasty.Simulation/Training/TrainingLoad.cs`, `game/src/PCDynasty.Simulation/Training/TrainingResponse.cs`, `game/tests/PCDynasty.Simulation.Tests/TrainingResponseTests.cs`
- **Gate**: type
- acceptance:
  - "Training affects form, fatigue and development through shared domain policies."

### S2 — Implement team chemistry state
- **Status**: pending
- **Files**: `game/src/PCDynasty.Simulation/Teams/TeamChemistry.cs`, `game/src/PCDynasty.Simulation/Teams/ChemistryAdvancer.cs`, `game/tests/PCDynasty.Simulation.Tests/ChemistryAdvancerTests.cs`
- **Gate**: type
- acceptance:
  - "Chemistry has explainable factors and evolves from durable team events."

### S3 — Implement match context motivation
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Simulation/Motivation/MatchContext.cs`, `game/src/PCDynasty.Simulation/Motivation/MotivationResolver.cs`, `game/tests/PCDynasty.Simulation.Tests/MotivationResolverTests.cs`
- **Gate**: type
- acceptance:
  - "Motivation and pressure are contextual inputs, not hidden permanent player attributes."

## acceptance

- Training affects form, fatigue and development through shared domain policies.
- Chemistry has explainable factors and evolves from durable team events.
- Motivation and pressure are contextual inputs, not hidden permanent player attributes.
