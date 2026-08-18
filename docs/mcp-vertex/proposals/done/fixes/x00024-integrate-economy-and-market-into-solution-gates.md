---
id: x00024
title: "Integrate Economy and Market into solution gates"
kind: fix
status: done
type: proposal
track: economy
date: 2026-08-11
---

# x00024 — Integrate Economy and Market into solution gates

## Goal

Add Economy, Market and their test projects to the canonical solution so global gates cover financial and negotiation behavior.

## why

Direct slice tests pass, but canonical solution evidence must include all newly created projects.

## non-goals

- Changing economic or market behavior.

## Slices

- global_gate: e2e

### S1 — Add Economy and Market projects to solution
- **Status**: done
- **Files**: `game/PCDynasty.sln`
- **Gate**: e2e
- acceptance:
  - "Canonical solution tests and builds include Economy, Market and both test projects."

## acceptance

- Canonical solution tests and builds include Economy, Market and both test projects.
