# PC Dynasty — Audio

Everything you need to design, generate, review and ship audio assets for
PC Dynasty.

## Documents in this folder

| File | Audience | What you'll find |
|---|---|---|
| [manual.md](./manual.md) | **Humans** | How to use the runner from your terminal. Examples for every workflow stage, troubleshooting, recipes for common tasks. |
| [mcp-vertex-usage.md](./mcp-vertex-usage.md) | **AI agents** | How an MCP agent is expected to drive the workflow through the `player-club-dynasty_*` tools. Includes a decision tree and example dialogues. |
| [mmx-reference.md](./mmx-reference.md) | Both | Every `mmx music generate` and `mmx speech synthesize` flag the runner relies on, plus the prompt-engineering rules baked into `audio-spec.json`. |
| [spec-format.md](./spec-format.md) | Both | The exact shape of `game/godot/Audio/audio-spec.json`: groups, slots, the `globalMusicAnchor` and the variant state machine. |

## The pipeline at a glance

```text
                mmx (music-3.0 / speech-2.8-hd)
                            │
                            ▼
                .audio-staging/<group>/<entry>/v_<ts>.wav  ← human listens
                            │
                            ▼
                ffmpeg (libvorbis / libopus / libmp3lame)
                            │
                            ▼
        ┌───────────────────┴───────────────────┐
        │                                       │
   approve                                 deprecate
        │                                       │
        ▼                                       ▼
game/godot/Audio/<Music|SFX>/<file>    .deprecated/assets/sounds/<group>/<entry>/
   (committed, ship-ready)              (gitignored, recoverable)
```

## Where assets end up

| Location | In git? | Purpose |
|---|---|---|
| `game/godot/Audio/Music/` | ✅ yes | Approved music tracks (shipped to Godot). |
| `game/godot/Audio/SFX/` | ✅ yes | Approved sound effects (shipped to Godot). |
| `.audio-staging/` | ❌ no | Raw, uncompressed wav queued for review. |
| `.deprecated/assets/sounds/` | ❌ no | Local archive of superseded variants. |
| `game/content/audio/audio-spec.json` | ✅ yes | The declarative manifest (what we want, what we've shipped). |

## Where the code lives

| Component | File | Role |
|---|---|---|
| **Manifest (data)** | `game/content/audio/audio-spec.json` | The declarative spec — groups, slots, `globalMusicAnchor`. Sibling of `game/content/starter-world/`. |
| Manifest types (TS) | `tools/contracts/interfaces/audio-spec.interface.ts` | The single source of truth — types + zod schemas. |
| Manifest loader | `tools/helpers/audio-spec.helper.ts` | Read/write the spec, resolve slots, find groups. Reusable from any content pipeline. |
| Encoder | `tools/helpers/audio-encoder.helper.ts` | ffmpeg wrapper, picks codec per slot format. |
| Progress indicator | `tools/helpers/progress.helper.ts` | Spinner + elapsed timer for slow CLI calls. |
| Runner | `tools/scripts/content/generate-audio.script.ts` | The `stage / list / listen / approve / deprecate / status / --validate` CLI. Sibling of `generate-world.script.ts`. |
| Plugin | `plugins/mcp-vertex_player-club-dynasty/src/lib/audio-manifest.helper.ts` + `src/index.ts` | MCP tools `audio_stage / audio_list_variants / audio_listen / audio_approve / audio_deprecate`. |
| Runtime location | `game/godot/Audio/{Music,SFX}/` | Where Godot reads approved files (committed). |
| Runtime README | `game/godot/Audio/SPEC.md` | Godot-side rationale (link to `docs/audios/`). |