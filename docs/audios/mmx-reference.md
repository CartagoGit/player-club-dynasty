# mmx reference for PC Dynasty audio

> Everything the runner relies on when calling `mmx music generate`
> and `mmx speech synthesize`. Run `mmx <command> --help` for the
> authoritative live output.

## Global flags the runner always sets

| Flag | Why |
|---|---|
| `--non-interactive` | Fail fast on missing args (no TTY prompts). |
| `--quiet` | Strip spinners / progress; stdout stays pure data so the plugin can parse it. |
| `--model <id>` | Slot-declared. `music-3.0` for music, `speech-2.8-hd` for SFX. |
| `--format wav` | **The runner always asks for wav on staging** — wav is uncompressed and is what ffmpeg encodes downstream. The slot's final format (ogg/opus/mp3) is decided at approve time. |
| `--out <abs-path>` | Absolute path inside `.generate/audios/staging/<group>/<entry>/v_<ts>.wav`. |
| `--prompt <text>` | Per-entry prompt from `audio-spec.json`. |

## `mmx music generate` flags the runner uses

| Flag | Source | Notes |
|---|---|---|
| `--instrumental` | hardcoded | All PC Dynasty music is instrumental — no vocals. |
| `--bpm <number>` | `globalMusicAnchor.bpm` | Must match across every music group; the runner refuses to stage a music group whose `coherence.bpm` drifts. |
| `--key <text>` | `globalMusicAnchor.key` | e.g. `"C major"`. Must match the anchor. |
| `--instruments <text>` | `coherence.instruments` | e.g. `"warm analog synth pads, electric piano (Rhodes)"`. |
| `--mood <text>` | `coherence.mood` | e.g. `"calm, inviting, contemplative"`. |
| `--tempo <text>` | `coherence.tempo` | Optional; mmx accepts free-form tempo descriptions like `"slow"`. |

Flags the runner does **not** pass but you may want when extending the
spec:

| Flag | Why you might want it |
|---|---|
| `--genre <text>` | mmx understands genre hints like `electronic` or `folk` — useful when the slot's style anchor leaves it ambiguous. |
| `--use-case <text>` | Helps mmx weight the mix toward a use case. |
| `--structure <text>` | Useful if a music slot needs a specific shape (e.g. `"intro-verse-outro"`). |
| `--vocals <text>` | Skip — we always pass `--instrumental`. |
| `--references <text>` | For matching a particular palette — e.g. `"similar to late 90s FIFA menus"`. |
| `--avoid <text>` | Tell mmx what **not** to include, e.g. `"drums, distorted guitars"`. |
| `--aigc-watermark` | Adds an inaudible watermark for provenance. Not used by the runner by default — enable per-slot if needed. |
| `--sample-rate / --bitrate` | Not used by the runner; ffmpeg re-encodes downstream with the libavcodec defaults. |

## `mmx speech synthesize` flags the runner uses

> **Important (2026-08-18):** the runner no longer calls
> `mmx speech synthesize` for SFX groups. `speech-2.8-hd` requires
> `--text`, which makes no sense for non-vocal UI / match stingers.
> The runner now pipes SFX through `mmx music generate --instrumental`
> (same as music groups), with the SFX `coherence.speed`/`volume`/
> `sound-effect` mapped to music-style flags (`--tempo`, `--mood`,
> `--instruments`). The slot prompt is the load-bearing input.

| Flag (was: speech) | Flag (now: music) | Source | Notes |
|---|---|---|---|
| `--speed <n>` | `--tempo <text>` (`"<n>x speed"`) | `coherence.speed` | Multiplier on the default speech rate. `1` is unchanged. |
| `--volume <n>` | `--mood <text>` (`"volume <n>"`) | `coherence.volume` | `0.9` keeps headroom for mixing with music. |
| `--sound-effect <name>` | `--instruments <text>` | `coherence['sound-effect']` (optional) | Adds a built-in effect (e.g. `ui_chime`). mmx-specific. |

The SFX `model` field in `audio-spec.json` should be `music-3.0`,
**not** `speech-2.8-hd` — see the audio-spec.json edit history.

## Prompt engineering rules baked into the spec

Every prompt in `audio-spec.json` follows these rules. Copy them
verbatim when adding new entries — mmx is sensitive to phrasing and
deviating hurts consistency across variants.

### Music slots

```
<context sentence>. EXACTLY <bpm> BPM, key of <key> only (no modulations,
no chromatic notes). Begin and end on a <key> triad (<notes>).
<instruments sentence>. <mood sentence>. Include a 4-bar bridge at
the loop point that resolves back to <key> so this track can crossfade
seamlessly into <other-slot-ids>. <duration> seconds, loops seamlessly.
```

Example (`music.menu.menu_main`):

```
Title screen theme for a retro 1990s football manager PC game.
EXACTLY 96 BPM, key of C major only (no modulations, no chromatic
notes). Begin and end on a C major triad (C-E-G). Warm analog synth
pad, soft Rhodes electric piano, pizzicato strings — no drums, no
percussion. Sparse melodic motif, contemplative and inviting. Include
a 4-bar bridge at the loop point that resolves back to C major so
this track can crossfade seamlessly into menu_credits, menu_loadsave
or any music.matchday entry. 30 seconds, loops seamlessly.
```

### SFX slots

```
<short description>. <timing hint>. <branding hint>.
```

Example (`sfx.ui.ui_click`):

```
soft UI click — short and dry, like a finger tapping a membrane
keyboard
```

Example (`sfx.match.whistle_penalty`):

```
penalty award whistle — three short sharp blasts in quick succession
```

## What you cannot do via mmx (and what to do instead)

| What you want | Why mmx can't | What to use instead |
|---|---|---|
| Trim silence from a track | mmx generates fixed-length audio | Run `ffmpeg -af silenceremove=…` after approve. |
| Layer two sounds together | mmx is one prompt → one audio | Generate each layer separately, mix with `ffmpeg -filter_complex amix`. |
| Set a precise length | mmx is approximate | Pick the nearest prompt length and trim with `ffmpeg -ss / -t`. |
| Re-prompt a single slot's exact same wav | mmx is non-deterministic | Same prompt + `--seed N` (only on `music cover`, not on `music generate`). |
| Make a vocal chant | mmx vocals need lyrics | Switch the slot to `music` with a lyrics file and `--vocals` style. |

## Verifying your changes locally

After editing `audio-spec.json`:

```bash
bun run game:audio:validate       # rejects anchor / coherence / orphan problems
bun run game:audio:stage music.menu.menu_main --variant test  # small dry-run
```

`--variant test` lets you keep the same slot id stable while testing
different prompts — the runner creates `v_<ts>__test.wav` style paths
when you pass a suffix.