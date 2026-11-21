---
id: d00002
title: "Play through PC Fútbol 7's Liga Manager and Liga Promanager screen by screen, documenting every button"
kind: docs
status: ready
type: proposal
track: knowledge
date: 2026-08-21
---

# d00002 — Play through PC Fútbol 7's Liga Manager and Liga Promanager screen by screen, documenting every button

## Goal

Exhaustively click through PC Fútbol 7's Liga Manager, Liga Promanager and
surrounding main-menu modes inside a preserved VirtualBox VM, mirroring the
methodology and per-screen checklist discipline of
[`d00001`](../review/d00001-play-through-liga-manager-and-liga-promanager-screen-by-screen-documenting-every-button.md),
and documenting every screen, button and distinct behavior in
`docs/wiki/in-game/pcf7/*.md`. PC Fútbol 7 is confirmed to have *more*
surface than 6.0, not less — see
[`00-menu-principal.md`](../../../wiki/in-game/pcf7/00-menu-principal.md):
same 3×3 grid/rail/bottom-bar structure, but new modes replace or add to
6.0's (Simulador Mundial instead of Partido Amistoso, Euro PCFútbol instead
of Proquinielas 2.0, a new "Guía Rápida" meta-action) — each of these needs
its own play-through, not an assumption that it behaves like its 6.0
counterpart just because the slot in the grid looks similar. Whenever a
match/simulation config screen appears, always choose simulate-by-result
(`RESULTADO`), never watch-the-match (`VISIONADO`) — same hard constraint
as d00001.

## why

d00001 built a screen-by-screen play-through of PC Fútbol 6.0, and that
work directly informs PC Dynasty's equivalence map (f00047). But 6.0 and
7.0 are both target versions per
[`docs/wiki/pcfutbol/13-version-objetivo-6-0-y-7-0.md`](../../../wiki/pcfutbol/13-version-objetivo-6-0-y-7-0.md) —
documenting only 6.0 risks silently missing whatever 7.0 added or changed,
which the user's own comparison table in `00-menu-principal.md` already
shows is non-trivial (new modes, a new meta-action, different build
number). Only one file exists under `docs/wiki/in-game/pcf7/` so far
(the main menu, captured 2026-08-20); the despacho and every mode inside it
are still completely undocumented for this version.

## non-goals

- Copying any asset, artwork, screenshot pixel-source, or trademarked name
  into the repo beyond the own-taken screenshots already permitted by
  `docs/wiki/in-game/README.md`.
- Deciding what PC Dynasty actually implements from this — that stays
  f00047 S4's equivalence map job.
- Re-documenting behavior already fully confirmed identical between 6.0 and
  7.0 (e.g. the despacho's 2×4 grid layout, if a first pass confirms it's
  pixel-identical) — the goal is to find and document *differences*, not
  duplicate d00001's prose for screens that turn out to be unchanged. Where
  a screen is confirmed identical, a short pointer to the equivalent
  `pcf6/*.md` section is enough; full new prose is only needed where 7.0
  actually differs.
- Playing out full simulated matches — every match/config screen must be
  driven to 'simulación por resultado', never 'ver partido', per explicit
  user instruction (same constraint as d00001).

## Slices

- global_gate: none

### S1 — Confirm the VM/tooling setup works for the PC Fútbol 7 install and reach Liga Manager's despacho
- **Status**: ready. The 6.0 disc auto-launches into its own menu; 7 does
  not (see `00-menu-principal.md`'s launch note: Inicio → Programas → PC
  Fútbol 7 → "PCFútbol 7 sin intro"). This slice exists so a fresh agent
  doesn't waste a session rediscovering that launch path, and so the first
  real difference check (does 7's despacho look/behave like 6.0's 2×4 grid?)
  is answered before deeper slices assume it.
- **Files**: `docs/wiki/in-game/pcf7/00-menu-principal.md`,
  `docs/wiki/in-game/pcf7/07-despacho.md` (new),
  `docs/wiki/in-game/como-esta-montado.md` (only if 7 needs any tooling
  note beyond the existing 6.0 one — e.g. a different launch command or
  window title to Alt+Tab against)
- **Gate**: none
- acceptance:
  - "PC Fútbol 7 is launched from a cold VM boot via the documented Inicio
    → Programas path, and Liga Manager's despacho is reached and screenshot."
  - "The despacho's button grid is compared directly against 6.0's
    (`pcf6/07-despacho.md`) and every difference (layout, button set,
    labels) is called out explicitly, not just described as if it were 6.0."

