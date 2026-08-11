---
id: v00001
title: "Establish simulation benchmarks and performance budgets"
kind: perf
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# v00001 — Establish simulation benchmarks and performance budgets

## Goal

Create reproducible benchmarks and budgets for the core simulation paths before the world scales.

## why

Performance regressions discovered after content growth are expensive and threaten the nearly instant advance experience.

## non-goals

- Premature optimization or hardware-specific tricks without measurement.

## Slices

- global_gate: e2e

### S1 — Set up benchmark harness
- **Status**: done
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/PCDynasty.Benchmarks.csproj`, `game/benchmarks/PCDynasty.Benchmarks/BenchmarkConfig.cs`, `game/benchmarks/PCDynasty.Benchmarks/README.md`
- **Gate**: type
- acceptance:
  - "Benchmarks run outside Godot with reproducible seeds and environment metadata."

### S2 — Benchmark contest and development paths
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/ContestBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/DevelopmentBenchmarks.cs`, `game/tests/PCDynasty.Benchmark.Tests/BenchmarkBudgetTests.cs`
- **Gate**: e2e
- acceptance:
  - "Budgets and regression reporting exist for hot simulation paths."

### S3 — Benchmark macro world and persistence
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/MacroWorldBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/PersistenceBenchmarks.cs`, `game/benchmarks/budgets.json`
- **Gate**: e2e
- acceptance:
  - "Macro transition, load/save and materialization budgets are explicit."

## acceptance

- Benchmarks run outside Godot with reproducible seeds and environment metadata.
- Budgets and regression reporting exist for hot simulation paths.
- Macro transition, load/save and materialization budgets are explicit.
