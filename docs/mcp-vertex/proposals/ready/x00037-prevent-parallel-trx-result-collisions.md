---
id: x00037
title: "Prevent parallel TRX result collisions"
kind: fix
status: ready
type: proposal
track: quality
date: 2026-08-11
---

# x00037 — Prevent parallel TRX result collisions

## Goal

Give each solution test assembly a distinct TRX output so successful parallel test runs return success in CI.

## why

The first GitHub Actions run passed every test but reused one LogFileName across parallel assemblies, causing overwrite races and a non-zero test command exit.

## non-goals

- Change test behavior or disable parallel execution.

## Slices

- global_gate: e2e

### S1 — Use per-assembly test result prefixes
- **Status**: done
- **Files**: `.github/workflows/game-quality.yml`
- **Gate**: e2e
- acceptance:
  - "The full CI quality job completes successfully and publishes distinct test diagnostics."

## acceptance

- The full CI quality job completes successfully and publishes distinct test diagnostics.
