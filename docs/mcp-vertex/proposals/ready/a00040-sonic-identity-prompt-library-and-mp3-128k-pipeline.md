---
id: a00040
title: "Audio sonic-identity prompt library and mp3 128k staging pipeline"
kind: audio
status: ready
type: proposal
track: quality
date: 2026-08-19
---

# a00040 — Audio sonic-identity prompt library and mp3 128k staging pipeline

## Goal

Replace the implicit per-slot prompts in `game/content/audio/audio-spec.json`
with a checked-in prompt library under `.generate/prompts/audios/` that:

1. Defines one global **sonic identity** for PC Dynasty (the shared 5-note
   motif C-E-G-A-G, the late-1990s Spanish PC game aesthetic, the loop
   design rules, the crossfade-compatibility contract).
2. Defines a per-group **variant identity** (the mood / tempo / arrangement
   that all slots in `music.menu`, `music.matchday`, etc. share).
3. Defines a per-entry **specific instruction** (the actual brief for that
   one screen — "main menu", "trophy lift", "ui_click", etc.).
4. Concatenates the three layers into the final mmx prompt at generation
   time, so every audio asset is consistent with the global DNA without
   each per-entry prompt having to repeat the whole identity.

While we are at it, switch the staging format from `.wav` (1411 kbps PCM)
to `.mp3` at 128 kbps, the lowest bitrate mmx 1.0.19 accepts. The final
asset is still ogg vorbis at 22050 Hz q=2/q=3, which we already do in
`precompress-audio.script.ts`. The mp3 staging intermediate is much
smaller (10×) and faster to upload, download, and stage on disk.

## why

- The current `entry.prompt` strings in `audio-spec.json` are short
  one-liners ("main menu music", "victory stinger"). The output is
  inconsistent — different generations of "main menu" sound like they
  come from different games. A user listening to the 65 staged variants
  hears 65 different sonic worlds.
- PC Dynasty's musical identity should be unmistakable. Every track
  should reuse the same 5-note motif, the same instrumentation, the
  same loop design rules, so a player who hears trophy_lift_01
  immediately knows they're still in the same game as menu_main.
- Looping is currently a hope, not a requirement. The new
  `sonic-identity.txt` makes loop design a first-class rule: "start
  immediately on beat 1, end with the same energy, do not fade,
  resolve back to the tonic chord". The user is going to listen to
  these tracks for hours. A loop the brain detects as a loop is a
  loop the brain tunes out — and we need the opposite.
- The 5-note motif reused across every track (ascending for
  positive, descending for negative) is the most efficient way to
  give 65 different audio assets a single recognizable identity.
  The user explicitly asked for this in the design review.
- mmx 1.0.16 produced 1411 kbps WAV files (≈ 10 MB / minute) for
  every staged variant. With 69 variants the `.generate/audios/staging/`
  directory reached 1.6 GB. The mp3 128 kbps intermediate is
  ≈ 1 MB / minute, 10× smaller. The final ogg 22050 Hz q=2/q=3 in
  the shipped asset is ≈ 800 KB / minute. The user explicitly asked
  for "compressed as much as possible."
- The mmx API does not support ogg, so the pipeline must stay
  mp3 → ogg. The mp3 must be the lowest bitrate the API accepts to
  minimize the staging disk footprint.

## non-goals

- Generating new audio assets in this slice. The library is
  declarative — the user (or an agent) will re-stage and re-approve
  slots in a follow-up slice.
- Adding a per-variant override for the 5-note motif. The motif is
  shared by design; if a future track needs a different motif, that
  is a new audio asset, not a new prompt.
- Adding ogg to mmx. The mmx CLI does not support ogg output. We
  accept the mp3 → ogg intermediate as a hard requirement of the
  upstream tool.

## Slices

- content_run:audio

### S1 — Author the `.generate/prompts/audios/` library (declarative content)
- **Status**: ready
- **Files**:
  - `.generate/prompts/audios/_shared/sonic-identity.txt` (the global 5-note motif,
    style, sound palette, loop design rules, crossfade contract,
    AESTHETIC, AVOID list)
  - `.generate/prompts/audios/music.menu/_variant.txt` and
    `.generate/prompts/audios/music.menu/{menu_main,menu_credits,menu_loadsave}.txt`
  - `.generate/prompts/audios/music.matchday/_variant.txt` and
    `.generate/prompts/audios/music.matchday/{matchday_warmup,matchday_kickoff,
    matchday_goal,matchday_fulltime,matchday_victory,
    matchday_defeat}.txt`
  - `.generate/prompts/audios/music.workspace/_variant.txt` and 11 entry files
    (workspace_hub_01..03, market_browsing_01..03,
    stadium_management_01..03, standings_review_01..03)
  - `.generate/prompts/audios/music.editor/_variant.txt` and 3 entry files
  - `.generate/prompts/audios/music.trophy/_variant.txt` and 3 entry files
  - `.generate/prompts/audios/sfx.ui/_variant.txt` and 6 entry files
  - `.generate/prompts/audios/sfx.match/_variant.txt` and 9 entry files
  - `.generate/prompts/audios/sfx.workspace/_variant.txt` and 3 entry files
  - `.generate/prompts/audios/sfx.editor/_variant.txt` and 4 entry files
  - `.generate/prompts/audios/sfx.match_extended/_variant.txt` and 8 entry files
  - `.generate/prompts/audios/sfx.transfer/_variant.txt` and 4 entry files
  - `.generate/prompts/audios/sfx.achievement/_variant.txt` and 4 entry files
