---
id: f00039
title: "Connect the manager career and club authority"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00032, f00033, f00035, f00038
---

# f00039 — Connect the manager career and club authority

## Goal

Make the manager a person with an identity, a contract, reputation, relationships,
objectives, offers, dismissal, resignation, club changes and unemployment, and let that
career decide which club the player is allowed to mutate.

## why

`PCDynasty.Career` models reputation, relationships, contracts and offers and has no
consumer; the visible game has a single profile, never lets the player choose a manager and
keeps career loading disabled. Authority today is implicit in the browsed club, which is
exactly the defect `x00041` contains but cannot fully resolve until the manager career
exists.

## non-goals

- A press or narrative system.
- Multiplayer or shared careers.

## Slices

- global_gate: e2e

### S1 — Create the manager identity and contract
- **Status**: ready
- **Files**: `game/src/PCDynasty.Career/Manager/`, `game/src/PCDynasty.Application/Careers/ManagerCareerService.cs`, `game/godot/Scripts/Shell/NewCareerScreen.cs`, `game/tests/PCDynasty.Career.Tests/ManagerCareerServiceTests.cs`
- **Gate**: unit
- acceptance:
  - "Starting a career picks a club and a manager and persists both atomically."
  - "The manager contract carries duration, objectives and terms."

### S2 — Run reputation, relationships, offers and dismissal
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Career/Relationships/`, `game/src/PCDynasty.Career/Offers/`, `game/godot/Scripts/Workspace/Screens/ManagerScreen.cs`, `game/tests/PCDynasty.Career.Tests/ManagerLifecycleTests.cs`
- **Gate**: e2e
- acceptance:
  - "Objective success and failure, offers, dismissal, resignation, unemployment and signing for another club are covered by tests."
  - "Changing club transfers command authority with no state leaking from the previous club."
  - "Save and load preserve reputation and relationships."
  - "SmokeBoot changes club and proves every action uses the new authority."

## acceptance

- The manager career governs which club can be mutated, and a club change leaks no state.
- New career and continue rebuild the same context.
- Reputation and relationships survive save and load.
