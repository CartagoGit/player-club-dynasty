---
id: f00036
title: "Connect the player dossier knowledge and scouting"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: x00041, f00031, f00032, f00033
---

# f00036 — Connect the player dossier, knowledge and scouting

## Goal

Open the player drill-down from the squad, keep truth and perception separated across the
interface contract, and make scouting productive: assignments, regions, scout quality and
workload, report progression and expiry, with signing decisions consuming permitted
knowledge only.

## why

`PCDynasty.Knowledge` models observation with confidence, source and date and has no
productive consumer; the scouting button is disabled and squad rows do not open the
existing dossier scene. Without this stage the manager cannot make an informed decision,
and the uncertainty the domain was designed around is invisible.

## non-goals

- Revealing hidden potential anywhere in the interface.
- Owning training effects; `f00037` does.
- Adding a scouting narrative or press system.

## Slices

- global_gate: e2e

### S1 — Open the dossier from the squad with perceived values only
- **Status**: ready
- **Files**: `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scenes/Players/PlayerProfile.tscn`, `game/src/PCDynasty.Application/Players/PlayerSummaryQuery.cs`, `game/tests/PCDynasty.Application.Tests/Players/PerceivedValueTests.cs`
- **Gate**: unit
- acceptance:
  - "A squad row opens the dossier for that player."
  - "The dossier shows estimate, range, confidence, source and date, never a hidden true value."
  - "Two clubs perceive the same player differently in a deterministic fixture."

### S2 — Make scouting a productive scheduled system
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Knowledge/Scouting/`, `game/src/PCDynasty.Application/Scouting/ScoutingService.cs`, `game/godot/Scripts/Workspace/Screens/ScoutingScreen.cs`, `game/tests/PCDynasty.Application.Tests/Scouting/ScoutingServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "Assignments, regions, scout quality and workload drive report progression and expiry on the career calendar."
  - "Signing decisions read permitted knowledge only; the true value never crosses the interface contract."
  - "Save and load preserve observations and their confidence."
  - "An indexed knowledge query over 160,000 people holds a median at or below 16 ms."
  - "SmokeBoot assigns a scout, advances the date and opens an updated report."

## acceptance

- Two clubs demonstrably perceive the same player differently and hidden truth never
  reaches the interface.
- Observations survive save and load with their confidence intact.
- Indexed knowledge queries over 160,000 people stay at or below a 16 ms median.
