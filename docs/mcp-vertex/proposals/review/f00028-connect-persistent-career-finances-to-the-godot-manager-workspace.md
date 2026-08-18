---
id: f00028
title: "Connect persistent career finances to the Godot manager workspace"
kind: feat
status: review
type: proposal
track: manager-workspace
date: 2026-08-18
---

# f00028 — Connect persistent career finances to the Godot manager workspace

## Goal

Enable a functional, instant finance office in Godot backed by an engine-neutral application service and an externally loadable SQLite career store.

## why

The finance navigation is still disabled even though the economy ledger exists. Managers need a persistent budget, cash, revenue, cost and debt overview that participates in the same career lifecycle as history and lineups.

## non-goals

- Implement transfer-market negotiations or scouting.
- Add sponsorship, ticket pricing or loan interaction forms in this increment.
- Couple Godot UI code directly to SQLite or economy domain entities.

## Slices

- global_gate: e2e

### S1 — Define finance contracts and application service
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/Careers/CareerFinanceState.cs`, `game/src/PCDynasty.Contracts/Careers/ICareerFinanceStore.cs`, `game/src/PCDynasty.Application/Finances/ClubFinanceOverview.cs`, `game/src/PCDynasty.Application/Finances/ICareerFinanceService.cs`, `game/src/PCDynasty.Application/Finances/CareerFinanceService.cs`, `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/tests/PCDynasty.Contracts.Tests/CareerFinanceStateTests.cs`, `game/tests/PCDynasty.Application.Tests/Finances/CareerFinanceServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "Finance persistence contracts contain immutable engine-neutral values and no Godot, SQLite or presentation dependency."
  - "The application service creates a deterministic opening ledger from the selected fictional club, persists it through ICareerFinanceStore and returns a focused overview."
  - "New and continued career behavior is explicit and covered without static or service-locator state."
- review-state: in_review
- review-implementer: root
### S2 — Persist career finance state in external SQLite
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceSchema.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceReader.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceWriter.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerFinanceStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "The adapter implements ICareerFinanceStore against the caller-provided database path and preserves existing career tables."
  - "Schema, reading and transactional writing remain focused internal collaborators."
  - "Round-trip, replace and clear semantics are tested against temporary external SQLite databases."
- review-state: in_review
- review-implementer: root
### S3 — Enable and render the finance office in Godot
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Careers/CareerRuntime.cs`, `game/godot/Scripts/Careers/CareerRuntimeFactory.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/FinancesScreen.cs`, `game/godot/Scripts/Workspace/Screens/FinancesScreen.cs.uid`, `game/godot/Tests/SmokeBoot.gd`, `game/tests/PCDynasty.Presentation.Tests/CareerRuntimeFactoryTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "FINANZAS is enabled and navigates to an original classic-manager office showing cash, transfer budget, revenue, costs and debt for the selected club."
  - "New career resets finance state while Continue restores it from the same external career database."
  - "Godot depends only on ICareerFinanceService at the screen boundary; SQLite composition stays in CareerRuntimeFactory."
  - "SmokeBoot opens the finance route and verifies meaningful rendered values without duplicate engine instances or visible loading."
- review-state: changes_requested
- review-implementer: root
- review-reviewer: review_tactics
- review-log: requested_changes by review_tactics — Changing the browsed club then opening Finances requests an uninitialized finance state. Preserve the managed career club for finances or explicitly guard the route, and cover Club Picker to Finances.
## acceptance

- Finance persistence contracts contain immutable engine-neutral values and no Godot, SQLite or presentation dependency.
- The application service creates a deterministic opening ledger from the selected fictional club, persists it through ICareerFinanceStore and returns a focused overview.
- New and continued career behavior is explicit and covered without static or service-locator state.
- The adapter implements ICareerFinanceStore against the caller-provided database path and preserves existing career tables.
- Schema, reading and transactional writing remain focused internal collaborators.
- Round-trip, replace and clear semantics are tested against temporary external SQLite databases.
- FINANZAS is enabled and navigates to an original classic-manager office showing cash, transfer budget, revenue, costs and debt for the selected club.
- New career resets finance state while Continue restores it from the same external career database.
- Godot depends only on ICareerFinanceService at the screen boundary; SQLite composition stays in CareerRuntimeFactory.
- SmokeBoot opens the finance route and verifies meaningful rendered values without duplicate engine instances or visible loading.
