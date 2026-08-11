---
id: f00026
title: "Create fictional starter world content pack"
kind: feat
status: done
type: proposal
track: content
date: 2026-08-11
---

# f00026 — Create fictional starter world content pack

## Goal

Create a small fictional football world pack with original names, clubs, people and competition data for end-to-end development.

## why

The vertical slices need legal, reproducible content without depending on unlicensed real-world databases.

## non-goals

- Real clubs, player likenesses, logos, imported community databases or eighty-country launch content.

## Slices

- global_gate: e2e

### S1 — Define starter pack manifest and source data
- **Status**: done
- **Files**: `game/content/starter-world/pack.json`, `game/content/starter-world/data/countries.json`, `game/content/starter-world/data/clubs.json`
- **Gate**: none
- acceptance:
  - "Content declares license, version, dependencies and fictional origin."

### S2 — Add fictional people and competition data
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/content/starter-world/data/people.json`, `game/content/starter-world/data/competitions.json`, `game/content/starter-world/data/rules.json`
- **Gate**: none
- acceptance:
  - "Data validates through the public import contract and contains no protected identities."

### S3 — Add content import integration scenario
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/tests/PCDynasty.Content.Integration.Tests/StarterWorldImportTests.cs`, `game/tests/PCDynasty.Content.Integration.Tests/Fixtures/StarterWorldExpected.json`
- **Gate**: e2e
- acceptance:
  - "The starter world imports to an immutable reproducible pack."

## acceptance

- Content declares license, version, dependencies and fictional origin.
- Data validates through the public import contract and contains no protected identities.
- The starter world imports to an immutable reproducible pack.
