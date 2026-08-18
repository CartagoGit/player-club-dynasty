---
id: x00041
title: "Correct managed club identity and command authority"
kind: fix
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: c00005
---

# x00041 — Correct managed club identity and command authority

## Goal

Separate player, manager and staff read models, introduce an explicit managed-club
context distinct from the browsed club, and carry real country and birth date to the
interface so no screen invents data.

## why

The audit proved the visible squad is wrong: `WorldClubProjection.ListPlayers` selects
every person whose kind is `person` and who references the club, so the seven starter
managers appear as players with zero-valued attributes, while the occupation filter is
used only for the counter. `ManagerWorkspaceController` hands tactics, competitions,
finances and market the *browsed* club rather than the *managed* one, so authority is
implicit and a rival club can reach command surfaces. `WorkspaceUiFactory` hard-codes
the year 2026 to compute age and infers a player's country from substrings of the
identifier. These are the cheapest defects to fix and every later stage depends on the
identity being right.

## non-goals

- Introducing the full manager career lifecycle; `f00039` owns that.
- Replacing the content pipeline; `f00030` owns the canonical loader.
- Redesigning the workspace navigation; `f00040` owns the classic decision loop.

## Slices

- global_gate: e2e

### S1 — Split person read models by occupation
- **Status**: ready
- **Files**: `game/src/PCDynasty.Application/Workspace/IManagerWorld.cs`, `game/src/PCDynasty.Application/Workspace/PlayerRow.cs`, `game/src/PCDynasty.Application/Workspace/ManagerRow.cs`, `game/src/PCDynasty.Application/Workspace/StaffRow.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldClubProjectionTests.cs`
- **Gate**: unit
- acceptance:
  - "Squad projection filters by occupation, not by entity kind, and returns players only."
  - "Managers and staff have their own read models and are never rendered as squad rows."
  - "A fixture with eleven players and seven managers returns exactly eleven squad rows and zero managers."
  - "Each row type validates its own values and stays immutable."

### S2 — Introduce managed and browsed club contexts with explicit authority
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Workspace/ManagedClubContext.cs`, `game/src/PCDynasty.Application/Workspace/BrowsedClubContext.cs`, `game/src/PCDynasty.Application/Market/CareerTransferMarketService.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/tests/PCDynasty.Application.Tests/Workspace/ManagedClubAuthorityTests.cs`
- **Gate**: unit
- acceptance:
  - "Every command surface accepts the managed club context only and rejects a browsed club."
  - "Tactics, finances and market resolve the managed club independently of what the user is browsing."
  - "An authority test proves a command against a rival club is rejected without mutating any store."

### S3 — Carry real country and birth date into the interface
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/Components/WorkspaceUiFactory.cs`, `game/src/PCDynasty.Application/Players/PlayerSummaryView.cs`, `game/tests/PCDynasty.Presentation.Tests/WorkspaceUiFactoryTests.cs`
- **Gate**: e2e
- acceptance:
  - "Age is computed from the career date, not from a hard-coded year, and is correct on both birthday boundaries."
  - "Country arrives in the read model and is never inferred from identifier substrings."
  - "SmokeBoot browses a rival club, returns to the managed club and shows unchanged state."

## acceptance

- A fixture with eleven players and seven managers renders exactly eleven squad rows
  in squad and tactics, and no manager ever appears in the market.
- Commands are rejected for any club that is not the managed one, proven by authority
  tests that assert no store mutation.
- Age is derived from the career date and validated on two boundary birthdays.
- SmokeBoot navigates a rival club and demonstrates its state does not change.