- **Gate**: type
- acceptance:
  - "Every music entry file references the shared 5-note motif
    C-E-G-A-G in either the entry itself or the per-group identity."
  - "Every sfx entry file references the shared 5-note motif (where
    musically possible — not for stadium sounds like whistles and
    crowd noise)."
  - "Every music entry file ends with instructions to begin and
    end on the C major tonic chord (the crossfade contract)."
  - "No prompt contains vocals, EDM drops, trap drums, cinematic
    scoring, fade-in/fade-out, or any other item in the AVOID
    list."
- review-state: ready
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: pending

### S2 — Teach the runner to read the layered prompts
- **Status**: ready
- **Files**: `tools/scripts/content/generate-audio.script.ts`
- **Gate**: type
- acceptance:
  - "The runner reads `.generate/prompts/audios/<group>/<entry>.txt` first; falls
    back to the `entry.prompt` from `audio-spec.json` if the file
    does not exist (backwards compatibility with v1)."
  - "The runner concatenates the per-entry prompt with the
    per-group `_variant.txt` (always) and the
    `_shared/sonic-identity.txt` (music only)."
  - "The final prompt is the single string passed to `mmx music
    generate --prompt ...`."
  - "The runner sets `--format mp3 --bitrate 128000` instead of
    `--format wav`."
  - "The runner's `--out` path now uses `.mp3` instead of `.wav`."
- review-state: ready
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: pending

### S3 — Update precompress to accept mp3 staging input
- **Status**: ready
- **Files**: `tools/scripts/content/precompress-audio.script.ts`
- **Gate**: type
- acceptance:
  - "The precompress reads `.mp3` and `.wav` (both) as input. The
    new mmx pipeline produces mp3, the legacy v1 produced wav."
  - "The output is still ogg vorbis at 22050 Hz q=2 (sfx) or
    q=3 (music) with the 65% size reduction already proven in
    a00032."
  - "The `--recompress-existing` flag still works on existing
    staged ogg files (no mp3 involvement)."
- review-state: ready
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: pending

### S4 — Document the new prompt layout in the audio README
- **Status**: ready
- **Files**: `docs/audios/README.md`, `docs/audios/mmx-reference.md`
- **Gate**: type
- acceptance:
  - "`docs/audios/README.md` describes the `.generate/prompts/audios/` layout
    and explains the 3-layer prompt composition."
  - "`docs/audios/mmx-reference.md` documents the new model
    (`music-3.0`), the new format/bitrate default (`mp3` 128
    kbps), and the new 5-note motif as the game's sonic DNA."
  - "The WSL audio note still works (the md in VS Code preview
    is silent but the server at `localhost:7333` plays via
    Brave/Chrome). No regression."
- review-state: ready
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: pending

## Notes

- The `.generate/prompts/audios/` directory is checked in (not gitignored). Every
  prompt is a human-readable plain text file that the user can edit
  and version-control.
- mmx 1.0.19 is the current version (we just updated from 1.0.16
  in this slice). Music 3.0 is now the default. The bitrate
  floor is 128 kbps for mp3. We documented this in mmx-reference.md.
- This slice is **declarative content + tool wiring**. It does not
  re-generate the 65 existing variants. A follow-up slice (or
  ad-hoc user request) will re-stage slots that the user wants
  to be aligned with the new sonic identity.
- The `--recompress-existing` flag we shipped in a00032 still
  works on oggs. It does not need to change for this slice.
- We do not touch `audio-spec.json`. The existing `entry.prompt`
  fields stay as the v1 fallback. The runner uses `.generate/prompts/audios/` when
  present, and falls back to the spec. This means we can roll
  this out without forcing a full re-stage of all 65 variants.
- The `audioPath` and `finalPath` fields in the spec are still the
  same. The user-visible review checklist (`REVIEW-CHECKLIST.md`)
  is unchanged. The new prompts only affect what mmx generates
  for the next `--variant` invocation.
