---
id: x00041
title: "Correct managed club identity and command authority"
kind: fix
status: review
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
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/Workspace/IManagerWorld.cs`, `game/src/PCDynasty.Application/Workspace/ClubRow.cs`, `game/src/PCDynasty.Application/Workspace/PlayerRow.cs`, `game/src/PCDynasty.Application/Workspace/ManagerRow.cs`, `game/src/PCDynasty.Application/Workspace/StaffRow.cs`, `game/src/PCDynasty.Application/Workspace/CompetitionRow.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldPeopleProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldCountryNames.cs`, `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldPeopleProjectionTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: unit
- acceptance:
  - "Squad projection filters by occupation, not by entity kind, and returns players only."
  - "Managers and staff have their own read models and are never rendered as squad rows."
  - "A fixture with eleven players and seven managers returns exactly eleven squad rows and zero managers."
  - "Each row type validates its own values and stays immutable."

### S2 — Introduce managed and browsed club contexts with explicit authority
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Workspace/ManagedClubContext.cs`, `game/src/PCDynasty.Application/Workspace/BrowsedClubContext.cs`, `game/src/PCDynasty.Application/Workspace/ClubAuthorityException.cs`, `game/src/PCDynasty.Application/Market/ICareerTransferMarketService.cs`, `game/src/PCDynasty.Application/Market/ICareerSquadProjection.cs`, `game/src/PCDynasty.Application/Market/CareerTransferMarketService.cs`, `game/src/PCDynasty.Application/Matches/ICareerLineupService.cs`, `game/src/PCDynasty.Application/Matches/CareerLineupService.cs`, `game/src/PCDynasty.Application/Finances/ICareerFinanceService.cs`, `game/src/PCDynasty.Application/Finances/CareerFinanceService.cs`, `game/src/PCDynasty.Application/Competitions/Simulation/ICompetitionSimulationService.cs`, `game/src/PCDynasty.Application/Competitions/Simulation/CompetitionSimulationService.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs`, `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`, `game/godot/Scripts/Workspace/Screens/FinancesScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Careers/CareerRuntimeFactory.cs`, `game/tests/PCDynasty.Application.Tests/Workspace/ManagedClubAuthorityTests.cs`
- **Gate**: unit
- acceptance:
  - "Every command surface accepts the managed club context only and rejects a browsed club."
  - "Tactics, finances and market resolve the managed club independently of what the user is browsing."
  - "An authority test proves a command against a rival club is rejected without mutating any store."

### S3 — Carry real country and birth date into the interface
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Workspace/CareerCalendar.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceUiFactory.cs`, `game/godot/Scripts/Careers/CareerRuntime.cs`, `game/godot/Scripts/Careers/CareerRuntimeFactory.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Tests/SmokeBoot.gd`, `game/tests/PCDynasty.Application.Tests/Workspace/CareerCalendarTests.cs`
- **Gate**: e2e
- acceptance:
  - "Age is computed from an injected career calendar, not from a hard-coded year, and is correct on both birthday boundaries."
  - "Country arrives in the read model and is never inferred from identifier substrings."
  - "SmokeBoot browses a rival club, returns to the managed club and shows unchanged state."
  - "The calendar is seeded from the injected clock until `f00033` replaces the source with the deterministic orchestrator; no other type needs to change then."

## acceptance

- A fixture with eleven players and seven managers renders exactly eleven squad rows
  in squad and tactics, and no manager ever appears in the market.
- Commands are rejected for any club that is not the managed one, proven by authority
  tests that assert no store mutation.
- Age is derived from the career date and validated on two boundary birthdays.
- SmokeBoot navigates a rival club and demonstrates its state does not change.

- review-implementer: claude-opus-5
- review-log: verified by claude-opus-5 — Starter-world fixture (11 players, 7 managers) now yields exactly 11 squad rows and 7 manager rows, squad size counts players only, and Aurora Quay's squad excludes its manager; authority tests prove a lineup, a finance open, a rival-only match and a rival season are all rejected with `ClubAuthorityException` and zero store writes; calendar tests cover the day before, the day of and the leap-day boundary; `WorldClubProjection` was split so the structural gate stayed at its limit instead of the limit being raised. Release build with analyzers, `dotnet format --verify-no-changes`, 27 test suites and headless SmokeBoot are green — the smoke now also asserts that a match plan stays on the managed club while a rival is browsed.
