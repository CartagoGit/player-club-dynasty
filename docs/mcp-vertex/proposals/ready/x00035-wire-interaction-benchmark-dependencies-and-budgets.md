---
id: x00035
title: "Wire interaction benchmark dependencies and budgets"
kind: fix
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# x00035 — Wire interaction benchmark dependencies and budgets

## Goal

Reference Application query services from the benchmark harness and copy the interaction budget contract into the regression test output.

## why

The interaction benchmark slice cannot compile or load its external budget contract with the current project wiring.

## non-goals

- Change interaction workloads or budget values.

## Slices

- global_gate: type

### S1 — Reference Application queries from benchmarks
- **Status**: done
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/PCDynasty.Benchmarks.csproj`
- **Gate**: type
- acceptance:
  - "Interaction benchmarks compile against public Application query services."

### S2 — Copy interaction budget contract to test output
- **Status**: done
- **Files**: `game/tests/PCDynasty.Benchmark.Tests/PCDynasty.Benchmark.Tests.csproj`
- **Gate**: type
- acceptance:
  - "Regression tests load the versioned external interaction budget contract independent of working directory."

## acceptance

- Interaction benchmarks compile against public Application query services.
- Regression tests load the versioned external interaction budget contract independent of working directory.
