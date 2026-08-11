---
id: x00007
title: "Declare the Football simulation project dependency boundary"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00007 — Declare the Football simulation project dependency boundary

## Goal

Allow the football contest adapter to declare its required Simulation project reference inside its owned slice.

## why

The adapter source depends on Simulation, while the Football project file is absent from the slice boundary.

## non-goals

- Implement the adapter or timeline.

## Slices

- global_gate: none

### S1 — Add the Football project file to the adapter slice
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00005-implement-deterministic-instant-contest-simulation.md`
- **Gate**: none
- acceptance:
  - "The football adapter slice owns PCDynasty.Football.csproj."

## acceptance

- The football adapter slice owns PCDynasty.Football.csproj.
