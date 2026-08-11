---
id: x00023
title: "Make economy and market slices independently testable"
kind: fix
status: ready
type: proposal
track: economy
date: 2026-08-11
---

# x00023 — Make economy and market slices independently testable

## Goal

Provide the omitted test projects required to execute the Economy and Market proposal gates directly.

## why

The proposal owns behavioral test sources but omitted both test project files.

## non-goals

- Implementing finance or market behavior and editing the solution before projects exist.

## Slices

- global_gate: none

### S1 — Add Economy test project
- **Status**: done
- **Files**: `game/tests/PCDynasty.Economy.Tests/PCDynasty.Economy.Tests.csproj`
- **Gate**: none
- acceptance:
  - "Economy tests reference the Economy project and repository test packages."

### S2 — Add Market test project
- **Status**: pending
- **Files**: `game/tests/PCDynasty.Market.Tests/PCDynasty.Market.Tests.csproj`
- **Gate**: none
- acceptance:
  - "Market tests reference the Market project and repository test packages."

## acceptance

- Economy tests reference the Economy project and repository test packages.
- Market tests reference the Market project and repository test packages.
