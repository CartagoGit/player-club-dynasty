---
id: f00004
title: "Create the minimal FootballPack and player read models"
kind: feat
status: ready
type: proposal
track: football
date: 2026-08-11
---

# f00004 — Create the minimal FootballPack and player read models

## Goal

Implement the smallest football adapter that maps its attribute catalogue, roles and relevance ordering onto the sport-neutral Core.

## why

Football must prove the pack boundary without defining football concepts in Core.

## non-goals

- Complete football tactics, licensed data, match simulation or Godot scenes.

## Slices

- global_gate: type

### S1 — Define football attribute catalogue
- **Status**: done
- **Files**: `game/src/PCDynasty.Football/PCDynasty.Football.csproj`, `game/src/PCDynasty.Football/Attributes/FootballAttributeCatalog.cs`, `game/tests/PCDynasty.Football.Tests/PCDynasty.Football.Tests.csproj`, `game/tests/PCDynasty.Football.Tests/FootballAttributeCatalogTests.cs`
- **Gate**: type
- acceptance:
  - "All football players have the complete catalogue with compiled IDs."

### S2 — Define position familiarity and role relevance
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Football/Positions/PositionFamiliarity.cs`, `game/src/PCDynasty.Football/Positions/RoleRelevance.cs`, `game/tests/PCDynasty.Football.Tests/PositionFamiliarityTests.cs`
- **Gate**: type
- acceptance:
  - "Positions describe familiarity and relevance, never structural attribute ownership."

### S3 — Produce player summary read models
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/src/PCDynasty.Application/Players/PlayerSummaryQuery.cs`, `game/src/PCDynasty.Application/Players/PlayerSummaryView.cs`, `game/tests/PCDynasty.Application.Tests/PCDynasty.Application.Tests.csproj`, `game/tests/PCDynasty.Application.Tests/PlayerSummaryQueryTests.cs`
- **Gate**: type
- acceptance:
  - "The view explains indicators and confidence without leaking Godot types."

## acceptance

- All football players have the complete catalogue with compiled IDs.
- Positions describe familiarity and relevance, never structural attribute ownership.
- The view explains indicators and confidence without leaking Godot types.
