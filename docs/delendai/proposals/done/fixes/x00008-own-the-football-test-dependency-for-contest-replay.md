---
id: x00008
title: "Own the Football test dependency for contest replay"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00008 — Own the Football test dependency for contest replay

## Goal

Let the football adapter slice declare the test-only Application reference required for end-to-end timeline verification.

## why

The production dependency direction stays clean, but the Football test project must compose both assemblies.

## non-goals

- Add a Football-to-Application production dependency.

## Slices

- global_gate: none

### S1 — Add the Football tests project to the replay slice
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/f00005-implement-deterministic-instant-contest-simulation.md`
- **Gate**: none
- acceptance:
  - "The replay slice owns PCDynasty.Football.Tests.csproj."

## acceptance

- The replay slice owns PCDynasty.Football.Tests.csproj.
