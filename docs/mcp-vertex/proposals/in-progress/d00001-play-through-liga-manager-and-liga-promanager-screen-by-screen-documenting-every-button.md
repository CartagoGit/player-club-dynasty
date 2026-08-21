---
id: d00001
title: "Play through Liga Manager and Liga Promanager screen by screen, documenting every button"
kind: docs
status: in-progress
type: proposal
track: knowledge
date: 2026-08-21
---

# d00001 — Play through Liga Manager and Liga Promanager screen by screen, documenting every button

## Goal

Exhaustively click through PC Fútbol 6.0's Liga Manager, Liga Promanager and surrounding main-menu modes inside the VirtualBox VM (see docs/wiki/in-game/como-esta-montado.md for the bun run vm:* tooling), documenting every screen, button and distinct behavior in docs/wiki/in-game/pcf6/*.md as it's found, with a running per-screen checklist so progress survives a context compaction. Priority order per explicit user instruction: Liga Manager and Liga Promanager first (their menus, despacho, and every reachable sub-screen), main-menu misc modes (Historia, Base de Datos, Desafío América-Europa, etc.) second. Whenever a match/simulation config appears, always pick simulate-by-result, never watch-the-match.

## why

f00047 (in review) built the classic-manager knowledge base from written sources, screenshots and inference. It has no slice for actually piloting the preserved PC Fútbol 6.0 VM and recording what each screen and button does firsthand — that live-exploration work started ad hoc under docs/wiki/in-game/ across several sessions and needs its own tracked checklist so a fresh agent (e.g. after a context compaction) can resume exactly where it left off without re-deriving the VM setup, the calibration approach, or which screens/buttons are already covered.

## non-goals

- Copying any asset, artwork, screenshot pixel-source, or trademarked name into the repo beyond the own-taken screenshots already permitted by docs/wiki/in-game/README.md.
- Deciding what PC Dynasty actually implements from this — that stays f00047 S4's equivalence map job.
- Playing out full simulated matches — every match/config screen must be driven to 'simulación por resultado', never 'ver partido', per explicit user instruction.

## Slices

- global_gate: none

### S1 — Liga Manager: despacho and every reachable sub-screen
- **Status**: done. All 12 grid buttons + SALIR/GRABAR LIGA/NOTICIAS/SEGUIR documented; ESTADIO's REMODELAR (all 4 categories), TÁCTICAS (5 sub-buttons), FICHAR/PLANTILLA (ESTADO OFERTAS/SECRETARIO/JUVENILES) all pulled to depth. Open items tracked per-file: CAJA's 4 tabs (likely not implemented, see `18-caja.md`), GRABAR TÁCTICA's actual save action (deliberately unrisked, see `16-tacticas.md`).
- **Files**: `docs/wiki/in-game/pcf6/07-despacho.md`, `docs/wiki/in-game/pcf6/08-personal-del-club.md`, `docs/wiki/in-game/pcf6/09-clasificacion.md`, `docs/wiki/in-game/pcf6/13-resultados.md`, `docs/wiki/in-game/pcf6/14-calendario.md`, `docs/wiki/in-game/pcf6/15-alineacion.md`, `docs/wiki/in-game/pcf6/16-tacticas.md`, `docs/wiki/in-game/pcf6/17-ver-rival.md`, `docs/wiki/in-game/pcf6/18-caja.md`, `docs/wiki/in-game/pcf6/19-decisiones.md`, `docs/wiki/in-game/pcf6/20-estadio.md`, `docs/wiki/in-game/pcf6/21-plantilla-fichar.md`
- **Gate**: none
- acceptance:
  - "Every despacho button (RESULTADOS, CALENDARIO, ALINEACIÓN, TÁCTICAS, VER RIVAL, CAJA, DECISIONES, ESTADIO, SALIR, GRABAR LIGA, NOTICIAS, SEGUIR, PLANTILLA, FICHAR) has a documented outcome: either its own screen file with wireframe+findings, or an explicit note of why it's still blocked (e.g. the Supercopa recap OK anomaly) with the exact input methods already ruled out."
  - "Any match/season-advance flow encountered is driven to a result-only simulation, never a played-out match, per user instruction."
  - "07-despacho.md's 'Confirmado navegable' / 'Pendiente' sections stay in sync with what's actually been reached."

### S2 — Liga Promanager: resolve offer selection and reach its own despacho
- **Status**: done. Root cause of the `PASSWORD`-field blocker: the manager-name field holds keyboard focus by default and needs a name typed first. Offer-row selection confirmed (click team name); full post-offer flow followed to Promanager's own despacho, confirmed pixel-identical in layout to Liga Manager's (see `22-promanager-despacho.md`).
- **Files**: `docs/wiki/in-game/pcf6/10-liga-promanager-ofertas.md`, `docs/wiki/in-game/pcf6/22-promanager-despacho.md`
- **Gate**: none
- acceptance:
  - "The offer-row selection mechanic on 'SELECCIÓN DE OFERTAS' is confirmed working (or confirmed genuinely blocked with every input method tried listed), and SEGUIR advances past it."
  - "Liga Promanager's own despacho (if different from Liga Manager's) is reached and compared against 07-despacho.md, noting every difference."
  - "Keyboard shortcuts (bare underlined letter, no Alt — confirmed working for SEGUIR via plain 'S') are used as the primary input method for buttons before falling back to mouse-coordinate calibration."

