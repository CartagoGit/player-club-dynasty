---
id: f00049
title: "Ship the video pipeline and rebuild settings as tabs"
kind: feat
status: in-progress
type: proposal
track: product
date: 2026-08-19
plan: q00001
audit-source: a00041
depends-on: f00048
---

# f00049 — Ship the video pipeline and rebuild settings as tabs

## Goal

Give the game the moments a manager remembers — an opening sequence, the walk to
the pitch, a trophy going up — through a video pipeline built in the image of the
audio one, and put every preference behind a tab so audio, video and graphics each
have a home instead of being stacked on one surface.

## why

Settings had four groups and already needed a scrollbar; with audio and video it
needs six, and the shape the reference itself used for exactly this problem is the
tab strip of its dossier. One strip, every option named, one open at a time.

The video side exists because the audio audit (`a00041`) found the failure worth
avoiding: 65 audio slots declared and nothing in the game that plays a sound. A
pipeline whose output nothing consumes looks finished and is not. So this one
ships its consumer first and its clips second.

## non-goals

- Generating the clips. The manifest declares what we want; producing and
  approving them is a content pass, not this proposal.
- An audio runtime. Wiring the audio buses to the new preferences needs its own
  proposal — `a00041` A5 records the gap.
- Fixing the audio pipeline's own defects. They are recorded in `a00041` and
  belong to the agent editing those files.

## Slices

- global_gate: e2e

### S1 — Extend the preferences with audio, video and their persistence
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/PresentationPreferences.cs`, `game/src/PCDynasty.Presentation.Core/PresentationPreferencesStore.cs`
- **Gate**: unit
- acceptance:
  - "`PresentationPreferences` is a record with init-only properties, so adding a
    preference is one line and no call site re-lists every field in order."
  - "Music and effects each carry a switch and a 0–100 level; video carries a
    master switch and one switch per moment (intro, match, celebration)."
  - "`PlaysVideo(moment)` answers the runtime's only question, with the master
    switch overriding the per-moment one."
  - "A preferences file written by an older build loads without migration: every
    new field falls back to its default."

### S2 — Add the tab strip and rebuild the board around it
- **Status**: done
- **Files**: `game/godot/Theme/TabStrip.cs`, `game/godot/Scripts/Shell/OptionsBoard.cs`
- **Gate**: e2e
- acceptance:
  - "`TabStrip` is the component the grammar documents (`04` §3.15): tabs are
    plain buttons with text and no children, the active one carries the area
    colour at full strength."
  - "The board shows one tab at a time, filling the page, with the edge rail and
    the command bar unchanged."
  - "Callers still register a section with a title and a `Control`; each becomes a
    tab. The board learns nothing about what a tab contains."
  - "Tabs are addressed by stable node name, never by index."

### S3 — Give each kind of option its own tab
- **Status**: done
- **Files**: `game/godot/Scripts/Shell/SettingsSections.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/OptionsSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/ShippedLocaleFiles.cs`, `game/godot/locale/langs/built/en.json`, `game/godot/locale/langs/built/es.json`
- **Gate**: e2e
- acceptance:
  - "Six tabs: language, theme, graphics, audio, video, data."
  - "The audio tab has a switch and a level per bus; the level stays usable while
    the bus is off."
  - "The video tab greys out the per-moment switches when the master is off, so it
    never offers a choice that would not be honoured."
  - "Every caption comes from the locale file; both shipped languages carry the
    new strings and the generated JSON is regenerated."
  - "The preference builders live in their own class: the menu controller routes,
    it does not also implement a preferences form."

### S4 — Build the video pipeline in the image of the audio one
- **Status**: done
- **Files**: `tools/contracts/interfaces/video-spec.interface.ts`, `tools/helpers/video-spec.helper.ts`, `tools/helpers/video-encoder.helper.ts`, `tools/scripts/content/generate-video.script.ts`, `game/content/video/video-spec.json`, `package.json`
- **Gate**: unit
- acceptance:
  - "Same verbs as audio — stage, list, approve, deprecate, status, --validate —
    plus `fetch`, because video generation is asynchronous and a variant can be
    in flight with no file yet."
  - "The runner imports the shared contract and helper instead of re-declaring the
    manifest shape, which is the drift `a00041` A9 records on the audio side."
  - "Approve transcodes to Ogg Theora, because that is what Godot plays without an
    addon, and refuses with a clear message when the local ffmpeg has no
    `libtheora`."
  - "A global look anchor — palette, camera, grade, what to avoid, aspect ratio —
    keeps clips made months apart in the same film, and `--validate` rejects a
    group that frames differently."
  - "`--validate` also rejects a group that belongs to no moment, because a clip
    the settings board cannot switch off is a clip that cannot be skipped."
  - "Every prompt describes an original fictional world and forbids legible text,
    real badges, sponsors and recognisable faces."

### S5 — Ship the consumer before the content
- **Status**: done
- **Files**: `game/godot/Scripts/Shell/VideoStage.cs`
- **Gate**: e2e
- acceptance:
  - "`VideoStage` plays a clip full-screen for a moment and always ends by raising
    `Finished`, so a caller can await it unconditionally."
  - "A missing clip is normal, not an error: the stage finishes on the next frame
    and nothing is logged as broken. The game boots to the menu with no clips
    approved."
  - "Any key, click or pad button skips."
  - "The preference is checked before the file is looked for."

### S6 — Prove it in the smoke test
- **Status**: done
- **Files**: `game/godot/Tests/SmokeBoot.gd`, `game/godot/Tests/CaptureShots.gd`
- **Gate**: e2e
- acceptance:
  - "The smoke waits for all six tabs, then opens each one and asserts its plate
    is shown and every other is hidden."
  - "It asserts the audio tab carries both levels, and that turning the video
    master off greys out the per-moment switches."
  - "The capture tool renders the audio, video and graphics tabs so the result is
    reviewed by looking at it."

### S7 — Document the pipeline
- **Status**: pending
- **DependsOn**: [S4]
- **Files**: `docs/videos/README.md`, `docs/videos/spec-format.md`
- **Gate**: unit
- acceptance:
  - "A README with the same shape as `docs/audios/README.md`: the pipeline at a
    glance, where assets end up, where the code lives, and the review workflow."
  - "The Ogg Theora constraint is stated where someone will read it before
    wondering why their mp4 does not play."

## acceptance

- Settings offers one tab per kind of option, every control works, and every
  change survives a restart.
- The video manifest validates, and the runner refuses a spec that would produce
  unskippable or mismatched clips.
- The game boots and plays with no clips approved.
- No third-party asset, name or likeness enters the repository or the prompts.

## notes

The audit that preceded this proposal is `a00041`. Its finding A5 — a pipeline
whose output nothing consumes — is the reason S5 exists and ships before any clip
is generated.

Fixing `bun run typecheck`, which had been red since the plugin's domain-grouping
refactor, was a precondition for any of this and is recorded in `a00041` S3.
