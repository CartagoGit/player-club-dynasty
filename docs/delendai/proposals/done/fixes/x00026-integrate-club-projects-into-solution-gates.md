---
id: x00026
title: "Integrate Club projects into solution gates"
kind: fix
status: done
type: proposal
track: club
date: 2026-08-11
---

# x00026 — Integrate Club projects into solution gates

## Goal

Add Club and its tests to the canonical solution so facilities, academy and evolution remain covered globally.

## why

Direct Club tests pass but the canonical solution does not yet include the new projects.

## non-goals

- Changing club behavior.

## Slices

- global_gate: e2e

### S1 — Add Club projects to solution
- **Status**: done
- **Files**: `game/PCDynasty.sln`
- **Gate**: e2e
- acceptance:
  - "Canonical solution tests and builds include Club and its tests."

## acceptance

- Canonical solution tests and builds include Club and its tests.
