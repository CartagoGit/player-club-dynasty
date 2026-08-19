# `audio-spec.json` format reference

> The full TypeScript types live in
> [`tools/contracts/interfaces/audio-spec.interface.ts`](../../tools/contracts/interfaces/audio-spec.interface.ts).
> This document explains the *why* and the runtime semantics.

## Top-level shape

```typescript
interface IAudioSpec {
  version: number;
  root: string;                 // "game/godot/Audio" — relative to workspace root
  stagingDir?: string;          // defaults to ".generate/audios/staging"
  deprecatedDir?: string;       // defaults to ".deprecated/assets/sounds"
  tool: { command: string; nonInteractive: boolean; quiet: boolean };
  globalMusicAnchor?: IAudioGlobalMusicAnchor;
  groups: IAudioGroup[];
  status: Record<string, IAudioSlotStatus>;
}
```

| Field | Required | Default | Purpose |
|---|---|---|---|
| `version` | yes | — | Bump when the schema changes in a breaking way. |
| `root` | yes | — | Where the *approved* assets live (`game/godot/Audio/`). |
| `stagingDir` | no | `.generate/audios/staging` | Where raw wavs queue for review. **Must be gitignored.** |
| `deprecatedDir` | no | `.deprecated/assets/sounds` | Where superseded variants are archived. **Must be gitignored.** |
| `tool.command` | yes | — | Path to the audio CLI. Almost always `mmx`. |
| `tool.nonInteractive` | yes | — | Runner always passes `--non-interactive`. |
| `tool.quiet` | yes | — | Runner always passes `--quiet`. |
| `globalMusicAnchor` | yes if any music group exists | — | Single source of truth for music coherence. |
| `groups` | yes | — | Every audio group (music or sfx). |
| `status` | yes | starts `{}` | Populated by the runner; never hand-edit. |

## Slots

A slot is `<group.id>.<entry.id>`. Examples:

* `music.menu.menu_main`
* `sfx.ui.ui_click`
* `sfx.match.whistle_penalty`

`status["<group>.<entry>"]` holds the runtime state for that slot:

```typescript
interface IAudioSlotStatus {
  approvedVariantId: string | null;
  variants: Record<string, IAudioVariantRecord>;
}
```

There is **always zero or one** approved variant per slot. Variants are
created on `stage`, approved on `approve`, archived on `deprecate`.

## Variants

```typescript
interface IAudioVariantRecord {
  variantId: string;            // "v_1692345678" or "darker"
  state: 'staged' | 'approved' | 'deprecated';
  stagedAt: string | null;      // ISO 8601
  approvedAt: string | null;
  deprecatedAt: string | null;
  wavPath: string | null;       // workspace-relative
  finalPath: string | null;     // workspace-relative
  finalSizeBytes: number | null;
  wavSizeBytes: number | null;
  lastPrompt: string;
}
```

The state machine:

```text
                 ┌──────────────────────────┐
                 │                          │
       stage()   ▼   approve()              │  deprecate()
   ───►  [staged]  ─────────►  [approved]  ─────────────────► [deprecated]
                 ▲                              │
                 │                              │
                 └──────────── re-stage ────────┘
```

A variant can move from `approved` back to `staged` only by running
`stage` again with the same `--variant` id — useful when you tweak the
prompt for an existing slot.

## Groups

```typescript
interface IAudioGroup {
  id: string;                   // "music.menu"
  kind: 'music' | 'sfx';
  outputDir: string;            // "Music" or "SFX" (relative to `root`)
  format: 'ogg' | 'opus' | 'mp3' | 'wav';
  model: string;                // "music-3.0" or "speech-2.8-hd"
  styleAnchor: string;          // human-readable description of the group's tone
  coherence: Record<string, unknown>;
  entries: IAudioEntryDef[];
}
```

`outputDir` + `root` define where approved files land. With
`root = "game/godot/Audio"` and `outputDir = "Music"`, the approved
file goes to `game/godot/Audio/Music/<entry.filename>`.

