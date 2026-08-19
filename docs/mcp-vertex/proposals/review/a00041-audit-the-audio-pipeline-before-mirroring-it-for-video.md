---
id: a00041
title: "Audit the audio pipeline before mirroring it for video"
kind: audit
status: review
type: proposal
track: quality
date: 2026-08-19
plan: q00001
---

# a00041 — Audit the audio pipeline before mirroring it for video

## Goal

Establish what the audio asset pipeline actually does, what it does well enough to
copy, and what is broken — before a video pipeline is built in its image and
inherits the same defects.

## why

A second pipeline modelled on the first multiplies whatever the first got wrong.
The audio work is substantial and largely well made, so the parts worth copying
need naming as precisely as the parts that need fixing.

## non-goals

- Fixing the audio pipeline's own defects. Most of them sit in files another agent
  is editing right now; this document records them so their owner can close them.
- Judging the sonic content. This is an audit of the machinery.

## Slices

- global_gate: unit

### S1 — Record what the pipeline is and what holds up
- **Status**: done
- **Files**: `docs/audios/README.md`, `tools/contracts/interfaces/audio-spec.interface.ts`, `tools/helpers/audio-spec.helper.ts`, `tools/scripts/content/generate-audio.script.ts`
- **Gate**: unit
- acceptance:
  - "The audit names the shape: a declarative manifest of 12 groups and 65 slots,
    a variant lifecycle of staged → approved → deprecated, generation into a
    gitignored staging tree, transcode on approve, and archival of the superseded
    variant into a recoverable local directory."
  - "It names what is worth copying: types and zod validators in one contract
    file, a helper shared by runner and plugin, staging outside git so the repo
    only ever carries approved assets, deprecation that is recoverable rather
    than destructive, MCP tools alongside the CLI, docs split by audience, and no
    machine-specific path anywhere in the committed code."

