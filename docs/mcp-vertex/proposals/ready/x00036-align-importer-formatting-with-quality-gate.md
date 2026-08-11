---
id: x00036
title: "Align importer formatting with quality gate"
kind: fix
status: ready
type: proposal
track: quality
date: 2026-08-11
---

# x00036 — Align importer formatting with quality gate

## Goal

Normalize the existing SQLite importer indentation so the solution-wide formatting gate starts green.

## why

The new verified formatting gate identifies one pre-existing whitespace drift in SqliteContentReader.

## non-goals

- Change importer behavior.

## Slices

- global_gate: type

### S1 — Normalize SQLite importer whitespace
- **Status**: done
- **Files**: `game/src/PCDynasty.Content.Importers/SqliteContentReader.cs`
- **Gate**: type
- acceptance:
  - "dotnet format reports no solution formatting drift."

## acceptance

- dotnet format reports no solution formatting drift.
