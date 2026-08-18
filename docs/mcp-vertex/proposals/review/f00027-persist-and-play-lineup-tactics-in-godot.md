---
id: f00027
title: "Persist and play lineup tactics in Godot"
kind: feat
status: ready
type: proposal
track: gameplay
date: 2026-08-18
---

# f00027 — Persist and play lineup tactics in Godot

## Goal

Enable the manager tactics route with a reusable lineup-selection flow whose latest plan is validated in Application and stored in the caller-selected external career database.

## why

The workspace currently exposes a disabled tactics button even though lineup command components already exist. Connecting them through contracts and persistence makes the classic manager workflow genuinely usable without placing career state in Godot.

## non-goals

- Build a graphical pitch editor.
- Add substitutions during a live match.
- Make tactics alter the contest simulation model in this slice.

## Slices

- global_gate: e2e

### S1 — Connect persisted lineup plans to the manager workspace
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/Careers/LineupPlan.cs`, `game/src/PCDynasty.Contracts/Careers/ICareerLineupStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerLineupStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerLineupSchema.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerLineupReader.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerLineupWriter.cs`, `game/src/PCDynasty.Application/Matches/ICareerLineupService.cs`, `game/src/PCDynasty.Application/Matches/CareerLineupService.cs`, `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/godot/Scenes/Main.tscn`, `game/godot/Scenes/Matches/LineupSelection.tscn`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Matches/LineupSelectionController.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs`, `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs.uid`, `game/godot/Tests/SmokeBoot.gd`, `game/tests/PCDynasty.Contracts.Tests/LineupPlanTests.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerLineupStoreTests.cs`, `game/tests/PCDynasty.Application.Tests/Matches/CareerLineupServiceTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "An immutable external LineupPlan contract validates unique participants, assignments and order."
  - "The SQLite adapter round-trips one latest lineup per career club without depending on Godot or Presentation.Core."
  - "CareerLineupService accepts SubmitLineupCommand and exposes the latest saved plan through interfaces."
  - "The existing lineup selector is reused inside a focused TacticsScreen rather than duplicated."
  - "The tactics sidebar route is enabled and survives reopening through the external career database."
  - "Godot workspace contains no SQLite or schema code."
  - "Application, persistence, architecture and Godot smoke tests remain green."

## acceptance

- An immutable external LineupPlan contract validates unique participants, assignments and order.
- The SQLite adapter round-trips one latest lineup per career club without depending on Godot or Presentation.Core.
- CareerLineupService accepts SubmitLineupCommand and exposes the latest saved plan through interfaces.
- The existing lineup selector is reused inside a focused TacticsScreen rather than duplicated.
- The tactics sidebar route is enabled and survives reopening through the external career database.
- Godot workspace contains no SQLite or schema code.
- Application, persistence, architecture and Godot smoke tests remain green.
