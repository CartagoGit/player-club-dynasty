---
id: f00038
title: "Connect staff academy facilities and club evolution"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00031, f00032, f00033, f00035, f00037
---

# f00038 — Connect staff, academy, facilities and club evolution

## Goal

Make employees age, progress, retire and participate in the market; make the academy
generate, develop and promote people; give facilities and the stadium capacity, modernity,
decay, maintenance, works and closures; and let fanbase and demand respond to history, city
and performance, all through ports rather than screen calls.

## why

`PCDynasty.Club` has no productive consumer at all, so the club is a static row: it cannot
invest, cannot decay, cannot grow a youth player and cannot lose an employee. This is the
stage that makes a long career diverge between clubs, and it must arrive after the economy so
that every investment moves the same ledger.

## non-goals

- Building an art pipeline for stadiums; visuals stay original placeholders until `f00040`.
- Owning the manager's own career; `f00039` does.

## Slices

- global_gate: e2e

### S1 — Give employees a lifecycle and a market
- **Status**: ready
- **Files**: `game/src/PCDynasty.Club/Staff/`, `game/src/PCDynasty.Application/Clubs/StaffService.cs`, `game/godot/Scripts/Workspace/Screens/StaffScreen.cs`, `game/tests/PCDynasty.Club.Tests/StaffLifecycleTests.cs`
- **Gate**: unit
- acceptance:
  - "Employees age, progress, retire and are replaced across thirty simulated seasons without freezing."
  - "Hiring and dismissal move the club ledger and are persisted as facts."

### S2 — Run the academy as a source of people
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Club/Academy/`, `game/src/PCDynasty.Application/Clubs/AcademyService.cs`, `game/tests/PCDynasty.Club.Tests/AcademyIntakeTests.cs`
- **Gate**: unit
- acceptance:
  - "The academy generates, develops and promotes people on the career calendar."
  - "Promotion produces a real squad member with a contract and persisted facts."

### S3 — Model facilities, stadium and demand
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/src/PCDynasty.Club/Facilities/`, `game/src/PCDynasty.Club/Reputation/`, `game/godot/Scripts/Workspace/Screens/FacilitiesScreen.cs`, `game/tests/PCDynasty.Club.Tests/FacilityWorksTests.cs`
- **Gate**: e2e
- acceptance:
  - "Facilities decay over time; a work changes capacity and cost only once completed."
  - "Demand never exceeds sellable capacity and responds to history, city and performance."
  - "Save, load and rollback preserve every fact."
  - "SmokeBoot hires staff, starts a work and promotes a youth player."

## acceptance

- Thirty simulated seasons replace staff without freezing, preserve the budget and produce
  youth players.
- A facility work changes capacity and cost only after completion, and demand never exceeds
  sellable capacity.
- Every effect reaches the economy through ports, never from a screen.
