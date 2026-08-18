---
id: i00002
title: "Repair workspace orchestration and enforce repository hygiene"
kind: infra
status: ready
type: proposal
track: quality
date: 2026-08-18
---

# i00002 — Repair workspace orchestration and enforce repository hygiene

## Goal

Make the mcp-vertex host configuration internally consistent and prevent generated game artifacts from ever being committed.

## why

The live host requests a deprecated duplicate plugin id, the documented root validation command does not exist, and artifact hygiene is only an implicit .gitignore convention.

## non-goals

- Delete authored Godot UID metadata or another agent's source files.
- Change game behavior.

## Slices

- global_gate: e2e

### S1 — Align the local plugin bootstrap and validation entrypoint
- **Status**: done
- **Files**: `.codex/config.toml`, `scripts/dev.script.ts`, `package.json`, `plugins/mcp-vertex_player-club-dynasty/src/lib/types.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/game-data.types.ts`, `plugins/mcp-vertex_player-club-dynasty/src/index.ts`, `plugins/mcp-vertex_player-club-dynasty/src/public/index.ts`
- **Gate**: type
- acceptance:
  - "The local plugin loads once through the configured path without a deprecated duplicate request."
  - "bun run validate exists and passes the workspace TypeScript/plugin gates."
  - "Plugin contracts follow the repository's explicit type-file naming convention."

### S2 — Turn generated-artifact hygiene into a CI gate
- **Status**: pending
- **Files**: `.gitignore`, `game/scripts/check-repository-hygiene.sh`, `.github/workflows/game-quality.yml`, `game/README.md`
- **Gate**: e2e
- acceptance:
  - "No tracked path matches .NET, Godot, test, benchmark, Node or MCP cache outputs."
  - "The CI job fails if a generated artifact becomes tracked."
  - "Authored Godot UID files remain versioned."

## acceptance

- The local plugin loads once through the configured path without a deprecated duplicate request.
- bun run validate exists and passes the workspace TypeScript/plugin gates.
- No tracked path matches .NET, Godot, test, benchmark, Node or MCP cache outputs.
- The CI job fails if a generated artifact becomes tracked.
- Authored Godot UID files remain versioned.
