---
id: f00029
title: "Deliver a persistent playable transfer market in the Godot manager workspace"
kind: feat
status: done
type: proposal
track: product
date: 2026-08-18
shipped-in: [9986c9f]
---

# f00029 — Deliver a persistent playable transfer market in the Godot manager workspace

## Goal

Enable the managed club to browse deterministic transfer candidates, complete affordable signings, persist them in the externally supplied career database, immediately update transfer finances and squad membership, and restore the result when continuing a career.

## why

The classic-manager workspace still exposes Mercado as a disabled placeholder. A vertical slice must connect the existing market engine to career-scoped contracts, atomic SQLite settlement, squad projection and an original high-density Godot screen.

## non-goals

- Player sales, loans, agents and multi-round contract negotiation
- Remote or server-side persistence
- Changing the immutable starter-world content files
- Copying protected visuals or assets from PC Fútbol

## Slices

- global_gate: e2e

### S1 — Define engine-neutral transfer contracts and application use case
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/Careers/CareerTransferRecord.cs`, `game/src/PCDynasty.Contracts/Careers/ICareerTransferStore.cs`, `game/src/PCDynasty.Application/PCDynasty.Application.csproj`, `game/src/PCDynasty.Application/Market/CareerTransferMarketService.cs`, `game/src/PCDynasty.Application/Market/ICareerTransferMarketService.cs`, `game/src/PCDynasty.Application/Market/ICareerSquadProjection.cs`, `game/src/PCDynasty.Application/Market/TransferCandidate.cs`, `game/src/PCDynasty.Application/Market/TransferOfferResult.cs`, `game/tests/PCDynasty.Contracts.Tests/CareerTransferRecordTests.cs`, `game/tests/PCDynasty.Application.Tests/Market/CareerTransferMarketServiceTests.cs`
- **Gate**: type
- acceptance:
  - "Contracts are immutable, validate all external values and live outside engine and persistence assemblies."
  - "The application service depends only on IManagerWorld, contracts, TimeProvider and the existing PCDynasty.Market negotiation/valuation model."
  - "Candidates exclude the managed squad and already acquired players, remain deterministic, and expose an asking price."
  - "An accepted offer atomically requests settlement against the expected finance snapshot; rejected or stale settlements do not mutate career state."
  - "Tests cover affordability, below-asking rejection, duplicate prevention, finance decrement and acquired-squad projection."
  - "Squad consumers depend on a dedicated read-only projection port rather than the market command surface."
- review-state: done
- review-implementer: root
- review-reviewer: review_world_fix_fresh
- review-log: requested_changes by review_tactics — Determinism needs a total tie-break; affordability must also respect cash; public market records must reject impossible states; tests must exercise real duplicate and stale snapshots; split squad projection into a narrow read port.
- review-log: approved by review_world_fix_fresh — Fresh re-review approved: total deterministic ordering, cash-bounded affordability, validated immutable public models, real duplicate and optimistic stale tests, segregated squad projection; focused contracts/application/market/architecture and Release build green.
### S2 — Persist transfers and finance settlement atomically in external SQLite
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerTransferStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerTransferSchema.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerTransferReader.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerTransferWriter.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceWriter.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerTransferStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "The store receives the database path from the caller and never assumes a project-local database."
  - "A single SQLite transaction inserts the accepted transfer and updates the matching finance snapshot or rolls back both."
  - "Optimistic snapshot mismatch and duplicate player settlement return false without partial writes."
  - "Clear removes only career transfer rows and never unrelated external tables."
  - "Roundtrip, rollback, duplicate, clear and external-table preservation tests pass."
  - "Optimistic finance replacement is centralized in the existing finance writer rather than duplicated in transfer persistence."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Approved: caller-owned path, SRP persistence collaborators, immutable ordered reads, full-snapshot CAS plus unique insert in one transaction, stale/duplicate rollback, exact debit validation, isolated clear, centralized finance writer; SQLite 26/26 and Release build green.
### S3 — Connect the playable market and acquired squad to Godot
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Careers/CareerRuntime.cs`, `game/godot/Scripts/Careers/CareerRuntimeFactory.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`, `game/godot/Scripts/Workspace/Screens/MarketScreen.cs.uid`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Tests/SmokeBoot.gd`, `game/tests/PCDynasty.Presentation.Tests/CareerRuntimeFactoryTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "Mercado is enabled and navigable from the sidebar with an original dense classic-manager layout consistent with existing design tokens."
  - "The screen lists candidates and supports an affordable one-click signing with explicit success or rejection feedback."
  - "After signing, transfer budget and cash decrease, the candidate disappears, and the player appears in the managed squad."
  - "New career clears prior transfers while Continue restores transfer, squad and finance state from the same external database."
  - "Presentation depends on the application port, concrete SQLite construction remains only in the composition root, and no mutable static state or service locator is introduced."
  - "Godot smoke exercises New career, Mercado signing, Finanzas and Plantilla, then Continue and verifies the same persisted result without runtime errors."
  - "Release build, focused tests, architecture tests, repository hygiene and full Godot smoke pass."
