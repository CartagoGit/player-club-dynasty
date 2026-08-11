---
id: f00006
title: "Define generic competition graphs and versioned rules"
kind: feat
status: ready
type: proposal
track: competition
date: 2026-08-11
---

# f00006 — Define generic competition graphs and versioned rules

## Goal

Implement sport-neutral competition stages, participation, rankings and effective-dated rules as validated graphs.

## why

League is not the generic competition abstraction; scalable seasons need graphs and versioned rules.

## non-goals

- A full set of football competitions, a visual editor or continental qualification.

## Slices

- global_gate: type

### S1 — Create competition graph contracts
- **Status**: pending
- **Files**: `game/src/PCDynasty.Competition/PCDynasty.Competition.csproj`, `game/src/PCDynasty.Competition/Stages/CompetitionGraph.cs`, `game/tests/PCDynasty.Competition.Tests/PCDynasty.Competition.Tests.csproj`, `game/tests/PCDynasty.Competition.Tests/CompetitionGraphTests.cs`
- **Gate**: type
- acceptance:
  - "Stages and transitions validate without league-specific concepts."

### S2 — Implement effective-dated rules
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Rules/PCDynasty.Rules.csproj`, `game/src/PCDynasty.Rules/Versioning/EffectiveRuleSet.cs`, `game/tests/PCDynasty.Rules.Tests/PCDynasty.Rules.Tests.csproj`, `game/tests/PCDynasty.Rules.Tests/EffectiveRuleSetTests.cs`, `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Rule selection is explicit by date and version."

### S3 — Implement ranking and outcome contracts
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Competition/Rankings/RankingTable.cs`, `game/src/PCDynasty.Competition/Outcomes/QualificationOutcome.cs`, `game/tests/PCDynasty.Competition.Tests/RankingTableTests.cs`
- **Gate**: type
- acceptance:
  - "Ranking and qualification remain independent from sport scoring details."

## acceptance

- Stages and transitions validate without league-specific concepts.
- Rule selection is explicit by date and version.
- Ranking and qualification remain independent from sport scoring details.
