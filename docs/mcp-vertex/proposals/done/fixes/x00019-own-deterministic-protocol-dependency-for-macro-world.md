---
id: x00019
title: "Own deterministic protocol dependency for macro world"
kind: fix
status: done
type: proposal
track: world
date: 2026-08-11
---

# x00019 — Own deterministic protocol dependency for macro world

## Goal

Allow the macro scheduler slice to add the Contracts project reference required for ordered deterministic facts.

## why

The World project does not yet reference Contracts and the scheduler slice does not own its project file.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct macro scheduler ownership
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00016-implement-macro-world-advancement-on-fidelity-contracts.md`
- **Gate**: none
- acceptance:
  - "The macro scheduler slice owns PCDynasty.World.csproj."

## acceptance

- The macro scheduler slice owns PCDynasty.World.csproj.
