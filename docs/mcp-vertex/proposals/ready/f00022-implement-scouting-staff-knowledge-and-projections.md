---
id: f00022
title: "Implement scouting staff knowledge and projections"
kind: feat
status: ready
type: proposal
track: knowledge
date: 2026-08-11
---

# f00022 — Implement scouting staff knowledge and projections

## Goal

Implement staff capabilities, regional knowledge, observation aging, scouting assignments and confidence-aware future projections.

## why

Non-omniscient information and useful professionals are core gameplay, not decorative UI.

## non-goals

- A global transfer market UI or real-world scouting data.

## Slices

- global_gate: type

### S1 — Define staff capability and knowledge domains
- **Status**: done
- **Files**: `game/src/PCDynasty.Knowledge/Staff/StaffKnowledgeProfile.cs`, `game/src/PCDynasty.Knowledge/Staff/KnowledgeDomain.cs`, `game/tests/PCDynasty.Knowledge.Tests/KnowledgeDomainTests.cs`
- **Gate**: type
- acceptance:
  - "Staff knowledge is regional/domain-specific and decoupled from person truth."

### S2 — Implement observation aging and fusion
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Knowledge/Observations/ObservationAging.cs`, `game/src/PCDynasty.Knowledge/Observations/KnowledgeFusion.cs`, `game/tests/PCDynasty.Knowledge.Tests/KnowledgeFusionTests.cs`
- **Gate**: type
- acceptance:
  - "Conflicting and stale observations produce explainable confidence estimates."

### S3 — Implement projections and scouting assignments
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Knowledge/Projections/PlayerProjection.cs`, `game/src/PCDynasty.Knowledge/Scouting/ScoutingAssignment.cs`, `game/tests/PCDynasty.Knowledge.Tests/PlayerProjectionTests.cs`
- **Gate**: type
- acceptance:
  - "Future estimates are probabilistic and traceable to available evidence."

## acceptance

- Staff knowledge is regional/domain-specific and decoupled from person truth.
- Conflicting and stale observations produce explainable confidence estimates.
- Future estimates are probabilistic and traceable to available evidence.
