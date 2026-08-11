---
id: x00027
title: "Make manager career slices globally testable"
kind: fix
status: ready
type: proposal
track: career
date: 2026-08-11
---

# x00027 — Make manager career slices globally testable

## Goal

Add the manager career test project and include both career projects in the authoritative solution gate.

## why

The manager career feature proposal names test files but omits the test project and solution integration required to execute them.

## non-goals

- Implement manager career behavior.

## Slices

- global_gate: type

### S1 — Create manager career test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Career.Tests/PCDynasty.Career.Tests.csproj`
- **Gate**: type
- acceptance:
  - "The career tests have an isolated executable test project referencing the career assembly."

### S2 — Include career projects in solution
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "The global solution build and test gates include career production and test projects."

## acceptance

- The career tests have an isolated executable test project referencing the career assembly.
- The global solution build and test gates include career production and test projects.
