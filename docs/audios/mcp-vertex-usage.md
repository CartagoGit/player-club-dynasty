# Audio workflow — mcp-vertex usage (AI agents)

> The agent-facing guide. Humans read [manual.md](./manual.md) instead.

## TL;DR

Five tools. The decision tree:

```text
user says "I want an X for slot <slot>"
    │
    ▼
audio_stage(<slot>)            ← blocks until mmx returns
    │
    ▼
audio_listen(<slot>, <variantId>)   ← returns the wav path; share with user
    │
    ▼
┌────────────────────────────────┐
│ user says "approve"            │──► audio_approve(<slot>, <variantId>)
│ user says "I don't like it"    │──► audio_deprecate(<slot>, <variantId>)
│ user says "give me another"    │──► audio_stage(<slot>) → loop
└────────────────────────────────┘
```

The agent **never** calls `audio_approve` without the human saying so.

## The five tools

| Tool | Purpose | When to call |
|---|---|---|
| `player-club-dynasty_audio_stage` | Generate a candidate wav | User asks for a new sound, or "give me another". |
| `player-club-dynasty_audio_list_variants` | List every slot with its variants | User asks "what do we have?", or at the start of a session to ground yourself. |
| `player-club-dynasty_audio_listen` | Return the absolute path of a staged variant | Right after `audio_stage`, to give the human something to preview. |
| `player-club-dynasty_audio_approve` | Encode + promote a variant | User explicitly says "I like this one / approve / ship it". |
| `player-club-dynasty_audio_deprecate` | Archive a variant | User says "I don't like it / skip / discard". |

You will not normally need the mcp-vertex plugin's own `validate`
command — the runner validates `audio-spec.json` on every invocation
and surfaces failures as `isError: true`.

## Required reading before you start

* [mmx-reference.md](./mmx-reference.md) — every flag the runner sets,
  and the prompt boilerplate you must copy verbatim when extending a slot.
* [spec-format.md](./spec-format.md) — the exact shape of the spec,
  what `globalMusicAnchor` enforces, what counts as a "slot".

## The contract you must keep

1. **Never auto-approve.** Even if the wav looks plausible by name,
   the human must listen before you promote it. Approval is a human
   decision.
2. **Never regenerate a variant that is `approved` without asking.**
   If the user wants a different vibe, stage a new variant; don't
   overwrite the one they already approved.
3. **Always share the `wavPath` returned by `audio_listen`** so the
   human can preview it in their player. Do not assume they will run
   the command themselves — paste the path into your reply.
4. **Treat `globalMusicAnchor` as load-bearing.** Music slots that drift
   from the anchor (different bpm / key / palette) will silently break
   the crossfade between scenes. If you add a new music group, copy the
   anchor's `bpm`, `key`, `instruments`, `loopSeconds` exactly into the
   group's `coherence` block.
5. **When extending `audio-spec.json`, run `audio_validate`-equivalent
   first** (or call the CLI `bun tools/scripts/content/audio/generate.script.ts
   --validate`). The runner rejects orphan status slots, groups that
   drift from the anchor, etc.

## Example dialogue

> **User:** "Make the goal celebration sting more triumphant."
>
> **Agent (thinking):** The slot is `music.matchday.matchday_goal`. The
> approved variant is `v_1692340000`. I should stage a new variant
> without touching the approved one.
>
> **Agent:** `player-club-dynasty_audio_stage { fullKey: "music.matchday.matchday_goal" }`
>
> **Tool response:** `{ fullKey: "music.matchday.matchday_goal", variantId: "v_1692345678", wavPath: "/.../.generate/audios/staging/music.matchday/matchday_goal/v_1692345678.wav", listenCommand: "bun tools/scripts/content/audio/generate.script.ts listen ..." }`
>
> **Agent:** "Generated a new variant at
> `/.../.generate/audios/staging/music.matchday/matchday_goal/v_1692345678.wav`.
> Listen with `xdg-open <path>` and tell me if you want to approve it
> or replace it."
>
> **User:** "Approve it."
>
> **Agent:** `player-club-dynasty_audio_approve { fullKey: "music.matchday.matchday_goal", variantId: "v_1692345678" }`
>
> **Tool response:** `{ fullKey: "music.matchday.matchday_goal", variantId: "v_1692345678", finalPath: "game/godot/Audio/Music/matchday_goal.ogg", wavSizeBytes: 2210000, finalSizeBytes: 96000, archivedPreviousVariantId: "v_1692340000" }`
>
> **Agent:** "Done. The new sting is now `game/godot/Audio/Music/matchday_goal.ogg`
> (96 KB ogg — 4.3 % of the wav). The previous one (`v_1692340000`)
> is archived in `.deprecated/assets/sounds/music.matchday/matchday_goal/`
> if you ever want to restore it."

## How the tools are wired

```
agent
  │
  ▼
mcp-vertex host
  │ (spawns)
  ▼
bun tools/scripts/content/audio/generate.script.ts <sub-command> ...
  │ (uses)
  ├── tools/helpers/content/audio/spec.helper.ts    ── read / write / findSlot
  ├── tools/helpers/content/audio/encoder.helper.ts ── ffmpeg encode
  └── tools/helpers/system/progress.helper.ts      ── spinner for the slow bits
```

The plugin tools themselves are thin shells over the runner — they
spawn `bun tools/scripts/content/audio/generate.script.ts` with the
matching sub-command and parse stdout / stderr.

## Common pitfalls

* **Calling `audio_listen` for a variant the user hasn't been given.**
  `audio_listen` is a passthrough — it returns the wav path that was
  already printed by `audio_stage`. Don't use it as a "search".
* **Running `audio_stage` in parallel for the same slot.** The runner
  gives each variant a unique id only when `--variant` is omitted, so
  two parallel stages with default ids may collide on the same wav
  filename. Always pass `--variant` if you intend to parallelise.
* **Skipping the "share the path" step.** The human needs the path
  *now*, not after you've already moved on to the next step.

## When in doubt

If `audio_stage` returns `isError: true`, **read the error message
verbatim** — the runner is the single source of truth for what is
allowed. Common causes:

* "group X not declared in spec" → you typo'd a slot id. Call
  `audio_list_variants` to see the canonical ids.
* "entry Y not declared in group X" → same, the slot id's group is
  right but the entry part is wrong.
* "variant V is deprecated; re-stage it first" → you're trying to
  approve an already-deprecated variant. Stage a new one.
* "ffmpeg is required …" → the host machine doesn't have ffmpeg.
  Tell the user.