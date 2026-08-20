# mmx reference for PC Dynasty audio

> Everything the runner relies on when calling `mmx music generate`.
> Run `mmx <command> --help` for the authoritative live output.

## Service status note (2026-08-20)

MiniMax changed the availability of the legacy Music API on 2026-08-20.
The official docs now state:

- paid Music Generation / Lyrics Generation are no longer available to new users
- existing paying users can continue using the current API services
- the free music generation APIs are also being discontinued
- the suggested paths forward are `MiniMax Audio` or the open-source
	`MiniMax-Music3` model on Hugging Face

This matters because `mmx auth status` may still be valid while
`mmx music generate` returns HTTP 410. In other words:

- authentication can be OK
- general token quota can be OK
- the music endpoint can still be unavailable for the current account

The runner now surfaces this distinction explicitly instead of treating it as a
generic failure.

## Global flags the runner always sets

| Flag | Why |
|---|---|
| `--non-interactive` | Fail fast on missing args (no TTY prompts). |
| `--quiet` | Strip spinners / progress; stdout stays pure data so the plugin can parse it. |
| `--model <id>` | Slot-declared. `music-3.0` for both music and SFX-style stingers. |
| `--format mp3` | The runner stages to mp3 because the legacy mmx Music API does not output ogg. |
| `--bitrate 128000` | Lowest bitrate the current CLI/API accepts for mp3. |
| `--out <abs-path>` | Absolute path inside `.generate/audios/staging/<group>/<entry>/v_<ts>.mp3`. |
| `--prompt <text>` | Built from `.generate/prompts/audios/`: entry prompt + group identity + shared sonic identity (music only). |

## `mmx music generate` flags the runner uses

| Flag | Source | Notes |
|---|---|---|
| `--instrumental` | hardcoded | All PC Dynasty music is instrumental — no vocals. |
| `--bpm <number>` | `coherence.bpm` | A hint to the model. The load-bearing instruction is still in the prompt text. |
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
| `--sample-rate` | Not used by the runner. The current Music API accepts only a fixed set and did not behave consistently in our tests. |
| `--bitrate` | The runner fixes this at `128000` for staging mp3. Final shipping size is still controlled by ffmpeg in `precompress.script.ts`. |

## SFX generation path

The runner does **not** call `mmx speech synthesize` for SFX groups.
All current SFX-style stingers are routed through `mmx music generate --instrumental`
because the old speech path required text and did not map well to non-vocal UI
and match sounds.

For SFX groups the runner currently maps coherence hints like this:

| Spec field | mmx flag |
|---|---|
| `coherence.speed` | `--tempo "<n>x speed"` |
| `coherence.volume` | `--mood "volume <n>"` |
| `coherence['sound-effect']` | `--instruments <text>` |

## Prompt engineering rules baked into the spec

Every new prompt should be authored in `.generate/prompts/audios/` and the
runner will compose it at runtime from:

1. `.generate/prompts/audios/<group>/<entry>.txt`
2. `.generate/prompts/audios/<group>/_variant.txt`
3. `.generate/prompts/audios/_shared/sonic-identity.txt` (music only)

The legacy `entry.prompt` field in `audio-spec.json` remains as a fallback only.

### Music slots

```
<context sentence>. EXACTLY <bpm> BPM, key of <key> only (no modulations,
no chromatic notes). Begin and end on a <key> triad (<notes>).
<instruments sentence>. <mood sentence>. Include a 4-bar bridge at
the loop point that resolves back to <key> so this track can crossfade
seamlessly into <other-slot-ids>. <duration> seconds, loops seamlessly.
```

Example (`music.menu.menu_main`, condensed modern path):

```
MAIN MENU. Tempo 112–118 BPM. Energetic but relaxed. Start with a
memorable 4-bar synth hook using the 5-note motif C-E-G-A-G. Funky bass +
bright FM piano + crisp drums. The melody says: this is your club, the season
starts now.

--- GROUP IDENTITY ---

Music.menu group: title-screen and main navigation. Tempo 90–118 BPM. FM
electric piano + funky bass + soft brass. Same sonic identity as the rest of
the game. Start with a memorable 4-bar synth hook. Always loop seamlessly.

--- SHARED SONIC IDENTITY ---

PC Dynasty sonic identity (late-1990s Spanish PC football manager): motif
C-E-G-A-G, FM electric piano, funky electric bass, muted clean guitar,
bright digital synth lead, soft synth brass, electronic drums. Loop starts on
beat 1 and ends with the same energy so end-to-end playback feels continuous.
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
| Trim silence from a track | mmx generates fixed-length audio | Run `ffmpeg -af silenceremove=…` after precompress/approve. |
| Layer two sounds together | mmx is one prompt → one audio | Generate each layer separately, mix with `ffmpeg -filter_complex amix`. |
| Set a precise length | mmx is approximate | Pick the nearest prompt length and trim with `ffmpeg -ss / -t`. |
| Rebuild all slots when the endpoint is unavailable | auth can be valid while the endpoint is disabled for the account | Wait for paid entitlement, migrate to MiniMax Audio, or switch to the open-source MiniMax-Music3 model. |
| Make a vocal chant | mmx vocals need lyrics | Switch the slot to `music` with a lyrics file and `--vocals` style. |

## Verifying your changes locally

After editing `audio-spec.json` or `.generate/prompts/audios/`:

```bash
bun run game:audio:validate       # rejects anchor / coherence / orphan problems
bun run game:audio:stage music.menu.menu_main --variant test  # small dry-run
```

`--variant test` lets you keep the same slot id stable while testing different
prompts — the runner creates `v_<ts>__test.mp3` style staging paths when you
pass a suffix.