---
id: x00013
title: "Own the SQLite importer project dependency"
kind: fix
status: done
type: proposal
track: content
date: 2026-08-11
---

# x00013 — Own the SQLite importer project dependency

## Goal

Allow the SQLite content slice to declare its provider dependency without editing outside its owned files.

## why

The SQLite reader cannot compile unless its existing importer project file is owned by the slice.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct SQLite slice ownership
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/f00010-implement-external-content-import-and-pack-validation.md`
- **Gate**: none
- acceptance:
  - "The SQLite reader slice owns PCDynasty.Content.Importers.csproj."

## acceptance

- The SQLite reader slice owns PCDynasty.Content.Importers.csproj.
