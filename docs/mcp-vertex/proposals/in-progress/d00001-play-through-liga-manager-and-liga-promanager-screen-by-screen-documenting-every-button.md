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
- **Status**: done (2026-08-21, two sessions). All twelve despacho grid buttons plus SALIR/GRABAR LIGA/NOTICIAS/SEGUIR have a documented outcome; the previously-blocked PLANTILLA/FICHAR turned out to be a calibration error, now resolved. A fourth stuck-dialog instance was found (GRABAR LIGA's own save dialog) and a counter-example where the Clasificación VOLVER anomaly did *not* reproduce (stuck-dialog count stays at 4 confirmed instances). Session 2 went a layer deeper into TÁCTICAS (PREDEF./CARGAR/GRABAR/TÁCTICA EQUIPO/DORSALES all pulled and documented), ESTADIO (all 4 buttons: DÍA DE PARTIDO/REMODELAR/OBRAS/GRÁFICA), and FICHAR/PLANTILLA (ESTADO OFERTAS, SECRETARIO's search form, JUVENILES' own youth-academy screen). A new (non-anomaly) UI quirk was found in CAJA: its 4 alternate tabs consistently trigger an ambient branding overlay instead of switching view, across two sessions — flagged for a future agent to try keyboard shortcuts instead of mouse. Remaining open items are narrow and tracked per-file: CAJA's 4 tabs (see [`18-caja.md`](../../../wiki/in-game/pcf6/18-caja.md)), GRABAR TÁCTICA's actual save action (deliberately not risked — see [`16-tacticas.md`](../../../wiki/in-game/pcf6/16-tacticas.md)), REMODELAR's 3 non-GRADAS categories, and a few "confirm criteria" details in FICHAR/SECRETARIO's search form.
- **Files**: `docs/wiki/in-game/pcf6/07-despacho.md`, `docs/wiki/in-game/pcf6/08-personal-del-club.md`, `docs/wiki/in-game/pcf6/09-clasificacion.md`, `docs/wiki/in-game/pcf6/13-resultados.md`, `docs/wiki/in-game/pcf6/14-calendario.md`, `docs/wiki/in-game/pcf6/15-alineacion.md`, `docs/wiki/in-game/pcf6/16-tacticas.md`, `docs/wiki/in-game/pcf6/17-ver-rival.md`, `docs/wiki/in-game/pcf6/18-caja.md`, `docs/wiki/in-game/pcf6/19-decisiones.md`, `docs/wiki/in-game/pcf6/20-estadio.md`, `docs/wiki/in-game/pcf6/21-plantilla-fichar.md`
- **Gate**: none
- acceptance:
  - "Every despacho button (RESULTADOS, CALENDARIO, ALINEACIÓN, TÁCTICAS, VER RIVAL, CAJA, DECISIONES, ESTADIO, SALIR, GRABAR LIGA, NOTICIAS, SEGUIR, PLANTILLA, FICHAR) has a documented outcome: either its own screen file with wireframe+findings, or an explicit note of why it's still blocked (e.g. the Supercopa recap OK anomaly) with the exact input methods already ruled out."
  - "Any match/season-advance flow encountered is driven to a result-only simulation, never a played-out match, per user instruction."
  - "07-despacho.md's 'Confirmado navegable' / 'Pendiente' sections stay in sync with what's actually been reached."

### S2 — Liga Promanager: resolve offer selection and reach its own despacho
- **Status**: pending
- **Files**: `docs/wiki/in-game/pcf6/10-liga-promanager-ofertas.md`, `docs/wiki/in-game/pcf6/22-promanager-despacho.md`
- **Gate**: none
- acceptance:
  - "The offer-row selection mechanic on 'SELECCIÓN DE OFERTAS' is confirmed working (or confirmed genuinely blocked with every input method tried listed), and SEGUIR advances past it."
  - "Liga Promanager's own despacho (if different from Liga Manager's) is reached and compared against 07-despacho.md, noting every difference."
  - "Keyboard shortcuts (bare underlined letter, no Alt — confirmed working for SEGUIR via plain 'S') are used as the primary input method for buttons before falling back to mouse-coordinate calibration."

### S3 — Main-menu misc modes: finish Historia, Los Goles de la Liga, Desafío América-Europa, Partido Amistoso
- **Status**: pending
- **Files**: `docs/wiki/in-game/pcf6/11-desafio-america-europa.md`, `docs/wiki/in-game/pcf6/12-historia.md`, `docs/wiki/in-game/pcf6/13-goles-de-la-liga.md`, `docs/wiki/in-game/pcf6/23-partido-amistoso.md`, `docs/wiki/in-game/pcf6/00-menu-principal.md`
- **Gate**: none
- acceptance:
  - "Los Goles de la Liga is documented (team/player filters, what LOS GOLES panel shows once populated)."
  - "Partido Amistoso's JUGADOR 1/2 team-assignment mechanic is confirmed working (or confirmed blocked with methods ruled out)."
  - "Desafío América-Europa is followed at least one screen past the draft (TERMINAR vs SEGUIR outcome)."
  - "Lower priority than S1/S2 per explicit user instruction — only worked on once Liga Manager/Promanager's own checklist has no open item left to try with a new approach."

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
