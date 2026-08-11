---
id: v00002
title: "Define quantified gameplay latency budgets"
kind: perf
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# v00002 — Define quantified gameplay latency budgets

## Goal

Set hardware-referenced budgets and test scenarios for immediate player interactions, simulation steps, materialization and load/save.

## why

The game must feel instant while playing; benchmarks need product-visible limits, not just raw measurements.

## non-goals

- Guaranteeing performance on unknown hardware without profiling or hiding unavoidable long operations.

## Slices

- global_gate: e2e

### S1 — Define reference hardware and latency budgets
- **Status**: pending
- **Files**: `game/benchmarks/interaction-budgets.json`, `docs/architecture/0009-performance-reference-and-budgets.md`
- **Gate**: none
- acceptance:
  - "Budgets distinguish UI interaction, background work, season advance and first-load scenarios."

### S2 — Create interaction performance scenarios
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/InteractionBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/MaterializationBenchmarks.cs`, `game/tests/PCDynasty.Benchmark.Tests/InteractionBudgetTests.cs`
- **Gate**: e2e
- acceptance:
  - "Opening player, club and competition views remains within defined interaction budgets."

### S3 — Implement background-work telemetry contract
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Performance/WorkBudget.cs`, `game/src/PCDynasty.Application/Performance/BackgroundWorkStatus.cs`, `game/tests/PCDynasty.Application.Tests/WorkBudgetTests.cs`
- **Gate**: type
- acceptance:
  - "Long work checkpoints without blocking commands or pretending stale data is final."

## acceptance

- Budgets distinguish UI interaction, background work, season advance and first-load scenarios.
- Opening player, club and competition views remains within defined interaction budgets.
- Long work checkpoints without blocking commands or pretending stale data is final.
