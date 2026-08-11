---
id: i00001
title: "Establish .NET build quality and delivery pipeline"
kind: infra
status: ready
type: proposal
track: quality
date: 2026-08-11
---

# i00001 — Establish .NET build quality and delivery pipeline

## Goal

Provision the .NET/Godot development prerequisites and establish formatting, analyzers, architecture tests, test execution and benchmark delivery gates.

## why

Architecture standards only become permanent when the build prevents regressions.

## non-goals

- Publishing a commercial release or replacing the existing mcp-vertex tooling.

## Slices

- global_gate: e2e

### S1 — Pin SDK and solution-wide compiler standards
- **Status**: done
- **Files**: `game/global.json`, `game/Directory.Build.props`, `game/.editorconfig`
- **Gate**: type
- acceptance:
  - "SDK, nullable, analyzers and warnings-as-errors are reproducibly configured."

### S2 — Add test and architecture check entrypoints
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/scripts/test-all.sh`, `game/scripts/check-architecture.sh`, `game/README.md`
- **Gate**: type
- acceptance:
  - "Core tests and forbidden-dependency checks run without Godot."

### S3 — Add CI quality workflow
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `.github/workflows/game-quality.yml`, `game/scripts/run-benchmarks.sh`
- **Gate**: e2e
- acceptance:
  - "Pull requests run relevant tests and publish benchmark comparison artifacts."

## acceptance

- SDK, nullable, analyzers and warnings-as-errors are reproducibly configured.
- Core tests and forbidden-dependency checks run without Godot.
- Pull requests run relevant tests and publish benchmark comparison artifacts.
