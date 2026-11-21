---
id: v00004
title: "Close real performance and self contained distribution"
kind: perf
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: c00005, x00041, f00030, f00031, f00032, f00033, f00034, f00035, f00036, f00037, f00038, f00039, f00040, f00041
---

# v00004 — Close real performance and self-contained distribution

## Goal

Measure every mandatory workload through the game's own ports and composition root, close
the budget matrix, and ship self-contained Windows and Linux exports that need no
preinstalled SDK, runtime or editor.

## why

The benchmark suite passes but does not exercise the obligations of the master plan: there
is no 10,000-match Fast workload, no 6,400-club market, no thirty-year macro run, no
scouting or continental workload, and no measured cold start or Godot navigation. The
declared workloads in `game/benchmarks/budgets.json` exceed those actually executed, and the
benchmark named for opening a club view measures a history query instead of the world
service. Distribution is also unfinished: there is no export preset and the launcher still
requires an installed SDK and Godot.

## non-goals

- Relaxing a budget to make a regression green; a threshold changes only through an ADR and
  a published baseline.
- Adding platforms beyond Windows and Linux desktop.

## Slices

- global_gate: e2e

### S1 — Close the workload and budget matrix on the real path
- **Status**: ready
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/`, `game/benchmarks/budgets.json`, `game/benchmarks/interaction-budgets.json`, `game/tests/PCDynasty.Benchmark.Tests/`
- **Gate**: perf
- acceptance:
  - "Every workload runs through the same ports and composition root the game uses."
  - "10,000 Fast matches complete within 1,000 ms; the remaining thresholds are the ones defined by their owning stages."
  - "Missing workloads are added: 160,000 development steps, a 6,400-club market, 320 league closes, thirty macro years, load and save, materialisation, scouting, continental classification, the SQLite path, cold start and Godot navigation."
  - "CI fails when a workload has no budget or breaches it, and publishes time, allocations, resident memory and save size."

### S2 — Ship self-contained Windows and Linux exports
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/export_presets.cfg`, `game/scripts/publish-desktop.sh`, `game/scripts/run-game.sh`, `docs/architecture/0009-performance-reference-and-budgets.md`
- **Gate**: e2e
- acceptance:
  - "Windows and Linux presets produce self-contained builds that need no .NET SDK, runtime or Godot installation."
  - "A clean export opens a single instance, creates a career, plays, saves, continues and loads an external pack or database."
  - "The published artifacts contain no tracked generated files and no development-only paths."

## acceptance

- Every mandatory workload has a versioned budget, runs on the real path and is compared in
  CI.
- 10,000 Fast matches stay within 1,000 ms and each stage's own thresholds hold.
- Clean Windows and Linux exports play, save, continue and load external content without any
  preinstalled toolchain.
