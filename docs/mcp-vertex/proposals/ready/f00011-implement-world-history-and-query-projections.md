---
id: f00011
title: "Implement world history and query projections"
kind: feat
status: ready
type: proposal
track: world
date: 2026-08-11
---

# f00011 — Implement world history and query projections

## Goal

Implement compact historical facts and query projections for careers, titles, transfers, records and competition history.

## why

Decades of emergent stories need durable, explainable history without retaining every transient calculation.

## non-goals

- A social feed, narrative scripting system or presentation scenes.

## Slices

- global_gate: type

### S1 — Define historical event contracts
- **Status**: done
- **Files**: `game/src/PCDynasty.History/PCDynasty.History.csproj`, `game/src/PCDynasty.History/Events/HistoricalEvent.cs`, `game/tests/PCDynasty.History.Tests/PCDynasty.History.Tests.csproj`, `game/tests/PCDynasty.History.Tests/HistoricalEventTests.cs`
- **Gate**: type
- acceptance:
  - "History records durable facts with compact indices."

### S2 — Create history projections
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.History/Projections/CareerHistoryProjection.cs`, `game/src/PCDynasty.History/Projections/CompetitionHistoryProjection.cs`, `game/tests/PCDynasty.History.Tests/HistoryProjectionTests.cs`
- **Gate**: type
- acceptance:
  - "Queries reconstruct career and competition histories deterministically."

### S3 — Expose application history queries
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/src/PCDynasty.Application/History/HistoryQuery.cs`, `game/src/PCDynasty.Application/History/HistoryView.cs`, `game/tests/PCDynasty.Application.Tests/HistoryQueryTests.cs`, `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Read models stay independent from storage and Godot."

## acceptance

- History records durable facts with compact indices.
- Queries reconstruct career and competition histories deterministically.
- Read models stay independent from storage and Godot.
