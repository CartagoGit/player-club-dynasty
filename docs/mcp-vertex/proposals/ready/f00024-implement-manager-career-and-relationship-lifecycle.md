---
id: f00024
title: "Implement manager career and relationship lifecycle"
kind: feat
status: ready
type: proposal
track: career
date: 2026-08-11
---

# f00024 — Implement manager career and relationship lifecycle

## Goal

Implement manager reputation, job relationships, offers, appointments, departures and history independently from direct club control.

## why

The product includes a career experience in which managers and staff can evolve across clubs.

## non-goals

- Online careers, social simulation or a full staff market UI.

## Slices

- global_gate: type

### S1 — Define career identity and reputation
- **Status**: done
- **Files**: `game/src/PCDynasty.Career/PCDynasty.Career.csproj`, `game/src/PCDynasty.Career/Manager/ManagerCareer.cs`, `game/tests/PCDynasty.Career.Tests/ManagerCareerTests.cs`
- **Gate**: type
- acceptance:
  - "Manager progression and history use durable facts."

### S2 — Implement organization relationships
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Career/Relationships/OrganizationRelationship.cs`, `game/src/PCDynasty.Career/Relationships/RelationshipLifecycle.cs`, `game/tests/PCDynasty.Career.Tests/RelationshipLifecycleTests.cs`
- **Gate**: type
- acceptance:
  - "Appointments and departures have explicit causes and effective dates."

### S3 — Implement offer and appointment resolver
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Career/Offers/CareerOfferResolver.cs`, `game/src/PCDynasty.Career/Offers/AppointmentDecision.cs`, `game/tests/PCDynasty.Career.Tests/CareerOfferResolverTests.cs`
- **Gate**: type
- acceptance:
  - "Offers are reproducible and respect reputation, contracts and club state."

## acceptance

- Manager progression and history use durable facts.
- Appointments and departures have explicit causes and effective dates.
- Offers are reproducible and respect reputation, contracts and club state.
