---
id: x00015
title: "Make deterministic protocol slices testable"
kind: fix
status: done
type: proposal
track: simulation
date: 2026-08-11
---

# x00015 — Make deterministic protocol slices testable

## Goal

Give deterministic protocol slices explicit ownership of their test projects, project references, fixture configuration, and solution registration.

## why

The protocol slice has no owned tests and downstream slices cannot add the project references needed to compile.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct deterministic protocol ownership
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00012-specify-deterministic-simulation-protocol.md`
- **Gate**: none
- acceptance:
  - "Every protocol slice owns all project and test files needed by its acceptance gate."

## acceptance

- Every protocol slice owns all project and test files needed by its acceptance gate.
