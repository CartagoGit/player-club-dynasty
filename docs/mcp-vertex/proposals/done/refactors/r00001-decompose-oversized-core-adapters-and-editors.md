---
id: r00001
title: "Decompose oversized core adapters and editors"
kind: refactor
status: done
type: proposal
track: architecture
date: 2026-08-18
---

# r00001 — Decompose oversized core adapters and editors

## Goal

Remove the remaining oversized multi-responsibility production files by extracting focused internal collaborators while preserving public contracts and runtime behavior.

## why

The current architecture gates protect dependency direction, but several legacy files still combine loading, projection, schema, persistence mapping, validation or multiple public types. This creates avoidable change coupling before finances, market and scouting are connected to Godot.

## non-goals

- Change public gameplay behavior or save formats.
- Introduce a new dependency injection framework.
- Redesign the domain contracts.

## Slices

- global_gate: e2e

### S1 — Separate world loading from UI projections
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/src/PCDynasty.Presentation.Core/WorldContentCatalog.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldCompetitionProjection.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldServiceTests.cs`, `game/tests/PCDynasty.Architecture.Tests/PresentationStructureTests.cs`
- **Gate**: e2e
- acceptance:
  - "WorldService remains the IManagerWorld facade while loading, club/player projection and competition projection move to focused internal collaborators."
  - "Public behavior and ordering remain compatible with existing tests."
  - "No productive C# file in the refactored WorldService cluster exceeds 180 lines."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Independent review: acceptance, contracts, SRP, focused tests, full Release suite and Godot smoke are green.
### S2 — Decompose the SQLite world store adapter
- **Status**: done
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/WorldStoreConcurrencyException.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldConnectionFactory.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldSchema.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldReader.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldWriter.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteWorldStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "SqliteWorldStore only orchestrates IWorldStore operations and the concurrency boundary."
  - "Connection/schema, reads and writes are isolated in internal collaborators."
  - "Concurrency, checkpoint and external database behavior remain unchanged."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Independent review: acceptance, contracts, SRP, focused tests, full Release suite and Godot smoke are green.
### S3 — Decompose career-history persistence
- **Status**: done
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerHistoryStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerHistorySchema.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerHistoryReader.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerHistoryWriter.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteCareerHistoryStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "The public history store is a small contract adapter."
  - "Schema, read mapping and transactional writes are separated."
  - "Existing external career databases and round-trip behavior remain compatible."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Independent review: acceptance, contracts, SRP, focused tests, full Release suite and Godot smoke are green.
### S4 — Give finance ledger types one file and one responsibility
- **Status**: done
- **Files**: `game/src/PCDynasty.Economy/Ledgers/ClubLedger.cs`, `game/src/PCDynasty.Economy/Ledgers/LedgerAccountKind.cs`, `game/src/PCDynasty.Economy/Ledgers/LedgerAccount.cs`, `game/src/PCDynasty.Economy/Ledgers/LedgerTransaction.cs`, `game/src/PCDynasty.Economy/Ledgers/AnnualClubFinancialState.cs`, `game/src/PCDynasty.Economy/Ledgers/LedgerValue.cs`, `game/tests/PCDynasty.Economy.Tests/ClubLedgerTests.cs`
- **Gate**: e2e
- acceptance:
  - "Each public ledger contract has its own canonically named file."
  - "ClubLedger retains only aggregate behavior and invariants."
  - "Public API and double-entry results do not change."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Independent review: acceptance, contracts, SRP, focused tests, full Release suite and Godot smoke are green.
### S5 — Separate competition graph editing, validation and pack building
- **Status**: done
- **Files**: `game/src/PCDynasty.Editor/Competitions/CompetitionGraphEditor.cs`, `game/src/PCDynasty.Editor/Competitions/EditableCompetitionStage.cs`, `game/src/PCDynasty.Editor/Competitions/EditableCompetitionTransition.cs`, `game/src/PCDynasty.Editor/Competitions/CompetitionGraphValidator.cs`, `game/src/PCDynasty.Editor/Competitions/CompetitionPackBuilder.cs`, `game/tests/PCDynasty.Editor.Tests/CompetitionGraphEditorTests.cs`
- **Gate**: e2e
- acceptance:
  - "The editor owns immutable edits while validation and pack construction use focused collaborators."
  - "Diagnostics and generated pack remain behaviorally compatible."
  - "No productive C# file in the refactored editor cluster exceeds 180 lines."
- review-state: done
- review-implementer: root
- review-reviewer: review_tactics
- review-log: approved by review_tactics — Independent review: acceptance, contracts, SRP, focused tests, full Release suite and Godot smoke are green.
## acceptance

- WorldService remains the IManagerWorld facade while loading, club/player projection and competition projection move to focused internal collaborators.
- Public behavior and ordering remain compatible with existing tests.
- No productive C# file in the refactored WorldService cluster exceeds 180 lines.
- SqliteWorldStore only orchestrates IWorldStore operations and the concurrency boundary.
- Connection/schema, reads and writes are isolated in internal collaborators.
- Concurrency, checkpoint and external database behavior remain unchanged.
- The public history store is a small contract adapter.
- Schema, read mapping and transactional writes are separated.
- Existing external career databases and round-trip behavior remain compatible.
- Each public ledger contract has its own canonically named file.
- ClubLedger retains only aggregate behavior and invariants.
- Public API and double-entry results do not change.
- The editor owns immutable edits while validation and pack construction use focused collaborators.
- Diagnostics and generated pack remain behaviorally compatible.
- No productive C# file in the refactored editor cluster exceeds 180 lines.
