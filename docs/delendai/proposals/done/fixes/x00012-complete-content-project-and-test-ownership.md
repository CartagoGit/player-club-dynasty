---
id: x00012
title: "Complete Content project and test ownership"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00012 — Complete Content project and test ownership

## Goal

Make normalized contracts, importers, compiler and SQLite reader independently testable and solution-wide discoverable.

## why

The content proposal omits abstraction tests, production project files for importers/compiler, their test projects, and solution registration.

## non-goals

- Implement content behavior.

## Slices

- global_gate: none

### S1 — Complete all Content build boundaries
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/f00010-implement-external-content-import-and-pack-validation.md`
- **Gate**: none
- acceptance:
  - "Every Content slice owns required production/test projects and S3 owns solution registration."

## acceptance

- Every Content slice owns required production/test projects and S3 owns solution registration.
