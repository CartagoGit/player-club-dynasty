---
id: p00001
title: "Adopt a tools/scripts architecture with helpers, contracts and a typed plugin surface"
kind: refactor
status: review
type: proposal
track: infra
date: 2026-08-18
---

# p00001 — Adopt a tools/scripts architecture with helpers, contracts and a typed plugin surface

## Goal

Reorganise every script and helper the workspace owns behind a single
**tools/** layout with a strict extension-based convention, group each role
folder by domain so no folder is a bucket, expose the canonical contracts and
helpers as typed modules, and put the whole tree under the typecheck gate. The
repository is left cleaner than we found it: scripts become thin, helpers
become reusable, contracts become the source of truth.

## why

The workspace alternates between `scripts/*.script.ts`, `game/scripts/*.sh`
and ad-hoc `bun` scripts. The mix has four symptoms:

1. **Hidden coupling.** `scripts/play-game.script.ts`, `run-godot-smoke.script.ts`
   and `game/scripts/run-game.sh` all re-implement the same dotnet-and-godot
   discovery; when one is fixed, the other two drift.
2. **No surface for agents.** There is no place a new agent can ask "what does
   a helper look like here?" — the only knowledge is locked in script bodies.
3. **Bash entrypoints hide behaviour.** `game/scripts/run-game.sh` does
   interesting things (lock detection, .NET 8 verification, .tools symlink
   refresh) but the whole shell layer is opaque to anyone who only reads
   the `.ts` files.
4. **The tree was never type-checked.** `tsconfig.json` included `src/**` (a
   directory that does not exist) and `scripts/**`, so every file under
   `tools/` compiled only under Bun's loader. Thirty type errors accumulated
   there unseen, three of them real runtime defects.

## non-goals

- Re-architecting the game domain (C# under `game/src/`).
- Touching the delendai core or its first-party plugins.
- Rewriting the audio runner to consume the shared contract. It duplicates the
  whole manifest inline and reaches for `node:fs` directly, which the
  convention forbids; that rewrite is `a00041` A6 and belongs to whoever owns
  the audio runner, not to a folder move.
- Adding a brand-new gate category or benchmark framework.

## Slices

- global_gate: type

### S1 — Define the tools/contracts and tools/helpers baseline
- **Status**: done
- **Files**: `tools/contracts/constants/*.constants.ts`, `tools/contracts/interfaces/*.interface.ts`, `tools/helpers/*.helper.ts`
- **Gate**: type
- acceptance:
  - "Every dotnet/godot/env/fs constant lives in `tools/contracts/constants/`."
  - "Every shared type lives in `tools/contracts/interfaces/`."
  - "Every shared utility lives in `tools/helpers/*.helper.ts` and is consumed by at least two scripts."
  - "`bun run typecheck` exits 0 for the new files."

### S2 — Move the Godot + .NET entrypoints to tools/scripts/game/
- **Status**: done
- **Files**: `tools/scripts/game/play.script.ts`, `tools/scripts/game/doctor.script.ts`, `tools/scripts/game/run-godot-smoke.script.ts`
- **Gate**: type
- acceptance:
  - "Each script is a thin orchestrator: imports helpers, never `node:fs` / `node:child_process` directly."
  - "`bun run game:doctor` reports the same diagnostics as before the migration."
  - "`bun run game:play`, `bun run game:editor`, `bun run game:import` and `bun run game:smoke` invoke the new paths."

### S3 — Port the four game/scripts/*.sh gates to tools/scripts/gate/
- **Status**: done
- **Files**: `tools/scripts/gate/{tests,architecture,hygiene,benchmarks}.script.ts`, `game/scripts/*.sh` deleted
- **Gate**: type
- acceptance:
  - "`game/scripts/test-all.sh`, `check-architecture.sh`, `check-repository-hygiene.sh`, `run-benchmarks.sh` are gone."
  - "The four `.script.ts` replacements carry the same behaviour and respect the env vars the bash versions respected."
  - "`bun run game:tests`, `bun run game:architecture`, `bun run game:hygiene`, `bun run game:benchmarks` work and exit 0 / 1 with the same contract."

### S4 — Migrate scripts/ legacy entrypoints to tools/scripts/<area>/
- **Status**: done
- **Files**: `tools/scripts/workspace/{root,run,dev,link,sync-bootstrap,lint-proposals}.script.ts`, `tools/scripts/builder/{plugin,all}.script.ts`, `tools/scripts/replays/{visible-routes,benchmark-budgets}.script.ts`, `tools/scripts/content/world/generate.script.ts`, `scripts/` deleted
- **Gate**: type
- acceptance:
  - "Every `.script.ts` that previously lived in `scripts/` now lives under `tools/scripts/<area>/`."
  - "The three `tools/` files that were left as stubs pointing back at the legacy folder (`visible-routes`, `benchmark-budgets`, `content/world/generate`) carry the real implementation."
  - "The three specs that only existed under `scripts/` move with their subject."
  - "`.mcp.json`, `.github/workflows/game-quality.yml`, `package.json`, `vitest.config.ts` and the READMEs name the new paths."
  - "`bun run check:visible-routes` runs the real gate and reports its findings instead of a stub message."

### S5 — Group every role folder by domain
- **Status**: done
- **Files**: `tools/contracts/{constants,interfaces}/{content,engine,system}/**`, `tools/helpers/{content,engine,system}/**`, `tools/scripts/content/{audio,video,world}/**`
- **Gate**: type
- acceptance:
  - "`tools/helpers/` has no loose files: every helper sits under `content/`, `engine/` or `system/`, and the audio and video pipelines have a folder each."
  - "`tools/scripts/content/` splits into `audio/`, `video/` and `world/`; the domain lives in the folder so filenames drop their prefix (`generate.script.ts`, not `generate-audio.script.ts`)."
  - "Contracts follow the same three-way split, with the audio and video manifests under `interfaces/content/`."
  - "Every moved file names its own path in its docblock."
  - "`package.json`, the docs under `docs/audios/` and `docs/videos/`, and the plugin README point at the new paths."

### S6 — Put tools/ under the typecheck gate and fix what it finds
- **Status**: done
- **Files**: `tsconfig.json`, `tools/helpers/content/{audio,video}/encoder.helper.ts`, `tools/helpers/content/{audio,video}/spec.helper.ts`, `tools/helpers/system/path.helper.ts`, `tools/contracts/interfaces/content/*.interface.ts`, `tools/scripts/content/audio/{checklist,review}.script.ts`, `tools/scripts/content/video/generate.script.ts`
- **Gate**: type
- acceptance:
  - "`tsconfig.json` includes `tools/**` and no longer includes the deleted `scripts/**` or the non-existent `src/**`."
  - "`bun run typecheck` exits 0 with the whole tools tree in scope."
  - "The three runtime defects the gate exposed are fixed, not silenced: both encoders awaited nothing and always reported failure; `review.script.ts` called `once` on a `BunFile`; its 'why is it bad' prompt compared against a value the switch never produced."
  - "The manifests keep their readonly read model and gain one declared mutable draft type each, so the runner stops casting at every call site."
  - "`workspaceRoot()` discovers the root by walking up to `package.json` instead of counting `..` segments, and a test pins that two different depths resolve to the same root."

### S7 — Teach the project MCP plugin about the new convention
- **Status**: ready
- **Files**: `plugins/delendai_player-club-dynasty/src/lib/conventions/file-conventions.constants.ts`, `plugins/delendai_player-club-dynasty/src/lib/conventions/file-assistant.helper.ts`, `plugins/delendai_player-club-dynasty/src/index.ts`, `plugins/delendai_player-club-dynasty/README.md`, `plugins/delendai_player-club-dynasty/src/lib/conventions/file-conventions.spec.ts`
- **Gate**: type
- acceptance:
  - "The convention constants describe the domain grouping, not just the role folders."
  - "The plugin exposes a `player-club-dynasty_create_file` tool that takes `{ kind, path, content }` and validates the path against the convention, domain folder included."
  - "The plugin exposes a `player-club-dynasty_new_file_template` prompt that returns a ready-made header + imports for the requested `kind`."
  - "`bun --cwd plugins/delendai_player-club-dynasty test` is green."

## acceptance

- `tools/` is the only place scripts and helpers live, and every role folder is
  grouped by domain.
- Every script is a thin orchestrator; helpers own the filesystem and process work.
- `tools/**` is inside the typecheck gate and the gate is green.
- The legacy `scripts/` and `game/scripts/*.sh` directories are gone.
