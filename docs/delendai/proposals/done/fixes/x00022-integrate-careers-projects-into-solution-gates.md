---
id: x00022
title: "Integrate Careers projects into solution gates"
kind: fix
status: done
type: proposal
track: people
date: 2026-08-11
---

# x00022 — Integrate Careers projects into solution gates

## Goal

Add Careers production and test projects to the canonical solution so global builds and tests cover the completed module.

## why

The Careers proposal created independently testable projects, but the canonical solution cannot prove their health until it includes them.

## non-goals

- Changing career behavior or other solution projects.

## Slices

- global_gate: e2e

### S1 — Add Careers projects to canonical solution
- **Status**: done
- **Files**: `game/PCDynasty.sln`
- **Gate**: e2e
- acceptance:
  - "Canonical solution tests and builds include both Careers projects."

## acceptance

- Canonical solution tests and builds include both Careers projects.
