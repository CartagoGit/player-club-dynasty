---
id: x00034
title: "Allow benchmark workload inheritance"
kind: fix
status: ready
type: proposal
track: performance
date: 2026-08-11
---

# x00034 — Allow benchmark workload inheritance

## Goal

Make existing BenchmarkDotNet workload types inheritable so generated benchmark executors can run.

## why

BenchmarkDotNet rejects benchmark methods declared in sealed classes.

## non-goals

- Change benchmark fixtures or budgets.

## Slices

- global_gate: type

### S1 — Unseal hot-path benchmark types
- **Status**: done
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/ContestBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/DevelopmentBenchmarks.cs`
- **Gate**: type
- acceptance:
  - "BenchmarkDotNet validation accepts contest and development workloads."

## acceptance

- BenchmarkDotNet validation accepts contest and development workloads.
