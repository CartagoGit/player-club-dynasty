---
id: d00001
title: "Play through Liga Manager and Liga Promanager screen by screen, documenting every button"
kind: docs
status: review
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
  - **Session 5 additions (2026-08-21)**: full sweep of every RESULTADOS competition tab (Supercopa de España ida/vuelta, Copa del Rey ronda-based IDA/VUELTA/GLOBAL, Superc. Europa and Copa Intercontinental's single-match "trofeo" templates with flags/neutral stadiums, Recopa/U.E.F.A./Promoción 2ªB "no comenzada" dialogs, 2ª División/2ªB I real-jornada data) written into `13-resultados.md`. CAJA's entry button and its financial breakdown (INGR.+GAST. combined view, real weekly figures) confirmed fully working — the earlier "CAJA not responding" scare was the same focus-loss/`--reset`-omission artifact documented below, not a game bug; the 4 internal tabs (INGRESOS/GASTOS/POR SEMANA/TEMPORADA) remain confirmed non-functional (3rd session in a row with identical symptom). GOLEADORES fully explored (per-goal minute log modal, COMPARA slots) into `09-clasificacion.md`, and its historical `VOLVER`-stuck anomaly did not reproduce this session. CARTERA DE FICHAJES' add-a-scouting-target flow completed end-to-end (continent/country/club picker → double-click a real player from any club's real roster → added with full attributes and a "secretario técnico" flavor-text reaction) into `21-plantilla-fichar.md`. A significant methodology finding was added to `como-esta-montado.md`: several "stuck button" scares this session traced back to omitting `--reset` on the recovery click after a spontaneous window-focus loss, not real game bugs — future agents must use `--reset` on any click whose target coordinate was freshly (re)computed, especially right after an Alt+Tab/taskbar recovery.
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

### S5 — Scouting and staff: OJEADOR/OBSERVADOR hiring, CONTRATAR/DESPEDIR flow, and a completed real transfer
- **Status**: ready. Session 5 audit (2026-08-21) found this materially incomplete despite S1 being marked done: `08-personal-del-club.md` has never had a row clicked, `CONTRATAR`/`DESPEDIR` have never been pressed, and there is no confirmation of what a "buscar candidatos" screen (if any) looks like for hiring backroom staff (the six specialist assistant coaches, fisioterapeuta, psicólogo, secretario técnico, entrenador de juveniles, encargado de campo, and — critically — the two distinct scouting roles OBSERVADOR and OJEADOR). Separately, `21-plantilla-fichar.md` has completed the *watchlist* (CARTERA) flow but never a real signing: FICHAR's own `OFERTAS` button (the 4th, distinct from `ESTADO OFERTAS`) has never been clicked, and no player has been selected → offered → confirmed end-to-end. JUVENILES's own `BUSCAR` (with its OJEADOR search-capability panel) and SECRETARIO's `BUSCAR` (once a numeric criterion, not just a checkbox, is set) also remain unpressed.
- **Files**: `docs/wiki/in-game/pcf6/08-personal-del-club.md`, `docs/wiki/in-game/pcf6/21-plantilla-fichar.md`
- **Gate**: none
- acceptance:
  - "PERSONAL DEL CLUB: clicking a staff row is confirmed to select it (or confirmed not to respond, with methods ruled out), and CONTRATAR/DESPEDIR are each pressed at least once with the resulting screen (candidate list, confirmation dialog, or no-op) documented."
  - "FICHAR's `OFERTAS` button is clicked and its screen documented."
  - "At least one real signing is completed end-to-end (select a player in JUGADORES A LA VENTA → make an offer via OFERTAS/ESTADO OFERTAS → confirm), or the flow is confirmed genuinely blocked with every input method tried listed."
  - "SECRETARIO's BUSCAR is retried with a numeric selector set (not just a checkbox) to determine whether it then returns results."
  - "JUVENILES's own BUSCAR is pressed at least once."

### S6 — Estadio: remaining REMODELAR/SERVICIOS sub-panels and the VALLAS offer
- **Status**: ready. `20-estadio.md`'s own Pendiente flags two open items the user specifically called out as under-covered: the `SERVICIOS` sub-navigation never changes its detail panel away from `ENFERMERÍA` (TIENDA DEL CLUB/CAFETERÍAS/W.C. untested — unclear if this is a calibration gap or a real limit), and no remodelación checkbox nor the `DÍA DE PARTIDO` VALLAS-rights `ACEPTAR` has ever actually been pressed (deliberately avoided to not disturb game-state economy). Both need a deliberate, economy-aware pass: press one low-stakes checkbox/ACEPTAR through to its confirmation dialog (without necessarily letting the transaction complete if a cancel option exists), specifically to document what happens next, not just what's on the form.
- **Files**: `docs/wiki/in-game/pcf6/20-estadio.md`
- **Gate**: none
- acceptance:
  - "SERVICIOS's TIENDA DEL CLUB, CAFETERÍAS, and W.C. sub-lines are each clicked with a keyboard-navigation fallback (Tab/arrows) attempted if mouse clicks keep landing on ENFERMERÍA, and the outcome (panel changes, or confirmed non-functional like CAJA's tabs) is documented."
  - "At least one REMODELAR checkbox is marked and its confirmation/ACEPTAR flow followed at least one screen further than the form (even if ultimately cancelled), and the DÍA DE PARTIDO vallas-rights ACEPTAR is pressed to see its confirmation dialog."

