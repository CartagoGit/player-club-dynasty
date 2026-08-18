---
id: f00021
title: "Implement facilities academy staff and club evolution"
kind: feat
status: done
type: proposal
track: club
date: 2026-08-11
---

# f00021 — Implement facilities academy staff and club evolution

## Goal

Implement infrastructure quality, condition, modernization, academy, staff effects, prestige and fanbase as long-term club systems.

## why

Club management must create durable sporting and financial consequences beyond transfers and tactics.

## non-goals

- 3D stadium construction, a complete human resources UI or all commercial systems.

## Slices

- global_gate: type

### S1 — Define facilities and maintenance domain
- **Status**: done
- **Files**: `game/src/PCDynasty.Club/PCDynasty.Club.csproj`, `game/src/PCDynasty.Club/Facilities/FacilityState.cs`, `game/tests/PCDynasty.Club.Tests/FacilityStateTests.cs`
- **Gate**: type
- acceptance:
  - "Quality, condition and modernization are distinct with explicit maintenance effects."

### S2 — Implement academy and youth generation
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Club/Academy/AcademyState.cs`, `game/src/PCDynasty.Club/Academy/YouthGenerator.cs`, `game/tests/PCDynasty.Club.Tests/YouthGeneratorTests.cs`
- **Gate**: type
- acceptance:
  - "Youth output derives from facilities, region and time with controlled randomness."

### S3 — Implement staff and club evolution effects
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Club/Staff/StaffEffectResolver.cs`, `game/src/PCDynasty.Club/Reputation/ClubEvolution.cs`, `game/tests/PCDynasty.Club.Tests/ClubEvolutionTests.cs`
- **Gate**: type
- acceptance:
  - "Staff, prestige and fanbase affect systems through reusable policies."

## acceptance

- Quality, condition and modernization are distinct with explicit maintenance effects.
- Youth output derives from facilities, region and time with controlled randomness.
- Staff, prestige and fanbase affect systems through reusable policies.
