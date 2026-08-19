# PC Dynasty — Audio asset workflow (runtime side)

> Source of truth for every audio asset the game ships.
> This document is for humans. Agents and scripts read
> [`audio-spec.json`](../../content/audio/audio-spec.json) and never this file.
>
> Full documentation (manual for humans, mcp-vertex guide for agents,
> mmx reference, spec format) lives in [`docs/audios/`](../../../docs/audios/).

## Goal

Every music track must be **crossfaded into any other music track without the
player noticing a change**. Every SFX is **small, crisp and on-brand**. No
uncompressed wav ever ships in git.

## The slot / variant model

A **slot** is the semantic concept the game cares about — `sfx.ui.ui_click`,
`music.menu.menu_main`. The slot can hold many **variants** over its life:

| State        | Meaning                                                                                                                |
|--------------|------------------------------------------------------------------------------------------------------------------------|
| `staged`     | A fresh mp3 in `.generate/audios/staging/` waiting for the human to listen.                                              |
| `approved`   | The single variant that ships. Its encoded file lives in `game/godot/Audio/<Music|SFX>/<filename>`.                  |
| `deprecated` | A previously approved variant now archived in `.deprecated/assets/sounds/<group>/<entry>/` so we can resurrect it later. |

A slot has **exactly zero or one approved variant**. It can hold any number of
staged and deprecated variants.

## The review pipeline

The pipeline is split into four human-driven commands. The agent cannot skip
the listen step: it must wait for the human to call `approve` or
`deprecate` after staging.

### 1. `stage <slot>` — generate a candidate

The agent runs `bun run game:audio:stage -- sfx.ui.ui_click`. The runner
invokes `mmx` and writes the **uncompressed wav** to
`.generate/audios/staging/sfx.ui/ui_click/v_<timestamp>.mp3`. The variant is recorded
in `audio-spec.json#status` with `state: "staged"`.

The runner prints a "listen" hint pointing at the staged wav. The agent
shares that hint with the human and waits.

### 2. `listen <slot> --variant <id>` — preview

The runner prints the absolute path to the staged wav. The human opens the
file in their audio player of choice (VLC, mpv, browser, etc.) and decides.

### 3. `approve <slot> --variant <id>` — promote

The runner:

1. encodes the staged wav to the slot's target format (`ogg`, `opus`, etc.)
   using `ffmpeg`;
2. moves the encoded file to
   `game/godot/Audio/<Music|SFX>/<filename>`;
3. **archives the previously approved variant** — if any — to
   `.deprecated/assets/sounds/<group>/<entry>/<prevVariantId>__<timestamp>__<filename>`
   so it can be restored later;
4. flips the new variant to `approved` in the spec and stamps the size
   savings (e.g. `ratio: 12.4% of the wav`).

Only **one** approved variant per slot exists at any moment.

### 4. `deprecate <slot> --variant <id>` — archive without approving

If the human listens to a staged variant and doesn't like it, they run
`bun run game:audio:deprecate`. The runner moves the wav into
`.deprecated/assets/sounds/`, marks the variant as `deprecated` in the spec
and frees the slot for a fresh `stage` run.

The same command works on a currently approved variant: it archives the
encoded file too, leaving the slot **empty**. The next `stage` + `approve`
sequence gives you a brand new asset.

## Why the deprecation dir is gitignored

The deprecation dir is local-only. It exists to give the human **instant
undo** ("I changed my mind, give me the old ui_click back") without bloating
git. When a variant is approved and stable, it lives in
`game/godot/Audio/...` and is the only audio artefact that ships.

## Why the staging dir is gitignored

The staging wavs are **large, intermediate artefacts**. They exist only while
the human is reviewing. Once approved, the encoded `ogg` lands in git and
the wav can be deleted with:

```bash
bun tools/scripts/content/generate-audio.script.ts deprecate <slot> --variant <id>
```

…which moves it into the deprecation dir rather than just `rm`'ing it, so
the asset is still recoverable if you change your mind.

## Global music anchor (transitions without seams)

All music groups (`music.menu`, `music.matchday`, future `music.training`,
future `music.office`, …) share a single anchor defined at the top of
`audio-spec.json#globalMusicAnchor`:

| Field         | Value                                                                                                  |
|---------------|--------------------------------------------------------------------------------------------------------|
| `bpm`         | **96** (single, fixed value across every track)                                                         |
| `key`         | **C major (Ionian)**                                                                                    |
| `scaleNotes`  | `C D E F G A B` — no chromatic notes, no borrowed chords                                                |
| `loopSeconds` | **30** (so crossfades stay in time — 30/30/30…)                                                         |

Every prompt copies this boilerplate verbatim:

> EXACTLY 96 BPM, key of C major only. Begin and end on a C major triad
> (C-E-G). … Include a 4-bar bridge at the loop point that resolves back to
> C major. 30 seconds, loops seamlessly.

`bun run game:audio:validate` rejects any group whose `coherence.bpm` or
`coherence.key` drifts from the anchor — agents cannot break the rule.

## Tools the agent has access to

| mcp-vertex tool              | What it does                                                                                |
|------------------------------|---------------------------------------------------------------------------------------------|
| `audio_stage`                | Generates a new variant for a slot. Same as `game:audio:stage`.                              |
| `audio_list_variants`        | Lists every variant per slot, with wav / final paths.                                       |
| `audio_listen`               | Prints the absolute path of a staged variant so the human can preview it.                    |
| `audio_approve`              | Encodes + promotes a staged variant. Archives the previously approved one in `.deprecated/`. |
| `audio_deprecate`            | Moves any variant to `.deprecated/`. Frees the slot.                                         |
| `audio_status`               | One-line summary of every slot: approved id, # staged, # deprecated.                          |

## How an agent should extend the spec

1. Add a new entry to the appropriate group's `entries[]` (only the slot id,
   filename, and prompt — the status starts empty and gets populated by the
   pipeline).
2. Reuse the group's `coherence` block — copy `bpm`, `key`, `instruments`,
   `mood`, `intensity` from an existing entry.
3. Append the `EXACTLY 96 BPM` / `key of C major only` / `Begin and end on a
   C major triad` / `4-bar bridge that resolves back to C major` boilerplate
   to the prompt. Copy-paste it from any existing prompt — do **not**
   paraphrase, mmx is sensitive to wording.
4. Run `bun run game:audio:validate` — it will reject the entry if the
   `coherence` block drifts from the global anchor.