### S7 — Close out the remaining per-file Pendiente backlog
- **Status**: ready. After S1-S4 were marked done, a large number of narrower Pendiente items across already-visited files remain genuinely open — this slice exists so they don't get silently dropped. Non-exhaustive list as of 2026-08-21: `00-menu-principal.md` (Proquinielas 2.0 crash-to-desktop, not re-tested for a fix); `07-despacho.md` (VISIONAR in CALENDARIO deliberately untested); `10-liga-promanager-ofertas.md` (only offer row 1 tried; second manager slot; CARGAR LIGA/BORRAR); `11-desafio-america-europa.md` (mini-hub ALINEACIÓN/TÁCTICAS/VER RIVAL; América-as-JUGADOR-1); `12-historia.md` (other competitions/decades; IMPRIMIR; help-viewer close-method); `13-resultados.md` (2ªB II/III/IV individually; final-CAMPEÓN announcement for Superc.Eur./Intercont.; Supercopa de España tiebreak criterion); `14-calendario.md` (VISIONAR; month navigation); `15-alineacion.md` (SEGUROS in LESIONADOS; AUTO in ENTRENAMIENTO); `16-tacticas.md` (GRABAR TÁCTICA save action; dorsal reassignment end-to-end); `17-ver-rival.md` (ORDENADOR button; TÁCTICAS-within-Ver-Rival; all-99 attributes partida-specific check); `18-caja.md` (the 4 tabs are settled as non-functional, not reopened by this slice); `19-decisiones.md` (OFERTAS TV/MERCHANDISING; PRIMAS OK buttons); `22-promanager-despacho.md` (sub-screens vs. Liga Manager's, SEGUIR cycle); `23-goles-de-la-liga.md` (A-Z index jump; LOS GOLES panel scroll with a high-goal player); `24-partido-amistoso.md` (mini-hub; human JUGADOR 2). Also fix the stray screenshot `screenshots/fichar/02-selector-continente-club.png` which was saved showing the wrong screen (post-add CARTERA view, not the country/club picker it's named for) — either re-capture it or confirm `21-plantilla-fichar.md` doesn't actually cite it (the session-2 file `screenshots/plantilla-fichar/05-selecciona-jugador-cartera.png` may already be the correct canonical shot, making the mislabeled one redundant/deletable).
- **Files**: all `docs/wiki/in-game/pcf6/*.md` files with an open Pendiente section as of 2026-08-21.
- **Gate**: none
- acceptance:
  - "Every Pendiente bullet listed above is either resolved (with the finding written into its file) or re-confirmed as genuinely blocked with the exact methods already tried, before this slice is marked done — no bullet should just be silently dropped."
  - "The mislabeled `02-selector-continente-club.png` screenshot is either replaced with the correct content or removed if redundant with the session-2 shot."

### S4 — Deep dive: demarcación/position changes, tactics editing, player profile, and the live match screen
- **Status**: done. Demarcación/ROL change confirmed via a per-player dropdown of ~18 tactical roles in TÁCTICAS (changes ME, distinct from the fixed DEM column); minicampo drag confirmed as read-only (new `vm:drag` tool built and tested, no effect); player profile-on-row-click confirmed in both ALINEACIÓN and TÁCTICAS (shared component, attributes only, no photo — distinct from the JUGADOR DEL PARTIDO/goleador cards which do have photos); a full match played through with ESTADÍSTICAS/TÁCTICAS/ALINEACIÓN all confirmed reachable mid-match from FINAL PRIMERA PARTE, with per-goal/per-card minute detail and stadium matchday revenue documented.
- **Files**: `docs/wiki/in-game/pcf6/15-alineacion.md`, `docs/wiki/in-game/pcf6/16-tacticas.md`, `docs/wiki/in-game/pcf6/07-despacho.md`, `docs/wiki/in-game/como-esta-montado.md` (new `vm:drag` tool)
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
- PERSONAL DEL CLUB's row-selection and CONTRATAR/DESPEDIR are exercised at least once, and at least one real transfer is completed end-to-end in FICHAR (not just the CARTERA watchlist flow).
- ESTADIO's SERVICIOS sub-navigation (TIENDA DEL CLUB/CAFETERÍAS/W.C.) and at least one REMODELAR checkbox / the vallas ACEPTAR are pressed through to their confirmation screens.
- No file is left with a stale Pendiente bullet that could have been resolved with a method not yet tried (S7).
