---
id: x00032
title: "Make benchmark regression gates executable"
kind: fix
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# x00032 — Make benchmark regression gates executable

## Goal

Add the missing benchmark test project and include it in the authoritative solution gate.

## why

The benchmark proposal names a regression test source file but omits its executable project and solution integration.

## non-goals

- Change benchmark workloads or performance budgets.

## Slices

- global_gate: e2e

### S1 — Create benchmark regression test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Benchmark.Tests/PCDynasty.Benchmark.Tests.csproj`
- **Gate**: type
- acceptance:
  - "Benchmark budget tests reference the benchmark assembly and public runtime assemblies."

### S2 — Include benchmark regression tests in solution
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/PCDynasty.sln`
- **Gate**: e2e
- acceptance:
  - "Global solution tests execute benchmark budget gates."

## acceptance

- Benchmark budget tests reference the benchmark assembly and public runtime assemblies.
- Global solution tests execute benchmark budget gates.
