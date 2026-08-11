---
id: x00002
title: "Declare the Knowledge test project in the vertical slice"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00002 — Declare the Knowledge test project in the vertical slice

## Goal

Make the Knowledge slice independently buildable and testable by declaring its missing test project file.

## why

The slice owns ObservationTests.cs but omits the project that compiles and runs those tests.

## non-goals

- Implement knowledge domain behavior.

## Slices

- global_gate: none

### S1 — Add the missing test project to the slice boundary
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00003-build-person-attributes-and-knowledge-vertical-slice.md`
- **Gate**: none
- acceptance:
  - "The Knowledge slice declares PCDynasty.Knowledge.Tests.csproj alongside its tests."

## acceptance

- The Knowledge slice declares PCDynasty.Knowledge.Tests.csproj alongside its tests.
