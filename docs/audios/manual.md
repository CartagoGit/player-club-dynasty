# Audio workflow — manual (humans)

> The human-facing guide. If you are an AI agent, read
> [delendai-usage.md](./delendai-usage.md) instead.

## TL;DR

```bash
bun run game:audio:stage sfx.ui.ui_click   # 1. candidate goes to .generate/audios/staging/
xdg-open .generate/audios/staging/sfx.ui/ui_click/v_<ts>.wav   # 2. listen
bun run game:audio:approve sfx.ui.ui_click --variant v_<ts>   # 3. ship it
```

Three commands, three directories, no surprises.

## Prerequisites

```bash
# Audio generation
mmx auth status              # must print "active" — register at https://platform.MiniMax.chat
ffmpeg -version              # any 4.x+ build with libvorbis + libopus + libmp3lame

# Repository
bun --version                # we use bun's runner; node ≥ 20 also works
```

The runner **fails fast** with a clear message when either is missing.

## The seven commands

| Command | What it does |
|---|---|
| `bun run game:audio:stage <slot> [--variant <id>]` | Generate a candidate wav in `.generate/audios/staging/`. Spinner shows elapsed seconds; the slot id is `group.entry`, e.g. `sfx.ui.ui_click`. |
| `bun run game:audio:list --all` | Markdown table of every slot with its approved / staged / deprecated variants. |
| `bun run game:audio:list <slot>` | Same, restricted to one slot. |
| `bun run game:audio:listen <slot> --variant <id>` | Print the absolute path of a staged wav so you can open it with any player. |
| `bun run game:audio:approve <slot> --variant <id>` | Encode the staged wav to the slot's target format, move it into `game/godot/Audio/<Music|SFX>/`, archive the previously approved variant in `.deprecated/`. |
| `bun run game:audio:deprecate <slot> --variant <id>` | Move any variant to `.deprecated/`, free the slot. |
| `bun run game:audio:status` | One-line summary per slot. |
| `bun run game:audio:validate` | Sanity-check `audio-spec.json` (anchor, coherence, status consistency). |

`<slot>` always looks like `<group>.<entry>` — see the full list in
[spec-format.md](./spec-format.md#slots).

## End-to-end recipe: replace a UI click

```bash
# 1. Generate two candidates for the same slot
bun run game:audio:stage sfx.ui.ui_click
# → ⠋ generating sfx.ui.ui_click (12.4s)
# → ✓ staged sfx.ui.ui_click variant v_1692345678 (2.3 MB wav)
# → listen with: bun run game:audio:listen sfx.ui.ui_click --variant v_1692345678

bun run game:audio:stage sfx.ui.ui_click --variant darker
# → ✓ staged sfx.ui.ui_click variant darker (2.1 MB wav)

# 2. Listen to both
xdg-open .generate/audios/staging/sfx.ui/ui_click/v_1692345678.wav
xdg-open .audio/staging/sfx.ui/ui_click/darker.wav

# 3. Approve the one you like; the other stays in staging (or you deprecate it).
bun run game:audio:approve sfx.ui.ui_click --variant darker
# → ✓ approved sfx.ui.ui_click variant darker
# →   wav:   .generate/audios/staging/sfx.ui/ui_click/darker.wav (2.1 MB)
# →   final: game/godot/Audio/SFX/ui_click.ogg (96.4 KB ogg)
# →   ratio: 4.5% of the wav

# 4. Clean up the rejected staging wav so it stops taking disk space
bun run game:audio:deprecate sfx.ui.ui_click --variant v_1692345678
```

That's it. Your approved `ui_click.ogg` is on disk and ready for Godot.

## Switching approved variants

If you change your mind later:

```bash
bun run game:audio:stage sfx.ui.ui_click --variant crisper
xdg-open .generate/audios/staging/sfx.ui/ui_click/crisper.wav
bun run game:audio:approve sfx.ui.ui_click --variant crisper
# → the previously approved (darker) is auto-archived in
#   .deprecated/assets/sounds/sfx.ui/ui_click/darker__2026-08-18T21-03-04Z__ui_click.ogg
```

## Recovering an old approved variant

The `.deprecated/` directory is local-only — it is **not** in git, so
each developer keeps their own archive. To restore an old approved
variant:

```bash
# 1. Find the archived file
ls .deprecated/assets/sounds/sfx.ui/ui_click/
# → darker__2026-08-18T21-03-04Z__ui_click.ogg

# 2. Move it back into the slot location
mv .deprecated/assets/sounds/sfx.ui/ui_click/darker__*.ogg game/godot/Audio/SFX/ui_click.ogg

# 3. Edit audio-spec.json so the slot's approvedVariantId points to that variant
#    (or just use git to revert the SFX file if it's the same variant)
```

## Troubleshooting

### "mmx failed: …"

Either you don't have mmx installed, your session token is stale, or
mmx rejected the prompt. Re-run with `--quiet=false` by editing
`audio-spec.json#tool.quiet` to see the full mmx output.

### "ffmpeg is required to encode the approved variant"

```bash
sudo apt install ffmpeg        # Debian / Ubuntu
brew install ffmpeg            # macOS
```

### "group <id> declares bpm=120 but anchor bpm=96"

All music groups **must** share the global music anchor — see
[spec-format.md](./spec-format.md#global-music-anchor). Edit the group's
`coherence.bpm` (and `coherence.key`) to match the anchor, or relax the
anchor if 96 BPM is not actually what you want.

### "slot <slot> has no staged variants"

You called `listen` / `approve` / `deprecate` on a slot that has no
variant with that id. Run `bun run game:audio:list <slot>` to see what
variants exist.

### The progress spinner doesn't move

mmx is silent for the entire generation (10–60 s). The spinner ticks
every 250 ms with elapsed seconds in parentheses — if it really hangs,
check that `mmx auth status` reports a valid token.

## Adding a new slot

Edit `game/godot/Audio/audio-spec.json` and append a new entry to the
appropriate group's `entries[]`:

```json
{
  "id": "crowd_banter",
  "filename": "crowd_banter.ogg",
  "prompt": "stadium crowd short rhythmic chant — two syllables repeated, ~3 seconds"
}
```

Then run `bun run game:audio:validate` — it will reject the entry if
its prompt is missing the coherence boilerplate or if the slot id
collides with an existing one.