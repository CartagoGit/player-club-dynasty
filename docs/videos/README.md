# PC Dynasty — Video

Everything needed to design, generate, review and ship the game's video clips.

Built in the image of [`docs/audios/`](../audios/README.md), on purpose: same
verbs, same lifecycle, same place for everything. If you have staged a sound you
already know how to stage a clip.

## Read this first

**Godot plays Ogg Theora and nothing else** without a third-party addon. The
generator returns mp4; `approve` transcodes to `.ogv`. If you drop an mp4 into
`game/godot/Video/` by hand, the engine will refuse it at runtime and the clip
will silently not play. The pipeline exists partly so nobody has to remember
that.

## Documents in this folder

| File | Audience | What you'll find |
|---|---|---|
| [spec-format.md](./spec-format.md) | Both | The exact shape of `game/content/video/video-spec.json`: groups, entries, the look anchor and the variant lifecycle. |
| this README | Both | The pipeline, the commands, and where everything lives. |

## The pipeline at a glance

```text
                    mmx video generate (MiniMax-H3)
                                │
                                ▼
       .generate/videos/staging/<group>/<entry>/v_<ts>.mp4   ← a human watches
                                │
                                ▼
                 ffmpeg (libtheora + libvorbis)
                                │
            ┌───────────────────┴───────────────────┐
            │                                       │
       approve                                 deprecate
            │                                       │
            ▼                                       ▼
 game/godot/Video/<Group>/<file>.ogv      .deprecated/assets/videos/...
      (committed, ship-ready)              (gitignored, recoverable)
```

## Commands

| Task | Command |
|---|---|
| Check the manifest without touching anything | `bun run game:video:validate` |
| Everything at a glance | `bun run game:video:status` |
| Generate a variant (waits for it) | `bun run game:video:stage video.brand.logo_sting` |
| Generate without waiting | `bun run game:video:stage video.brand.logo_sting --async` |
| Download an async variant | `bun run game:video:fetch video.brand.logo_sting --variant v_1692345678` |
| Every variant of every slot | `bun run game:video:list` |
| Ship one | `bun run game:video:approve video.brand.logo_sting --variant v_1692345678` |
| Archive one | `bun run game:video:deprecate video.brand.logo_sting --variant v_1692345678` |

Review is watching the staged mp4 in any player — no special tooling, because
mp4 plays everywhere and the review copy is never the shipped copy.

## Where assets end up

| Location | In git? | Purpose |
|---|---|---|
| `game/godot/Video/<Group>/` | ✅ yes | Approved `.ogv` clips (what Godot loads). |
| `.generate/videos/staging/` | ❌ no | Raw mp4 queued for review. |
| `.deprecated/assets/videos/` | ❌ no | Local archive of superseded variants. |
| `game/content/video/video-spec.json` | ✅ yes | The declarative manifest. |

## Where the code lives

| Component | File | Role |
|---|---|---|
| Manifest (data) | `game/content/video/video-spec.json` | Groups, entries, the look anchor, the status map. |
| Manifest types | `tools/contracts/interfaces/video-spec.interface.ts` | Types + zod schemas, the single source of truth. |
| Manifest loader | `tools/helpers/video-spec.helper.ts` | Read, write, resolve, find a slot, assert the anchor. |
| Encoder | `tools/helpers/video-encoder.helper.ts` | ffmpeg wrapper; refuses clearly when `libtheora` is missing. |
| Runner | `tools/scripts/content/generate-video.script.ts` | `stage / fetch / list / approve / deprecate / status / --validate`. |
| Runtime | `game/godot/Scripts/Shell/VideoStage.cs` | Plays a clip for a moment, skippable, missing file is normal. |
| Preference | `game/src/PCDynasty.Presentation.Core/PresentationPreferences.cs` | `PlaysVideo(moment)` — the runtime's only question. |

## Moments, and why every clip belongs to one

A clip that cannot be turned off is a clip that will be hated. Every group
declares a **moment**, and each moment has its own switch in the settings board's
`VIDEO` tab, under a master switch:

| Moment | Group | When |
|---|---|---|
| `intro` | `video.brand`, `video.frontend` | Opening sequence and the menu backdrop |
| `match` | `video.match` | The walk to the pitch, kick-off |
| `celebration` | `video.celebration` | Trophy, title, promotion |

`bun run game:video:validate` rejects a group with no moment, precisely so this
stays true.

## Rules the prompts follow

Every prompt in the manifest describes an **original, fictional football world**:

- **No legible text of any kind.** Burnt-in words cannot be translated, and the
  game ships in more than one language.
- No real club, competition, sponsor or badge.
- No recognisable faces. Figures are anonymous, silhouetted or out of focus.
- The shared look anchor — palette, camera, grade, what to avoid, aspect ratio —
  is appended to every prompt, so clips made months apart still belong to the
  same film. `--validate` rejects a group that frames differently.

## The game runs with no clips at all

`VideoStage` treats a missing file as normal: it finishes on the next frame and
the game continues. A fresh clone with an empty `game/godot/Video/` boots
straight to the menu. That is deliberate — the audio audit
([`a00041`](../mcp-vertex/proposals/review/a00041-audit-the-audio-pipeline-before-mirroring-it-for-video.md))
found a pipeline producing 65 assets that nothing in the game ever loaded, and the
cheapest way not to repeat it was to ship the consumer first.
