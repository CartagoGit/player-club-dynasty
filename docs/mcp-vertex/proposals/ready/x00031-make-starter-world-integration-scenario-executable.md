---
id: x00031
title: "Make starter world integration scenario executable"
kind: fix
status: ready
type: proposal
track: content
date: 2026-08-11
---

# x00031 — Make starter world integration scenario executable

## Goal

Add the content integration test project and include it in the authoritative solution gate.

## why

The starter-world proposal names integration test files but omits their executable project and solution integration.

## non-goals

- Change starter-world content.

## Slices

- global_gate: e2e

### S1 — Create content integration test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Content.Integration.Tests/PCDynasty.Content.Integration.Tests.csproj`
- **Gate**: type
- acceptance:
  - "Starter-world integration tests reference public content reader and compiler assemblies."

### S2 — Include content integration tests in solution
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/PCDynasty.sln`
- **Gate**: e2e
- acceptance:
  - "Global solution tests execute the starter-world integration scenario."

## acceptance

- Starter-world integration tests reference public content reader and compiler assemblies.
- Global solution tests execute the starter-world integration scenario.
