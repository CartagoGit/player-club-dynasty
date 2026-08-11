---
id: f00019
title: "Implement people careers development and injuries"
kind: feat
status: ready
type: proposal
track: people
date: 2026-08-11
---

# f00019 — Implement people careers development and injuries

## Goal

Implement long-term person development, biological wear, career transitions, injuries and recovery with lazy advancement and durable facts.

## why

Emergent careers need believable changes without fixed visible potential or daily simulation of everyone.

## non-goals

- A complete life simulator, staff market or player contract negotiations.

## Slices

- global_gate: type

### S1 — Define development and biological profiles
- **Status**: done
- **Files**: `game/src/PCDynasty.Careers/PCDynasty.Careers.csproj`, `game/src/PCDynasty.Careers/Development/BiologicalProfile.cs`, `game/tests/PCDynasty.Careers.Tests/BiologicalProfileTests.cs`
- **Gate**: type
- acceptance:
  - "Development uses talent, work, opportunity and biological factors without a fixed public potential."

### S2 — Implement lazy development advancement
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Careers/Development/DevelopmentAdvancer.cs`, `game/src/PCDynasty.Careers/Development/CareerWear.cs`, `game/tests/PCDynasty.Careers.Tests/DevelopmentAdvancerTests.cs`
- **Gate**: type
- acceptance:
  - "Elapsed time advances deterministically on demand with no daily world-wide loop."

### S3 — Implement injuries and recovery states
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Careers/Injuries/InjuryInstance.cs`, `game/src/PCDynasty.Careers/Injuries/RecoveryAdvancer.cs`, `game/tests/PCDynasty.Careers.Tests/RecoveryAdvancerTests.cs`
- **Gate**: type
- acceptance:
  - "Medical fitness and competitive readiness remain distinct and persist durable consequences."

## acceptance

- Development uses talent, work, opportunity and biological factors without a fixed public potential.
- Elapsed time advances deterministically on demand with no daily world-wide loop.
- Medical fitness and competitive readiness remain distinct and persist durable consequences.
