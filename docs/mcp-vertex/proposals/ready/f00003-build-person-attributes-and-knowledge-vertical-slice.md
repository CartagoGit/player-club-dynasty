---
id: f00003
title: "Build person, attributes and knowledge vertical slice"
kind: feat
status: ready
type: proposal
track: people
date: 2026-08-11
---

# f00003 — Build person, attributes and knowledge vertical slice

## Goal

Implement a small, testable person model with compiled attributes, position-independent capability, observations and confidence-aware player summaries.

## why

The first product slice must make player information deep without making the core football-specific or omniscient.

## non-goals

- Career development, injuries, contracts, transfer market or full scouting staff.

## Slices

- global_gate: type

### S1 — Create people domain contracts
- **Status**: done
- **Files**: `game/src/PCDynasty.Core/People/Person.cs`, `game/src/PCDynasty.Core/People/PersonProfile.cs`, `game/tests/PCDynasty.Core.Tests/People/PersonTests.cs`
- **Gate**: type
- acceptance:
  - "Person identity and personal profile are immutable and sport-neutral."

### S2 — Implement attribute storage and derived indicators
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Core/Performance/CapabilityProfile.cs`, `game/src/PCDynasty.Core/Performance/CurrentPerformance.cs`, `game/tests/PCDynasty.Core.Tests/Performance/CurrentPerformanceTests.cs`
- **Gate**: type
- acceptance:
  - "Technique, form, physical and mental factors derive a current indicator without duplicating state."

### S3 — Implement observation and confidence contracts
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Knowledge/PCDynasty.Knowledge.csproj`, `game/src/PCDynasty.Knowledge/Observations/Observation.cs`, `game/tests/PCDynasty.Knowledge.Tests/PCDynasty.Knowledge.Tests.csproj`, `game/tests/PCDynasty.Knowledge.Tests/ObservationTests.cs`
- **Gate**: type
- acceptance:
  - "Knowledge has source, date, uncertainty and does not expose hidden truth."

## acceptance

- Person identity and personal profile are immutable and sport-neutral.
- Technique, form, physical and mental factors derive a current indicator without duplicating state.
- Knowledge has source, date, uncertainty and does not expose hidden truth.