- review-state: done
- review-implementer: root
- review-reviewer: review_world_fix_fresh
- review-log: requested_changes by review_tactics — Squad header uses original club.SquadSize after acquisitions; smoke must prove Market stays scoped after club browsing, Continue keeps acquired player out of candidates, and a subsequent New career clears prior transfers rather than only starting from an empty DB.
- review-log: approved by review_world_fix_fresh — Independent re-review approved: projected squad header is correct; managed-club market scope survives browsing; Continue preserves transfer exclusion; second New resets transfers, finances and squad. Application 9/9, SQLite 6/6, Presentation 49/49, Architecture 19/19, Godot Release 0/0, isolated SmokeBoot and diff-check green.
### S4 — Verify shipped delivery evidence and archive readiness
- **Status**: done
- **DependsOn**: [S3]
- **Files**: `docs/mcp-vertex/proposals/review/f00029-deliver-a-persistent-playable-transfer-market-in-the-godot-manager-workspace.md`
- **Gate**: e2e
- acceptance:
  - "The shipped-in reference identifies the commit containing the complete Godot transfer-market delivery."
  - "An independent reviewer confirms the complete acceptance suite remains green after the proposal enters review."
- review-state: done
- review-implementer: root
- review-reviewer: review_world_fix_fresh
- review-log: approved by review_world_fix_fresh — Approved archive evidence: 9986c9f resolves to the complete S3 delivery; independent 9/9 Application, 6/6 SQLite, 49/49 Presentation, 19/19 Architecture, Godot build and isolated smoke evidence matches that content with no functional drift.
## acceptance

- Contracts are immutable, validate all external values and live outside engine and persistence assemblies.
- The application service depends only on IManagerWorld, contracts, TimeProvider and the existing PCDynasty.Market negotiation/valuation model.
- Candidates exclude the managed squad and already acquired players, remain deterministic, and expose an asking price.
- An accepted offer atomically requests settlement against the expected finance snapshot; rejected or stale settlements do not mutate career state.
- Tests cover affordability, below-asking rejection, duplicate prevention, finance decrement and acquired-squad projection.
- Squad consumers depend on a dedicated read-only projection port rather than the market command surface.
- The store receives the database path from the caller and never assumes a project-local database.
- A single SQLite transaction inserts the accepted transfer and updates the matching finance snapshot or rolls back both.
- Optimistic snapshot mismatch and duplicate player settlement return false without partial writes.
- Clear removes only career transfer rows and never unrelated external tables.
- Roundtrip, rollback, duplicate, clear and external-table preservation tests pass.
- Optimistic finance replacement is centralized in the existing finance writer rather than duplicated in transfer persistence.
- Mercado is enabled and navigable from the sidebar with an original dense classic-manager layout consistent with existing design tokens.
- The screen lists candidates and supports an affordable one-click signing with explicit success or rejection feedback.
- After signing, transfer budget and cash decrease, the candidate disappears, and the player appears in the managed squad.
- New career clears prior transfers while Continue restores transfer, squad and finance state from the same external database.
- Presentation depends on the application port, concrete SQLite construction remains only in the composition root, and no mutable static state or service locator is introduced.
- Godot smoke exercises New career, Mercado signing, Finanzas and Plantilla, then Continue and verifies the same persisted result without runtime errors.
- Release build, focused tests, architecture tests, repository hygiene and full Godot smoke pass.
