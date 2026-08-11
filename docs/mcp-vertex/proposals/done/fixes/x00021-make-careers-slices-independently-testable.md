---
id: x00021
title: "Make Careers slices independently testable"
kind: fix
status: done
type: proposal
track: people
date: 2026-08-11
---

# x00021 — Make Careers slices independently testable

## Goal

Provide the test project required by the Careers proposal so every behavioral slice can execute its TDD gate directly.

## why

The Careers proposal owns test source files but omitted the test project that compiles and runs them.

## non-goals

- Implementing career behavior or editing the solution before the projects exist.

## Slices

- global_gate: none

### S1 — Add Careers test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Careers.Tests/PCDynasty.Careers.Tests.csproj`
- **Gate**: none
- acceptance:
  - "The test project references the Careers project and uses the repository test conventions."

## acceptance

- The test project references the Careers project and uses the repository test conventions.
