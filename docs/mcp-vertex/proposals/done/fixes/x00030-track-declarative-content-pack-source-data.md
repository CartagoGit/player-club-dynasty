---
id: x00030
title: "Track declarative content pack source data"
kind: fix
status: done
type: proposal
track: content
date: 2026-08-11
---

# x00030 — Track declarative content pack source data

## Goal

Narrow the local generated-data ignore rule so authored declarative pack sources under game/content remain versionable.

## why

The generic data directory rule unintentionally ignores source-controlled starter pack documents.

## non-goals

- Track local generated plugin data or saves.

## Slices

- global_gate: none

### S1 — Unignore authored game content
- **Status**: done
- **Files**: `.gitignore`
- **Gate**: none
- acceptance:
  - "Authored files under game/content are trackable while root local data and saves remain ignored."

## acceptance

- Authored files under game/content are trackable while root local data and saves remain ignored.