### S2 — Liga Manager: despacho and every reachable sub-screen (mirroring d00001 S1/S4)
- **Status**: ready. Same acceptance shape as d00001's S1 and S4 combined,
  applied to 7.0: RESULTADOS, CALENDARIO, ALINEACIÓN, TÁCTICAS, VER RIVAL,
  CAJA, DECISIONES, ESTADIO, PLANTILLA, FICHAR, EMPLEADOS, CLASIFICACIÓN —
  every despacho button needs its own documented outcome, with explicit
  callouts wherever 7.0 differs from the already-documented 6.0 behavior
  (new fields, new sub-screens, or a screen that's gone/renamed).
- **Files**: one new file per despacho destination under
  `docs/wiki/in-game/pcf7/`, following d00001's numbering scheme (mirror
  `pcf6/07-despacho.md` through `pcf6/22-promanager-despacho.md`'s slot
  numbers where the equivalent screen exists in 7.0).
- **Gate**: none
- acceptance:
  - "Every despacho button reachable in 7.0 has a documented outcome, cross-
    referencing its 6.0 counterpart and stating explicitly whether it's
    identical, changed, or new."
  - "Any match/season-advance flow encountered is driven to a result-only
    simulation, never a played-out match."

### S3 — The modes that actually changed from 6.0: Simulador Mundial, Euro PCFútbol, Guía Rápida
- **Status**: ready. These are the three concrete, already-identified
  differences from `00-menu-principal.md`'s comparison table — the modes
  most likely to contain genuinely new mechanics PC Dynasty's equivalence
  map doesn't yet know about, so they should not wait for S2 to fully
  finish if time is tight.
- **Files**: `docs/wiki/in-game/pcf7/` new files for each (numbering after
  whatever S2 establishes; follow 6.0's convention of one file per mode).
- **Gate**: none
- acceptance:
  - "Simulador Mundial (replacing 6.0's Partido Amistoso slot) is played
    through at least to its own mini-hub or setup screen and documented."
  - "Euro PCFútbol (replacing 6.0's Proquinielas 2.0 slot) is opened and
    documented, including whether it crashes to desktop the way Proquinielas
    2.0 does in 6.0 (see `pcf6/00-menu-principal.md`) or is fixed in 7.0."
  - "Guía Rápida (the new bottom-bar meta-action with no 6.0 equivalent) is
    opened and documented — this is the strongest existing candidate for
    the `HoverReadout`/contextual-help gap already flagged in
    `mapa-de-equivalencias.md`."

### S4 — Liga Promanager in 7.0
- **Status**: ready. Mirrors d00001 S2: confirm the manager-name/keyboard-
  focus mechanic still applies (or has changed), reach Liga Promanager's
  own despacho, and diff it against both 6.0's Promanager despacho and this
  proposal's own S2 findings for 7.0's Liga Manager despacho.
- **Files**: `docs/wiki/in-game/pcf7/` new files for the offer-selection
  screen and Promanager's despacho.
- **Gate**: none
- acceptance:
  - "Liga Promanager's offer-selection screen is reached and its mechanic
    confirmed (or confirmed blocked with methods tried listed)."
  - "Liga Promanager's despacho is reached and compared against both 6.0's
    Promanager despacho and this proposal's Liga Manager despacho."

## acceptance

- Every mode in PC Fútbol 7's main-menu 3×3 grid (Historia, Base de Datos,
  Seguimiento, 100 Mejores Goles 97-98, Simulador Mundial, Desafío América-
  Europa, Liga Manager, Liga Promanager, Euro PCFútbol) plus the bottom-bar
  meta-actions (Proquinielas 3, Infofútbol, Guía Rápida, Créditos, Salir)
  has at least a first-pass documented outcome.
- Every despacho button inside Liga Manager (and Liga Promanager's own
  despacho, if it differs) has a documented outcome.
- Every screen documented here explicitly states whether it's identical to,
  a variant of, or wholly new versus its PC Fútbol 6.0 counterpart in
  `docs/wiki/in-game/pcf6/` — no screen should be written up as if 6.0's
  play-through didn't already exist.
- Any match/season-advance flow encountered is driven to a result-only
  simulation, never a played-out match, per user instruction.
- This proposal is not marked `done` while any of its slices still have an
  open acceptance item — partial progress should move slices to
  `in-progress`, not close the whole proposal early.
