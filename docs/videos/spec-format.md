# The shape of `video-spec.json`

The manifest lives at `game/content/video/video-spec.json`. Its types and zod
validators are in `tools/contracts/interfaces/content/video-spec.interface.ts`; that file
is the source of truth and this page explains it.

## Top level

| Field | Meaning |
|---|---|
| `version` | Manifest version. Bump when the shape changes. |
| `root` | Where approved clips land — `game/godot/Video`. |
| `stagingDir` | Where generated clips wait for review. Gitignored. |
| `deprecatedDir` | Where superseded clips are archived. Gitignored. |
| `tool` | The generator command and the flags always passed to it. |
| `globalLookAnchor` | The look every clip shares. |
| `groups` | The clips we want. |
| `status` | The clips we have, keyed `group.entry`. |

## `globalLookAnchor`

The equivalent of the audio manifest's `globalMusicAnchor`, for the eye.

| Field | Example |
|---|---|
| `palette` | `deep navy and steel blue, warm gold highlights, floodlit whites` |
| `camera` | `slow deliberate moves, locked or gentle dolly, no handheld shake` |
| `grade` | `high contrast, slightly crushed blacks, cool shadows with warm key light` |
| `avoid` | `any legible text, real club badges, sponsor logos, recognisable faces` |
| `ratio` | `16:9` |

Every field is appended to every prompt at stage time. `ratio` is also enforced:
`--validate` fails if a group frames differently.

## A group

| Field | Meaning |
|---|---|
| `id` | `video.celebration` — the first half of a slot key. |
| `moment` | `intro`, `match` or `celebration`. Decides which settings switch governs the clip. |
| `outputDir` | Subdirectory under `root`. |
| `format` | Shipping container. Always `ogv` today; see the README. |
| `model` | Generator model id. |
| `ratio` | Must match the anchor. |
| `styleAnchor` | One sentence describing what this group is for. Appended to each prompt. |
| `coherence` | Free-form notes that keep entries in a group consistent. |
| `entries` | The clips. |

## An entry

| Field | Meaning |
|---|---|
| `id` | `trophy_lift` — the second half of a slot key. |
| `filename` | Basename of the shipped file, without extension. |
| `prompt` | What the clip shows. See the README for the rules. |
| `durationSeconds` | 4–15. The generator refuses anything outside that, so the schema does too. |

## The variant lifecycle

```text
requested ──fetch──▶ staged ──approve──▶ approved
    │                  │                    │
    └───────────── deprecate ───────────────┘
```

| State | Meaning |
|---|---|
| `requested` | Asked for with `--async`; a task id exists, no file yet. |
| `staged` | The mp4 is on disk in the staging tree, waiting to be watched. |
| `approved` | Transcoded to `.ogv` and committed under `root`. One per slot. |
| `deprecated` | Superseded or rejected. Archived, recoverable, never in git. |

Approving a variant automatically deprecates the one it replaces, so a slot has
at most one approved clip and the previous one is always recoverable.

## Validation

`bun run game:video:validate` checks:

1. The manifest parses against the zod schema.
2. Every group frames at the anchor's aspect ratio.
3. Every group declares a moment — so every clip can be switched off.