## Entries (slots)

```typescript
interface IAudioEntryDef {
  id: string;                   // "menu_main" (joined with group.id to form the slot)
  filename: string;             // "menu_main.ogg" — what gets shipped
  prompt: string;               // the exact text sent to mmx
}
```

`filename` must include the extension that matches the group's `format`.

## Global music anchor

```typescript
interface IAudioGlobalMusicAnchor {
  bpm: number;                  // 96 in the shipped spec
  key: string;                  // "C major (Ionian)"
  scaleNotes: string[];         // ["C", "D", "E", "F", "G", "A", "B"]
  instruments: string;          // human-readable summary
  palette?: string;             // free-form mood description
  loopSeconds: number;          // 30
  transitionRules: string[];    // natural-language rules baked into the spec
}
```

### Why it exists

Every music slot must be **crossfaded** with every other music slot
when the manager moves between scenes (menu → matchday warmup →
matchday kickoff → menu again). Crossfades only sound seamless when the
two tracks share:

* the same tempo,
* the same key (no modulations, no chromatic notes),
* a loop point that resolves back to the tonic chord.

`globalMusicAnchor` encodes that single source of truth. The runner
refuses to stage a music group whose `coherence.bpm` or `coherence.key`
drifts from the anchor — that's how the invariant is enforced.

### Transition rules (the rules baked into every prompt)

The shipped spec has four `transitionRules`:

1. Every track begins and ends on the **C major tonic chord (C-E-G)**.
2. Every track carries a **4-bar bridge** at the loop point that
   resolves back to C major.
3. No track introduces **chromatic notes, modulations, or borrowed
   chords**.
4. No track uses **drums** (except soft brushed snare in `matchday`
   stingers).

These rules are also copied into every prompt via a boilerplate
sentence ("Begin and end on a C major triad", "Include a 4-bar bridge
at the loop point that resolves back to C major", etc.) — see
[mmx-reference.md](./mmx-reference.md#prompt-engineering-rules-baked-into-the-spec).

## Coherence block (per group)

Every group's `coherence` block is a flat `Record<string, unknown>`
that the runner projects into mmx flags:

| Group kind | mmx flag | Read from |
|---|---|---|
| music | `--bpm` | `coherence.bpm` (must equal `globalMusicAnchor.bpm`) |
| music | `--key` | `coherence.key` (must start with `globalMusicAnchor.key`) |
| music | `--instruments` | `coherence.instruments` |
| music | `--mood` | `coherence.mood` |
| music | `--tempo` | `coherence.tempo` (optional) |
| sfx | `--speed` | `coherence.speed` |
| sfx | `--volume` | `coherence.volume` |
| sfx | `--sound-effect` | `coherence['sound-effect']` (optional) |

Adding a new coherence key is safe — the runner just skips unknown
ones. Adding a key that **must** flow into an mmx flag is a small
change in [`tools/helpers/audio-spec.helper.ts`](../../tools/helpers/audio-spec.helper.ts) →
`buildCommand`.

## Status lifecycle

The `status` object is the runtime state. The runner owns it. Hand-editing
the spec is allowed (e.g. to flip a slot's `approvedVariantId`) but
disruptive — always run `bun run game:audio:validate` afterwards.

When the runner writes the spec, it preserves the on-disk JSON
shape (including any fields it doesn't know) by going through
`readAudioSpec` / `writeAudioSpec` only on the fields it controls.

## Validation

`bun run game:audio:validate` checks:

1. Every music group has `coherence.bpm` equal to the anchor.
2. Every music group has `coherence.key` starting with the anchor's key
   word (e.g. anchor key `"C major (Ionian)"` accepts `"C major"`).
3. No status slot references an entry that doesn't exist in any group.
4. If any music group is declared, `globalMusicAnchor` is present.

It **does not** check prompts — that's a human review job.