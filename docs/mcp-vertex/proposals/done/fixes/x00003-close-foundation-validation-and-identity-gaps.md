---
id: x00003
title: "Close foundation validation and identity gaps"
kind: fix
status: done
type: proposal
track: foundation
date: 2026-08-11
---

# x00003 — Close foundation validation and identity gaps

## Goal

Make solution-wide validation exhaustive and prevent invalid default person identities from entering entities.

## why

The solution omits several existing projects, and a default value type can bypass PersonId constructor validation.

## non-goals

- Change person persistence or introduce new domain behavior.

## Slices

- global_gate: type

### S1 — Include every existing project in solution validation
- **Status**: done
- **Files**: `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "A single solution test command builds all production projects and executes all test projects."

### S2 — Reject default person identities at the entity boundary
- **Status**: done
- **Files**: `game/src/PCDynasty.Core/People/Person.cs`, `game/tests/PCDynasty.Core.Tests/People/PersonTests.cs`
- **Gate**: type
- acceptance:
  - "Person cannot be created with a default or non-positive PersonId."

## acceptance

- A single solution test command builds all production projects and executes all test projects.
- Person cannot be created with a default or non-positive PersonId.
