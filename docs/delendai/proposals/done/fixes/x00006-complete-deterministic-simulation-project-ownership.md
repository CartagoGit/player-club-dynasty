---
id: x00006
title: "Complete deterministic simulation project ownership"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00006 — Complete deterministic simulation project ownership

## Goal

Make the simulation slice create its executable test project and register both projects in the canonical solution.

## why

The planned simulation source cannot be tested or discovered solution-wide with its current file boundary.

## non-goals

- Implement simulation behavior.

## Slices

- global_gate: none

### S1 — Extend the simulation slice boundary
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/f00005-implement-deterministic-instant-contest-simulation.md`
- **Gate**: none
- acceptance:
  - "The runner slice owns its test project and solution registration."

## acceptance

- The runner slice owns its test project and solution registration.
