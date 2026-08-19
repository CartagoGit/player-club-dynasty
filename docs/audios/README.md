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
| [REVIEW-CHECKLIST.md](./REVIEW-CHECKLIST.md) | **Humans (the only file you need)** | Live review: each row has `<audio controls src=".generate/audios/staging/.../track.ogg">` for inline play, `<a download>` for grabbing the .ogg, and HTML checkboxes for OK / Bad / Re-stage. Open in VS Code preview (`Ctrl+Shift+V`) or in a browser. |

## The pipeline at a glance

```text
                mmx (music-3.0 / speech-2.8-hd)
                            │
                            ▼
                .generate/audios/staging/<group>/<entry>/v_<ts>.wav  ← human listens
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
| `.generate/audios/staging/` | ❌ no | Raw, uncompressed wav queued for review. |
| `.deprecated/assets/sounds/` | ❌ no | Local archive of superseded variants. |
| `game/content/audio/audio-spec.json` | ✅ yes | The declarative manifest (what we want, what we've shipped). |

## Where the code lives

| Component | File | Role |
|---|---|---|
| **Manifest (data)** | `game/content/audio/audio-spec.json` | The declarative spec — groups, slots, `globalMusicAnchor`. Sibling of `game/content/starter-world/`. |
| Manifest types (TS) | `tools/contracts/interfaces/content/audio-spec.interface.ts` | The single source of truth — types + zod schemas. |
| Manifest loader | `tools/helpers/content/audio/spec.helper.ts` | Read/write the spec, resolve slots, find groups. Reusable from any content pipeline. |
| Encoder | `tools/helpers/content/audio/encoder.helper.ts` | ffmpeg wrapper, picks codec per slot format. |
| Progress indicator | `tools/helpers/system/progress.helper.ts` | Spinner + elapsed timer for slow CLI calls. |
| Runner | `tools/scripts/content/audio/generate.script.ts` | The `stage / list / listen / approve / deprecate / status / --validate` CLI. Sibling of `generate-world.script.ts`. |
| Checklist builder | `tools/scripts/content/audio/checklist.script.ts` | Regenerates `docs/audios/REVIEW-CHECKLIST.md` from the spec + on-disk staging state. |
| Interactive review | `tools/scripts/content/audio/review.script.ts` | Walks every staged ogg, plays it, prompts `o/b/r/s`, optionally commits decisions. |
| Open in OS player | `tools/scripts/content/audio/open.script.ts` | Cross-platform open: on WSL → cmd.exe + Windows file association; on Linux → xdg-open. |
| Pre-compress wav→ogg | `tools/scripts/content/audio/precompress.script.ts` | Encodes staged wavs to libvorbis q=5 oggs in place. Use `--delete-wavs` after to clean up. |
| Plugin | `plugins/mcp-vertex_player-club-dynasty/src/lib/audio-manifest.helper.ts` + `src/index.ts` | MCP tools `audio_stage / audio_list_variants / audio_listen / audio_approve / audio_deprecate`. |
| Runtime location | `game/godot/Audio/{Music,SFX}/` | Where Godot reads approved files (committed). |
| Runtime README | `game/godot/Audio/SPEC.md` | Godot-side rationale (link to `docs/audios/`). |

## Review workflow (md or terminal)

There are two ways to review. Pick whichever fits your setup.

### Method A — Visual md (REVIEW-CHECKLIST.md)

The single review file. After staging new variants, regenerate it:

```bash
bun run game:audio:checklist
```

Open `docs/audios/REVIEW-CHECKLIST.md` in VS Code preview (`Ctrl+Shift+V`) or in a browser (`xdg-open docs/audios/REVIEW-CHECKLIST.md`).

Each variant row has:
- `<audio controls>` for inline play (best in browser)
- `<a href download>` to grab the .ogg to your Downloads folder
- HTML checkboxes for OK / Bad / Re-stage
- Copy-paste commands for approve / deprecate / re-stage

If the `<audio>` doesn't play in your preview (e.g. WSL with no pulseaudio), the `listen:` command in each row opens the file in the OS-native player:

```bash
bun run game:audio:open .generate/audios/staging/sfx.ui/ui_click/v_xxx.ogg
```

### Method B — Terminal interactive review

```bash
bun run game:audio:review --group=sfx.ui      # one group
bun run game:audio:review --group=sfx.ui --commit   # execute the decisions at the end
```

The script plays each staged ogg in the OS-native player (VLC on Windows, ffplay on Linux), prompts `o/b/r/s`, and at the end prints the decisions and (with `--commit`) runs `audio_approve` / `audio_deprecate` / `audio_stage` for you.

If you just want a single track in your player:

```bash
bun run game:audio:open .generate/audios/staging/sfx.ui/ui_click/v_xxx.ogg
```

## WSL audio note

WSLg (Windows 11 + WSL 2 GUI) provides a PulseAudio socket at `/mnt/wslg/PulseServer` that bridges Linux audio to the Windows speakers. After installing `pulseaudio` in WSL (`sudo apt install pulseaudio pulseaudio-utils`), set the env var and any audio player will work:

```bash
export PULSE_SERVER=unix:/mnt/wslg/PulseServer
ffplay -nodisp -autoexit .generate/audios/staging/sfx.ui/ui_click/v_xxx.ogg
```

The `~/.bashrc` already exports `PULSE_SERVER` automatically when the socket exists, so this just works in every new terminal. The `game:audio:review` script auto-detects the bridge and uses `ffplay` instead of the external-player fallback.

### WSL preview: why `<audio>` still doesn't sound in VS Code

The inline `<audio controls>` in the VS Code markdown preview is **always silent on WSL**, even with pulseaudio installed — the webview Chromium sandbox doesn't have access to the audio device. This is a VS Code / WSL limitation, not a config issue. For sound during review, use `game:audio:review` (terminal) or open the md in a real browser (`game:audio:open docs/audios/REVIEW-CHECKLIST.md`).