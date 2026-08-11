---
id: f00018
title: "Create player club and lineup interaction vertical slice"
kind: feat
status: ready
type: proposal
track: presentation
date: 2026-08-11
---

# f00018 — Create player club and lineup interaction vertical slice

## Goal

Build original Godot screens for a player profile, club overview and lineup selection using application read models and commands.

## why

This validates the manager's core daily flow and the classic quick-navigation UX without moving rules into UI.

## non-goals

- Full transfer UI, editor tooling or copied legacy screens.

## Slices

- global_gate: e2e

### S1 — Build player profile screen
- **Status**: done
- **Files**: `game/godot/Scenes/Players/PlayerProfile.tscn`, `game/godot/Scripts/Players/PlayerProfileController.cs`, `game/godot/Scripts/Players/PlayerMetricDetailPanel.cs`
- **Gate**: type
- acceptance:
  - "Shows summary then reasons, history and confidence through read models."

### S2 — Build club overview screen
- **Status**: pending
- **Files**: `game/godot/Scenes/Clubs/ClubOverview.tscn`, `game/godot/Scripts/Clubs/ClubOverviewController.cs`, `game/godot/Scripts/Clubs/ClubMetricGrid.cs`
- **Gate**: type
- acceptance:
  - "Dense club information remains scannable and uses shared components."

### S3 — Build lineup command flow
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/Scenes/Matches/LineupSelection.tscn`, `game/godot/Scripts/Matches/LineupSelectionController.cs`, `game/godot/Scripts/Matches/LineupCommandMapper.cs`
- **Gate**: e2e
- acceptance:
  - "UI validates interaction locally then sends application commands; no match rule lives in scene code."

## acceptance

- Shows summary then reasons, history and confidence through read models.
- Dense club information remains scannable and uses shared components.
- UI validates interaction locally then sends application commands; no match rule lives in scene code.
