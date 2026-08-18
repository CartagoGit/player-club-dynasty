---
id: v00003
title: "Enforce instant manager interaction budgets"
kind: perf
status: ready
type: proposal
track: performance
date: 2026-08-18
---

# v00003 — Enforce instant manager interaction budgets

## Goal

Add reproducible benchmark workloads and CI budgets for playing one manager match and simulating a complete competition season through the new application use case.

## why

The manager workflow now has a clean application boundary, but its user-facing latency is not yet protected. Explicit median budgets prevent future features from introducing visible pauses during normal interaction.

## non-goals

- Benchmark Godot rendering or GPU frame time.
- Change simulation results to satisfy a budget.
- Run long BenchmarkDotNet jobs on every test pass.

## Slices

- global_gate: e2e

### S1 — Budget match and season interactions
- **Status**: done
- **Files**: `game/benchmarks/interaction-budgets.json`, `game/benchmarks/PCDynasty.Benchmarks/ManagerInteractionBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/README.md`, `game/tests/PCDynasty.Benchmark.Tests/InteractionBudgetTests.cs`
- **Gate**: e2e
- acceptance:
  - "A reproducible benchmark invokes CompetitionSimulationService for one match and one 20-club season."
  - "The committed match median budget fits inside one 8 ms interaction frame."
  - "The committed 20-club season median budget is at most 50 ms."
  - "CI regression tests warm the workloads and enforce median latency over five samples."
  - "Benchmark documentation names the manager interaction filter and explains the budgets."
  - "The full benchmark tests and repository gates remain green."

## acceptance

- A reproducible benchmark invokes CompetitionSimulationService for one match and one 20-club season.
- The committed match median budget fits inside one 8 ms interaction frame.
- The committed 20-club season median budget is at most 50 ms.
- CI regression tests warm the workloads and enforce median latency over five samples.
- Benchmark documentation names the manager interaction filter and explains the budgets.
- The full benchmark tests and repository gates remain green.
