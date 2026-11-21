---
id: x00028
title: "Make declarative editor slices globally testable"
kind: fix
status: done
type: proposal
track: modding
date: 2026-08-11
---

# x00028 — Make declarative editor slices globally testable

## Goal

Add the editor test project and include editor production and test projects in the authoritative solution gate.

## why

The editor proposal names tests but omits their project and solution integration.

## non-goals

- Implement editor behavior.

## Slices

- global_gate: type

### S1 — Create editor test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Editor.Tests/PCDynasty.Editor.Tests.csproj`
- **Gate**: type
- acceptance:
  - "Editor tests have an isolated executable test project referencing editor services."

### S2 — Include editor projects in solution
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Global solution build and test gates include editor production and test projects."

## acceptance

- Editor tests have an isolated executable test project referencing editor services.
- Global solution build and test gates include editor production and test projects.
