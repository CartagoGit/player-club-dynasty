---
id: f00010
title: "Implement external content import and pack validation"
kind: feat
status: ready
type: proposal
track: content
date: 2026-08-11
---

# f00010 — Implement external content import and pack validation

## Goal

Implement the versioned import pipeline for local JSON/CSV and SQLite sources, validating and compiling external content into safe immutable packs.

## why

The game must load community or external databases without leaking external schemas into the engine.

## non-goals

- Downloading commercial data, arbitrary pack scripting or an in-game editor.

## Slices

- global_gate: e2e

### S1 — Define normalized import contracts
- **Status**: done
- **Files**: `game/src/PCDynasty.Content.Abstractions/PCDynasty.Content.Abstractions.csproj`, `game/src/PCDynasty.Content.Abstractions/IContentReader.cs`, `game/src/PCDynasty.Content.Abstractions/NormalizedContentModel.cs`, `game/tests/PCDynasty.Content.Abstractions.Tests/PCDynasty.Content.Abstractions.Tests.csproj`, `game/tests/PCDynasty.Content.Abstractions.Tests/NormalizedContentModelTests.cs`
- **Gate**: type
- acceptance:
  - "External providers communicate through versioned DTOs and diagnostics."

### S2 — Implement JSON and CSV readers
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Content.Importers/PCDynasty.Content.Importers.csproj`, `game/src/PCDynasty.Content.Importers/JsonContentReader.cs`, `game/src/PCDynasty.Content.Importers/CsvContentReader.cs`, `game/tests/PCDynasty.Content.Importers.Tests/PCDynasty.Content.Importers.Tests.csproj`, `game/tests/PCDynasty.Content.Importers.Tests/ContentReaderTests.cs`
- **Gate**: type
- acceptance:
  - "Equivalent input produces the same normalized model."

### S3 — Compile and validate packs
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Content/PCDynasty.Content.csproj`, `game/src/PCDynasty.Content/PackCompiler.cs`, `game/src/PCDynasty.Content/PackValidator.cs`, `game/tests/PCDynasty.Content.Tests/PCDynasty.Content.Tests.csproj`, `game/tests/PCDynasty.Content.Tests/PackCompilerTests.cs`, `game/PCDynasty.sln`
- **Gate**: type
- acceptance:
  - "Invalid references or dependency graphs fail with useful diagnostics."

### S4 — Add SQLite content reader
- **Status**: pending
- **DependsOn**: [S1, S3]
- **Files**: `game/src/PCDynasty.Content.Importers/SqliteContentReader.cs`, `game/tests/PCDynasty.Content.Importers.Tests/SqliteContentReaderTests.cs`
- **Gate**: e2e
- acceptance:
  - "A large local database imports through the same external contract."

## acceptance

- External providers communicate through versioned DTOs and diagnostics.
- Equivalent input produces the same normalized model.
- Invalid references or dependency graphs fail with useful diagnostics.
- A large local database imports through the same external contract.
