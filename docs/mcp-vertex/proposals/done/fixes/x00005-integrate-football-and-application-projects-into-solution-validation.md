---
id: x00005
title: "Integrate Football and Application projects into solution validation"
kind: fix
status: done
type: proposal
track: foundation
date: 2026-08-11
---

# x00005 — Integrate Football and Application projects into solution validation

## Goal

Keep the canonical solution exhaustive after adding Football and Application assemblies and their tests.

## why

Direct project tests pass, but solution-wide validation does not discover newly created projects until they are registered.

## non-goals

- Change Football or Application behavior.

## Slices

- global_gate: type

### S1 — Register the four new projects
- **Status**: done
- **Files**: `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Solution build includes Football and Application assemblies, and solution test runs both new test suites."

## acceptance

- Solution build includes Football and Application assemblies, and solution test runs both new test suites.
