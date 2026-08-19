---
id: p00001
title: "Adopt a tools/scripts architecture with helpers, contracts and a typed plugin surface"
kind: refactor
status: ready
type: proposal
track: infra
date: 2026-08-18
---

# p00001 — Adopt a tools/scripts architecture with helpers, contracts and a typed plugin surface

## Goal

Reorganise every script and helper that the workspace owns behind a single
**tools/** layout with a strict extension-based convention, expose the
canonical contracts and helpers as typed modules, and teach the
workspace's MCP plugin how to scaffold a new file under the same
rules. The repository is left cleaner than we found it: scripts
become thin, helpers become reusable, contracts become the source of
truth, and any agent (human or IA) can ask the plugin for a template
that follows the convention.

## why

The workspace alternates between `scripts/*.script.ts`, `game/scripts/*.sh`
and ad-hoc `bun` scripts. The mix has three symptoms:

1. **Hidden coupling.** `scripts/play-game.script.ts`, `run-godot-smoke.script.ts`
   and `game/scripts/run-game.sh` all re-implement the same dotnet-and-godot
   discovery; when one is fixed, the other two drift.
2. **No surface for agents.** There is no place a new agent can ask "what does
   a helper look like here?" — the only knowledge is locked in script bodies.
3. **Bash entrypoints hide behaviour.** `game/scripts/run-game.sh` does
   interesting things (lock detection, .NET 8 verification, .tools symlink
   refresh) but the whole shell layer is opaque to anyone who only reads
   the `.ts` files.

## non-goals

- Re-architecting the game domain (C# under `game/src/`).
- Touching the mcp-vertex core or its first-party plugins.
- Renaming paths that already follow the convention (e.g. `dev.script.ts`,
  `mcp-vertex-root.script.ts`).
- Adding a brand-new gate category or benchmark framework.

## Slices

- global_gate: type

### S1 — Define the tools/contracts and tools/helpers baseline
- **Status**: ready
- **Files**: `tools/contracts/constants/*.constants.ts`, `tools/contracts/interfaces/*.interface.ts`, `tools/helpers/*.helper.ts`
- **Gate**: type
- acceptance:
  - "Every dotnet/godot/env/fs constant lives in `tools/contracts/constants/`."
  - "Every shared type lives in `tools/contracts/interfaces/`."
  - "Every shared utility lives in `tools/helpers/*.helper.ts` and is consumed by at least two scripts."
  - "`bun run typecheck` exits 0 for the new files."

### S2 — Move the Godot + .NET entrypoints to tools/scripts/game/
- **Status**: ready
- **Files**: `tools/scripts/game/play.script.ts`, `tools/scripts/game/doctor.script.ts`, `tools/scripts/game/run-godot-smoke.script.ts`
- **Gate**: type
- acceptance:
  - "Each script is a thin orchestrator: imports helpers, never `node:fs` / `node:child_process` directly."
  - "`bun run game:doctor` reports the same diagnostics as before the migration."
  - "`bun run game:play`, `bun run game:editor`, `bun run game:import` and `bun run game:smoke` invoke the new paths."

### S3 — Port the four game/scripts/*.sh gates to tools/scripts/gate/
- **Status**: ready
- **Files**: `tools/scripts/gate/{tests,architecture,hygiene,benchmarks}.script.ts`, `game/scripts/*.sh` deleted
- **Gate**: type
- acceptance:
  - "`game/scripts/test-all.sh`, `check-architecture.sh`, `check-repository-hygiene.sh`, `run-benchmarks.sh` are gone."
  - "The four `.script.ts` replacements carry the same behaviour and respect the env vars the bash versions respected."
  - "`bun run game:tests`, `bun run game:architecture`, `bun run game:hygiene`, `bun run game:benchmarks` work and exit 0 / 1 with the same contract."

### S4 — Migrate scripts/ legacy entrypoints to tools/scripts/<area>/
- **Status**: ready
- **Files**: `tools/scripts/mcp-vertex/{root,run,dev,link,sync-bootstrap,lint-proposals}.script.ts`, `tools/scripts/build/{plugin,all}.script.ts`, `tools/scripts/replays/{visible-routes,benchmark-budgets}.script.ts`, `tools/scripts/content/generate-world.script.ts`, `scripts/` deleted
- **Gate**: type
- acceptance:
  - "Every `.script.ts` that previously lived in `scripts/` now lives under `tools/scripts/<area>/`."
  - "Cross-script imports use the new relative paths."
  - "`package.json` scripts point at the new paths and `bun run typecheck` is green."

### S5 — Teach the project MCP plugin about the new convention
- **Status**: ready
- **Files**: `plugins/mcp-vertex_player-club-dynasty/src/lib/architecture-conventions.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/file-assistant.ts`, `plugins/mcp-vertex_player-club-dynasty/src/index.ts`, `plugins/mcp-vertex_player-club-dynasty/src/public/index.ts`, `plugins/mcp-vertex_player-club-dynasty/README.md`, `plugins/mcp-vertex_player-club-dynasty/src/lib/architecture-conventions.spec.ts`
- **Gate**: type
- acceptance:
  - "The plugin exposes a `player-club-dynasty_create_file` tool that takes `{ kind, path, content }` and validates the extension against the convention."
  - "The plugin exposes a `player-club-dynasty_new_file_template` prompt that returns a ready-made header + imports for the requested `kind`."
  - "The plugin registers a `player-club-dynasty_architecture` skill summarising the SOLID rules and the conventions."
  - "`bun --cwd plugins/mcp-vertex_player-club-dynasty test` is green."

### S6 — Wire package.json and validate end-to-end
- **Status**: ready
- **Files**: `package.json`, `tools/scripts/**/*`, `plugins/mcp-vertex_player-club-dynasty/src/**`
- **Gate**: type
- acceptance:
  - "`package.json` only references paths under `tools/` and the plugin."
  - "`bun run typecheck`, `bun run test:scripts`, `bun run game:doctor`, `bun --cwd plugins/mcp-vertex_player-club-dynasty test` and `bun run build:plugin` are all green."
  - "No file lives under `scripts/` (except a `.gitkeep` if any test reference demands it)."

## acceptance

- `tools/` is the only place scripts and helpers live.
- Every script is a thin orchestrator; helpers own the filesystem and process work.
- The project MCP plugin can scaffold a new file under the convention.
- The legacy `scripts/` and `game/scripts/*.sh` directories are gone.
