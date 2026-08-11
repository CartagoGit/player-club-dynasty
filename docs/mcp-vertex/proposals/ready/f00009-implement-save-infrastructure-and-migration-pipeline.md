---
id: f00009
title: "Implement save infrastructure and migration pipeline"
kind: feat
status: ready
type: proposal
track: persistence
date: 2026-08-11
---

# f00009 — Implement save infrastructure and migration pipeline

## Goal

Implement atomic local saves, snapshots, facts, version manifests and tested migration/recovery behind the persistence ports.

## why

Long-lived careers require reproducibility and safe evolution independently of the chosen database engine.

## non-goals

- Cloud synchronization, multiplayer or an unrestricted external database server.

## Slices

- global_gate: e2e

### S1 — Define persistence contracts and manifest
- **Status**: pending
- **Files**: `game/src/PCDynasty.Persistence.Abstractions/PCDynasty.Persistence.Abstractions.csproj`, `game/src/PCDynasty.Persistence.Abstractions/IWorldStore.cs`, `game/src/PCDynasty.Persistence.Abstractions/SaveManifest.cs`
- **Gate**: type
- acceptance:
  - "Contracts separate snapshots, facts, versions and pack hashes."

### S2 — Implement local SQLite save adapter
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Persistence.Sqlite/PCDynasty.Persistence.Sqlite.csproj`, `game/src/PCDynasty.Persistence.Sqlite/SqliteWorldStore.cs`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/SqliteWorldStoreTests.cs`
- **Gate**: e2e
- acceptance:
  - "Append and checkpoint are atomic and reload a consistent world."

### S3 — Implement migrations and recovery
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Persistence/PCDynasty.Persistence.csproj`, `game/src/PCDynasty.Persistence/Migrations/SaveMigrator.cs`, `game/tests/PCDynasty.Persistence.Tests/SaveMigratorTests.cs`
- **Gate**: type
- acceptance:
  - "Version migration is explicit, tested and preserves recoverable input."

## acceptance

- Contracts separate snapshots, facts, versions and pack hashes.
- Append and checkpoint are atomic and reload a consistent world.
- Version migration is explicit, tested and preserves recoverable input.
