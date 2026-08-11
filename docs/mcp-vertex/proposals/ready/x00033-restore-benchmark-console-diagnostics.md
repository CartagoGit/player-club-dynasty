---
id: x00033
title: "Restore benchmark console diagnostics"
kind: fix
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# x00033 — Restore benchmark console diagnostics

## Goal

Include default console diagnostics and result columns in the custom benchmark configuration.

## why

The custom manual configuration exports files but omits a logger, so benchmark runs hide environment metadata and progress.

## non-goals

- Change benchmark workloads or budgets.

## Slices

- global_gate: type

### S1 — Add benchmark diagnostics to custom config
- **Status**: done
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/BenchmarkConfig.cs`
- **Gate**: type
- acceptance:
  - "Benchmark execution reports environment metadata and result columns."

## acceptance

- Benchmark execution reports environment metadata and result columns.
