---
id: r00005
title: "Move career persistence behind engine-neutral contracts"
kind: refactor
status: done
type: proposal
track: architecture
date: 2026-08-18
---

# r00005 — Move career persistence behind engine-neutral contracts

## Goal

Remove SQLite and save-file ownership from Presentation.Core by defining engine-neutral career persistence contracts and implementing them in the SQLite adapter assembly.

## why

Presentation.Core currently depends directly on Microsoft.Data.Sqlite and owns schema, filesystem and connection concerns. This violates dependency inversion, makes the UI core hard to replace or test, and couples the future manager engine to one storage technology.

## non-goals

- Change the existing SQLite schema or invalidate current career.sqlite files.
- Replace the canonical world snapshot store.
- Introduce asynchronous background saves in this slice.

## Slices

- global_gate: e2e

### S1 — Extract career save contracts and SQLite adapters
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/Careers/CareerProfile.cs`, `game/src/PCDynasty.Contracts/Careers/HistoryEntry.cs`, `game/src/PCDynasty.Contracts/Careers/SeasonHistoryEntry.cs`, `game/src/PCDynasty.Contracts/Careers/ICareerHistoryStore.cs`, `game/src/PCDynasty.Contracts/Careers/ICareerProfileStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerHistoryStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerProfileStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/PCDynasty.Persistence.Sqlite.csproj`, `game/src/PCDynasty.Presentation.Core/CareerStore.cs`, `game/src/PCDynasty.Presentation.Core/CareerProfileStore.cs`, `game/src/PCDynasty.Presentation.Core/PCDynasty.Presentation.Core.csproj`, `game/src/PCDynasty.Presentation.Core/CareerSession.cs`, `game/src/PCDynasty.Presentation.Core/MatchHistory.cs`, `game/src/PCDynasty.Presentation.Core/SeasonHistory.cs`, `game/src/PCDynasty.Presentation.Core/SeasonSimulator.cs`, `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Workspace/Persistence/CareerHistoryPersistence.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/godot/Scripts/Workspace/Screens/SeasonReportView.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`, `game/tests/PCDynasty.Contracts.Tests/CareerPersistenceContractTests.cs`, `game/tests/PCDynasty.Presentation.Tests/CareerSessionTests.cs`, `game/tests/PCDynasty.Presentation.Tests/HistoryFilterTests.cs`, `game/tests/PCDynasty.Presentation.Tests/MatchHistoryTests.cs`, `game/tests/PCDynasty.Presentation.Tests/SeasonHistoryTests.cs`, `game/tests/PCDynasty.Presentation.Tests/CareerStoreTests.cs`, `game/tests/PCDynasty.Presentation.Tests/CareerProfileStoreTests.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerHistoryStoreTests.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerProfileStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "PCDynasty.Contracts owns immutable career profile, match history, season history and standing contracts plus persistence interfaces."
  - "PCDynasty.Persistence.Sqlite implements those interfaces without Presentation.Core references."
  - "Presentation.Core has no Microsoft.Data.Sqlite package or SQLite API usage."
  - "Godot composes the SQLite adapter while workspace persistence depends only on ICareerHistoryStore."
  - "Existing career.sqlite tables remain readable and writable."
  - "Persistence tests live with the SQLite adapter and the complete .NET and Godot gates remain green."
- review-state: done
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: approved by delivery-verifier — Independent review approved: immutable Contracts ports own career DTOs, SQLite adapters have no Presentation.Core dependency, Presentation.Core has no SQLite API/package, schema compatibility is preserved, and Contracts/SQLite/Architecture/Godot smoke gates pass.
## acceptance

- PCDynasty.Contracts owns immutable career profile, match history, season history and standing contracts plus persistence interfaces.
- PCDynasty.Persistence.Sqlite implements those interfaces without Presentation.Core references.
- Presentation.Core has no Microsoft.Data.Sqlite package or SQLite API usage.
- Godot composes the SQLite adapter while workspace persistence depends only on ICareerHistoryStore.
- Existing career.sqlite tables remain readable and writable.
- Persistence tests live with the SQLite adapter and the complete .NET and Godot gates remain green.