### S2 — Record the defects, with their blast radius
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/review/a00041-audit-the-audio-pipeline-before-mirroring-it-for-video.md`
- **Gate**: unit
- acceptance:
  - "Each finding states what is wrong, how it was observed, and who owns it."
  - "Findings that belong to work in flight are marked as such rather than fixed
    under this audit."

### S3 — Fix what is committed, broken and unowned
- **Status**: done
- **Files**: `plugins/mcp-vertex_player-club-dynasty/src/lib/conventions/file-conventions.helper.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/conventions/file-conventions.spec.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/conventions/file-assistant.helper.ts`, `plugins/mcp-vertex_player-club-dynasty/src/lib/index.ts`
- **Gate**: unit
- acceptance:
  - "`bun run typecheck` no longer fails on the plugin's conventions module."

## acceptance

- Every finding is reproducible from the commands quoted in it.
- The video pipeline built next copies the strengths and none of the defects.

## verified state

### What the pipeline is

A declarative manifest (`game/content/audio/audio-spec.json`, version 3) of **12
groups and 65 slots**, each slot a named sound with a prompt. Generation goes to
`.generate/audios/staging/` (gitignored), review is either an inline-audio
markdown checklist or a terminal walker, approval transcodes with ffmpeg into
`game/godot/Audio/{Music,SFX}/` and archives the previous variant under
`.deprecated/`. The same manifest is exposed to agents through five MCP tools.

### What is worth copying

1. **One contract, two consumers.** Types and zod validators live in
   `tools/contracts/interfaces/audio-spec.interface.ts`, so the runner and the
   plugin cannot disagree about what a valid spec is.
2. **A shared helper** (`tools/helpers/audio-spec.helper.ts`) for read, write,
   resolve and slot lookup.
3. **Staging outside git.** The repository only ever carries approved, compressed
   assets; raw generator output never lands in history.
4. **Deprecation is recoverable**, not deletion.
5. **A global anchor** (`globalMusicAnchor`: bpm, key, instruments, loop length)
   so tracks generated months apart still belong to the same game.
6. **Docs split by audience** — one for humans, one for agents, one reference.
7. **No machine-specific path** in any committed file; the two that mention one
   are comments explaining a WSL path translation.

## findings

| # | Finding | Evidence | Owner |
|---|---|---|---|
| **A1** | **`bun run typecheck` was red, and had been since the plugin's domain-grouping refactor.** Four files imported `./file-conventions.constant`; the file is `file-conventions.constants.ts`. Bun's bundler resolves it so the plugin kept building; `tsc` does not, so the whole `bun run validate` chain could not pass. | `bun run typecheck` | **Fixed here** (S3) |
| **A2** | **Five of the runner's own tests fail.** The anchor bpm-drift guard no longer rejects (`expected 1, received 0` on exit code), `deprecate` on a slot with no status throws a stack trace instead of printing a clean error, and three output assertions drifted (`wav` column renamed to `staged`, empty-state hints changed). | `bun run test:scripts` | Audio agent, in flight |
| **A3** | **Two type errors in the plugin's audio view.** `audioPath`/`wavPath`/`audioSizeBytes`/`wavSizeBytes` became optional in `audio-spec.types.ts` but the view type still requires them. | `bun run typecheck` | Audio agent, in flight |
| **A4** | **The manifest advertises a schema that does not exist.** `"$schema": "./audio-spec.schema.json"`; `game/content/audio/` contains only the manifest, so no editor can resolve it. | `ls game/content/audio/` | Audio agent |
| **A5** | **Nothing plays.** 65 slots are declared; `game/godot/Audio/` contains one file, `SPEC.md`; and there is no `AudioStreamPlayer` or `AudioServer` call anywhere in the Godot project. The pipeline produces assets that no code consumes. | `find game/godot/Audio -type f`, `grep -r AudioStreamPlayer game/godot` | Open — needs its own proposal |
| **A6** | **No settings surface for audio.** Until `f00049` there was no volume, mute or enable preference of any kind. | — | **Closed by f00049** |
| **A7** | **The pipeline is not gated in CI.** `game:audio:validate` exists; no workflow runs it, so the spec can drift with nothing noticing. | `grep -n audio .github/workflows/*.yml` | Open |
| **A8** | **A generated artefact is committed into `docs/`.** `docs/audios/REVIEW-CHECKLIST.md` is 142 KB of generated markdown, regenerated by `game:audio:checklist`, with an untracked `.csv` beside it. It will conflict on every regeneration. | `git ls-files docs/audios/` | Audio agent |
| **A9** | **The runner re-declares the manifest types locally** instead of importing the contract that names it as a consumer, which is the drift the contract exists to prevent. | `tools/scripts/content/generate-audio.script.ts` lines 50–70 | Audio agent |
| **A10** | **A stray `test-audio.html` sits at the repository root.** | `ls test-audio.html` | Audio agent |
| **A11** | **`a00040` declares `kind: audio`, which is not one of the fourteen known kinds**, so `bun run lint:proposals` — and therefore `bun run validate` — fails on it. The filename prefix says `audit`. | `bun run lint:proposals` | Audio agent, in flight |

## scoreboard

| Area | Verdict |
|---|---|
| Contract and helper design | **Copy it** — one contract, two consumers, zod at both ends |
| Asset lifecycle (stage → approve → deprecate) | **Copy it** — recoverable, gitignored staging, approved-only in git |
| Docs | **Copy it** — split by audience |
| Gates | **Do not copy** — the pipeline validates nothing in CI and its own tests are failing |
| Runtime consumption | **Do not copy** — 65 slots declared, nothing plays them |
| Generated output in `docs/` | **Do not copy** — regenerating conflicts every time |

### The lesson the video pipeline took from this

A5 is the one that matters. A pipeline that produces assets nothing consumes looks
finished and is not. The video pipeline therefore shipped its **consumer first**:
`VideoStage` plays a clip, treats a missing file as normal, and is wired to the
settings switch — before a single clip exists.
