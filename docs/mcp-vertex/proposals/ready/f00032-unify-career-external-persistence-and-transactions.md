---
id: f00032
title: "Unify career, external persistence and transactions"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00030
---

# f00032 — Unify career, external persistence and transactions

## Goal

Collapse the parallel save architectures into one contract family with a manifest, one
shared SQLite unit of work, multiple career slots stored in the user directory, and
asynchronous cancellable ports so no Godot handler performs input or output inside a
frame.

## why

Two similar save manifests exist, in `PCDynasty.Contracts` and in
`PCDynasty.Persistence.Abstractions`, while `IWorldStore`, `SaveMigrator`,
`CommandEnvelope` and the deterministic protocol have no consumer in the game. The
visible career instead writes five presentation-specific stores that each repeat lock,
open, directory creation and schema initialisation, with per-instance locks over a shared
database. `CareerSession` owns mutable history in the presentation layer and
`CareerHistoryPersistence.Save` mutates memory before writing and swallows exceptions as
warnings, so the interface can show a result that was never persisted. There is one slot,
no manifest, no backup and no recoverable migration, and default paths are relative to the
project rather than the user directory, which breaks an installed export.

## non-goals

- Designing the calendar or the orchestrator; `f00033` consumes this unit of work.
- Changing content formats; `f00030` owns the content pipeline.
- Remote or cloud saves.

## Slices

- global_gate: e2e

### S1 — Collapse to one save contract family and manifest
- **Status**: ready
- **Files**: `game/src/PCDynasty.Contracts/Persistence/SaveManifest.cs`, `game/src/PCDynasty.Persistence.Abstractions/`, `game/src/PCDynasty.Persistence/Migrations/SaveMigrator.cs`, `game/tests/PCDynasty.Persistence.Tests/SaveManifestTests.cs`
- **Gate**: unit
- acceptance:
  - "Exactly one manifest type exists and carries schema version, save version, root seed and content hashes."
  - "The duplicate manifest is deleted in this slice, not deprecated."
  - "A save produced by the previous schema migrates forward and preserves its golden hash."

### S2 — Share one transactional unit of work across career stores
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/SqliteUnitOfWork.cs`, `game/src/PCDynasty.Persistence.Sqlite/`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteUnitOfWorkTests.cs`
- **Gate**: unit
- acceptance:
  - "Career creation, reset, migration and fact commits are atomic across every store."
  - "The five stores share one connection, one lock and one schema initialisation."
  - "Fault injection at each write leaves the database complete before or after, never mixed."

### S3 — Own career state outside presentation, asynchronously
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/src/PCDynasty.Application/Careers/CareerSessionService.cs`, `game/src/PCDynasty.Presentation.Core/CareerSession.cs`, `game/godot/Scripts/Careers/CareerRuntime.cs`, `game/tests/PCDynasty.Application.Tests/Careers/CareerSessionServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "Presentation no longer owns mutable career history; it renders a projection it cannot mutate."
  - "A failed write surfaces as an explicit error state and never as a silently accepted result."
  - "Every persistence port is asynchronous and cancellable, and each Godot handler completes its synchronous part within 8 ms without input or output."

### S4 — Ship career slots, backup and external database selection
- **Status**: ready
- **DependsOn**: [S3]
- **Files**: `game/godot/Scripts/Shell/CareerSlotPicker.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/src/PCDynasty.Application/Careers/CareerCatalog.cs`, `game/tests/PCDynasty.Application.Tests/Careers/CareerCatalogTests.cs`
- **Gate**: e2e
- acceptance:
  - "Saves default to the user directory, support several slots, backup and an external database override."
  - "Continue career is enabled and restores the exact world, date and facts."
  - "Two slots and an external database never contaminate each other."
  - "Loading a 16 MiB checkpoint with 10,000 facts completes within 500 ms."

## acceptance

- One save contract family and one manifest carry schema version, save version, seed and
  content hashes.
- Fault injection at every write leaves a complete before or after state.
- Save, load and migration preserve the golden hash across schema versions.
- Two slots plus an external database stay isolated, and a 16 MiB checkpoint loads within
  500 ms while no Godot handler blocks more than 8 ms.