### S3 — Main-menu misc modes: finish Historia, Los Goles de la Liga, Desafío América-Europa, Partido Amistoso
- **Status**: done. Los Goles de la Liga and Partido Amistoso documented from scratch (`23-goles-de-la-liga.md`, `24-partido-amistoso.md`); Desafío América-Europa pushed past the draft with TERMINAR (auto-completes squad) vs SEGUIR (only advances once "complete") conclusively distinguished, full match played to a champion pop-up in modo RESULTADO. Historia left as previously documented (own stuck-dialog instance already tracked in `12-historia.md`).
- **Files**: `docs/wiki/in-game/pcf6/11-desafio-america-europa.md`, `docs/wiki/in-game/pcf6/12-historia.md`, `docs/wiki/in-game/pcf6/23-goles-de-la-liga.md`, `docs/wiki/in-game/pcf6/24-partido-amistoso.md`, `docs/wiki/in-game/pcf6/00-menu-principal.md` (note: `13` was already taken by `13-resultados.md`, so Los Goles de la Liga and Partido Amistoso were filed as `23`/`24` instead, following the next free numbers after `22-promanager-despacho.md`)
- **Gate**: none
- acceptance:
  - "Los Goles de la Liga is documented (team/player filters, what LOS GOLES panel shows once populated)."
  - "Partido Amistoso's JUGADOR 1/2 team-assignment mechanic is confirmed working (or confirmed blocked with methods ruled out)."
  - "Desafío América-Europa is followed at least one screen past the draft (TERMINAR vs SEGUIR outcome)."
  - "Lower priority than S1/S2 per explicit user instruction — only worked on once Liga Manager/Promanager's own checklist has no open item left to try with a new approach."

### S4 — Deep dive: demarcación/position changes, tactics editing, player profile, and the live match screen
- **Status**: in-progress. Reopened per explicit user instruction (2026-08-21, session 4): previous slices covered navigation breadth but not depth on player-level and match-level interaction.
- **Files**: `docs/wiki/in-game/pcf6/15-alineacion.md`, `docs/wiki/in-game/pcf6/16-tacticas.md`, new `docs/wiki/in-game/pcf6/25-ficha-jugador.md` (player profile card, if it's a distinct screen), `docs/wiki/in-game/pcf6/07-despacho.md` (live match screen detail)
- **Gate**: none
- acceptance:
  - "Changing a player's demarcación/position (drag on the minicampo in TÁCTICAS and/or ALINEACIÓN, or any other mechanism found) is confirmed working, or confirmed genuinely blocked with every method tried listed."
  - "Clicking a player row to open their individual profile/ficha (attributes, photo, bio if any) is confirmed and documented — distinguish this from the CALIFICACIÓN/PARÁMETROS column toggle already documented, if it's a separate screen."
  - "A full match is played through in modo RESULTADO to reach the actual in-match/live result screen (not just FINAL DE PARTIDO) — document what's shown minute-by-minute if anything is visible (goals, cards, substitutions), not just the final scoreline."
  - "TÁCTICAS's formation editing is tried beyond selecting a preset: attempt dragging a player token on the field to fine-tune a role/position within a formation."

## acceptance

- Every despacho button (RESULTADOS, CALENDARIO, ALINEACIÓN, TÁCTICAS, VER RIVAL, CAJA, DECISIONES, ESTADIO, SALIR, GRABAR LIGA, NOTICIAS, SEGUIR, PLANTILLA, FICHAR) has a documented outcome: either its own screen file with wireframe+findings, or an explicit note of why it's still blocked (e.g. the Supercopa recap OK anomaly) with the exact input methods already ruled out.
- Any match/season-advance flow encountered is driven to a result-only simulation, never a played-out match, per user instruction.
- 07-despacho.md's 'Confirmado navegable' / 'Pendiente' sections stay in sync with what's actually been reached.
- The offer-row selection mechanic on 'SELECCIÓN DE OFERTAS' is confirmed working (or confirmed genuinely blocked with every input method tried listed), and SEGUIR advances past it.
- Liga Promanager's own despacho (if different from Liga Manager's) is reached and compared against 07-despacho.md, noting every difference.
- Keyboard shortcuts (bare underlined letter, no Alt — confirmed working for SEGUIR via plain 'S') are used as the primary input method for buttons before falling back to mouse-coordinate calibration.
- Los Goles de la Liga is documented (team/player filters, what LOS GOLES panel shows once populated).
- Partido Amistoso's JUGADOR 1/2 team-assignment mechanic is confirmed working (or confirmed blocked with methods ruled out).
- Desafío América-Europa is followed at least one screen past the draft (TERMINAR vs SEGUIR outcome).
- Lower priority than S1/S2 per explicit user instruction — only worked on once Liga Manager/Promanager's own checklist has no open item left to try with a new approach.
