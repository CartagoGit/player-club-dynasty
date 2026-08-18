---
id: x00038
title: "Run golden replay child from the active build output"
kind: fix
status: done
type: proposal
track: quality
date: 2026-08-11
---

# x00038 — Run golden replay child from the active build output

## Goal

Make the independent replay test execute the exact test assembly already built by the parent configuration.

## why

GitHub compiles Release, but invoking dotnet test on the project without an explicit configuration makes the child look for Debug output.

## non-goals

- Change deterministic replay semantics or weaken process isolation.

## Slices

- global_gate: e2e

### S1 — Target the current test assembly
- **Status**: done
- **Files**: `game/tests/PCDynasty.Simulation.Tests/GoldenReplayTests.cs`
- **Gate**: e2e
- acceptance:
  - "The independent replay test passes from both Debug and Release outputs without rebuilding."

## acceptance

- The independent replay test passes from both Debug and Release outputs without rebuilding.
