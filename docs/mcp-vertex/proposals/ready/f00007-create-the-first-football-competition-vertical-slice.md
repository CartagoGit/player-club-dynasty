---
id: f00007
title: "Create the first football competition vertical slice"
kind: feat
status: ready
type: proposal
track: football
date: 2026-08-11
---

# f00007 — Create the first football competition vertical slice

## Goal

Implement one country with two football divisions, promotions, relegations and deterministic season closure using the generic competition contracts.

## why

This is the smallest proof that rules, standings and transitions can support a real manager world.

## non-goals

- Eighty countries, continental cups, a visual competition editor or a fully detailed market.

## Slices

- global_gate: e2e

### S1 — Define football points and season rules
- **Status**: pending
- **Files**: `game/src/PCDynasty.Football/PCDynasty.Football.csproj`, `game/src/PCDynasty.Football/Competitions/FootballScoringRules.cs`, `game/src/PCDynasty.Football/Competitions/DivisionRuleSet.cs`, `game/tests/PCDynasty.Football.Tests/DivisionRuleSetTests.cs`
- **Gate**: type
- acceptance:
  - "Football scoring is supplied by the pack and respects rule versions."

### S2 — Implement promotion and relegation resolver
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Football/Competitions/PromotionRelegationResolver.cs`, `game/src/PCDynasty.Football/Competitions/SeasonClosure.cs`, `game/tests/PCDynasty.Football.Tests/PromotionRelegationResolverTests.cs`
- **Gate**: type
- acceptance:
  - "Thousands of generated seasons preserve participant and outcome invariants."

### S3 — Expose season result read models
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/src/PCDynasty.Application/Competitions/SeasonTableQuery.cs`, `game/src/PCDynasty.Application/Competitions/SeasonTableView.cs`, `game/tests/PCDynasty.Application.Tests/SeasonTableQueryTests.cs`
- **Gate**: type
- acceptance:
  - "Presentation receives read models rather than competition entities."

## acceptance

- Football scoring is supplied by the pack and respects rule versions.
- Thousands of generated seasons preserve participant and outcome invariants.
- Presentation receives read models rather than competition entities.
