---
id: x00025
title: "Make Club evolution slices independently testable"
kind: fix
status: done
type: proposal
track: club
date: 2026-08-11
---

# x00025 — Make Club evolution slices independently testable

## Goal

Add the omitted Club test project so every facilities, academy and staff slice can run its TDD gate.

## why

The Club proposal owns test sources but not the project required to compile and execute them.

## non-goals

- Implementing club behavior or editing the solution before the project exists.

## Slices

- global_gate: none

### S1 — Add Club test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Club.Tests/PCDynasty.Club.Tests.csproj`
- **Gate**: none
- acceptance:
  - "The test project references Club and follows repository test conventions."

## acceptance

- The test project references Club and follows repository test conventions.
