---
id: x00001
title: "Add executable TestSport test project"
kind: fix
status: done
type: proposal
track: quality
date: 2026-08-11
---

# x00001 — Add executable TestSport test project

## Goal

Add the missing test project definition required for the TestSport TDD slice to compile and execute its declared tests.

## why

The TestSport feature proposal declares a test source file but omitted the owning .csproj, which would leave the test outside every executable test assembly.

## non-goals

- Implementing TestSport behavior or adding future projects to the root solution.

## Slices

- global_gate: none

### S1 — Create TestSport test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.TestSport.Tests/PCDynasty.TestSport.Tests.csproj`
- **Gate**: none
- acceptance:
  - "The project references TestSport and the pack abstractions and is ready to compile once the feature files exist."
  - "The project uses the same pinned test dependencies and standards as existing tests."

## acceptance

- The project references TestSport and the pack abstractions and is ready to compile once the feature files exist.
- The project uses the same pinned test dependencies and standards as existing tests.
