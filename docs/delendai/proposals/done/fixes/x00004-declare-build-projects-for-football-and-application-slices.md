---
id: x00004
title: "Declare build projects for Football and Application slices"
kind: fix
status: done
type: proposal
track: orchestration
date: 2026-08-11
---

# x00004 — Declare build projects for Football and Application slices

## Goal

Make every FootballPack vertical slice independently compilable and testable.

## why

The planned source and test files omit required project files from their ownership boundaries.

## non-goals

- Implement Football or Application behavior.

## Slices

- global_gate: none

### S1 — Complete project-file ownership in the Football proposal
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/f00004-create-the-minimal-footballpack-and-player-read-models.md`
- **Gate**: none
- acceptance:
  - "The Football tests project belongs to S1."
  - "The Application source and tests projects belong to S3."

## acceptance

- The Football tests project belongs to S1.
- The Application source and tests projects belong to S3.
