---
id: x00010
title: "Own football competition dependency declarations"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00010 — Own football competition dependency declarations

## Goal

Keep production dependency declarations inside the slices that consume Competition and Rules.

## why

Football scoring needs Competition and Rules references, while season read models need Competition, but the corresponding project files are absent from slice ownership.

## non-goals

- Implement football competition behavior.

## Slices

- global_gate: none

### S1 — Add project files to their consuming slices
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00007-create-the-first-football-competition-vertical-slice.md`
- **Gate**: none
- acceptance:
  - "S1 owns PCDynasty.Football.csproj and S3 owns PCDynasty.Application.csproj."

## acceptance

- S1 owns PCDynasty.Football.csproj and S3 owns PCDynasty.Application.csproj.
