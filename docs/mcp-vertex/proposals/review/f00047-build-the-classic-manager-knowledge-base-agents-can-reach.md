---
id: f00047
title: "Build the classic-manager knowledge base every agent can reach"
kind: feat
status: review
type: proposal
track: knowledge
date: 2026-08-19
plan: q00001
audit-source: a00001
---

# f00047 — Build the classic-manager knowledge base every agent can reach

## Goal

Create `docs/wiki/`: a researched, structured reference on how the classic desktop
football managers of the nineties organised their screens, their interaction and
their game systems, plus an explicit decision record of what PC Dynasty adopts,
corrects and adds. Wire it into the `docs` plugin so **any** agent reaches it through
`mcp-vertex_docs_search` without being told, and make the visual grammar chapter
normative for every UI change.

## why

Three UI proposals have shipped (f00042, f00043, f00044) and the result still does
not read as a classic manager. The reason is structural, not one of effort: the only
written reference was a one-page style guide describing tokens and spacing, which is
enough to keep two panels consistent with each other and nowhere near enough to
reproduce an interface grammar. Every agent that touched a screen re-derived the look
from its own idea of "classic", and the results diverged.

A second cost is worse: **the same defects keep coming back**. A `Button` with
container children that swallows the click, a panel pinned with absolute offsets, a
shared component calling `tr()` — each has been introduced, found and fixed at least
once. Nothing recorded them, so nothing prevents the next one.

The knowledge base fixes both: one normative chapter every UI slice is held to, and a
list of the concrete implementation traps this codebase has already fallen into.

## non-goals

- Copying any asset, artwork, font, sound, screenshot, trademark, club, competition
  or real person's name from the referenced products. The wiki records **facts and
  rules**, in our own words, and says so on every page.
- Documenting our own code. That is `docs/architecture/`.
- Deciding scope. Chapter 12 lists candidates with a state; promoting one to work
  still needs its own proposal.

## Slices

- global_gate: unit

### S1 — Research the reference and record the sources
- **Status**: done
- **Files**: `docs/wiki/pcfutbol/99-fuentes.md`
- **Gate**: unit
- acceptance:
  - "Every factual claim in the wiki carries a provenance tag: `[manual 4.0]` for the
    1995 printed manual, `[wiki]` for encyclopaedias, `[prensa]` for press,
    `[comunidad]` for player communities, `[capturas]` for screenshot analysis, and
    the literal word **inferencia** for our own deductions."
  - "The primary document — the official 19-page manual — is identified with its URL
    and its reliability stated. No long verbatim text and no image is copied into the
    repository."
  - "The sources page explains how to extend the wiki and what to do when two sources
    disagree."

### S2 — Write the reference chapters
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `docs/wiki/pcfutbol/01-historia-y-versiones.md`, `docs/wiki/pcfutbol/02-modos-de-juego.md`, `docs/wiki/pcfutbol/03-mapa-de-pantallas.md`, `docs/wiki/pcfutbol/05-interaccion-y-controles.md`, `docs/wiki/pcfutbol/06-sistemas-deportivos.md`, `docs/wiki/pcfutbol/07-economia-y-club.md`, `docs/wiki/pcfutbol/08-mercado-y-fichajes.md`, `docs/wiki/pcfutbol/09-competiciones-y-calendario.md`, `docs/wiki/pcfutbol/10-datos-seguimiento-y-base-de-datos.md`, `docs/wiki/pcfutbol/11-recepcion-y-lecciones.md`
- **Gate**: unit
- acceptance:
  - "The history chapter covers every release from 1992 to the abandoned 2025 revival,
    with what each added, and separates the four reasons it succeeded from the four
    that killed it."
  - "The screen map documents eighteen screens as zones and rules, not pixels, so each
    can be rebuilt with original assets."
  - "The systems chapters record the actual numbers where they were published — the
    eighteen player roles, the three weeks of red numbers before bankruptcy, the 12%
    credit interest, the European prize structure, the card suspension rules — marked
    as reference, never as our own specification."
  - "The reception chapter turns the criticism into six numbered lessons, each with
    its application to this repository."

### S3 — Make the visual grammar normative
- **Status**: done
- **DependsOn**: [S2]
- **Files**: `docs/wiki/pcfutbol/04-gramatica-visual.md`
- **Gate**: unit
- acceptance:
  - "The chapter states eight principles, the fixed screen structure, and fourteen
    named components with the properties each must satisfy."
  - "It ends with a checklist that is the acceptance bar for any UI slice."
  - "It states that a screen contradicting it is a defect, not a variant."
  - "It describes form, never material: colours are expressed as roles, never as
    literals."

### S4 — Record what we adopt, correct, add and refuse
- **Status**: done
- **DependsOn**: [S2]
- **Files**: `docs/wiki/pcfutbol/12-mejoras-para-pc-dynasty.md`, `docs/wiki/pc-dynasty/mapa-de-equivalencias.md`
- **Gate**: unit
- acceptance:
  - "Four tables: keep as-is, correct, add, and explicitly refused — each entry with
    its rationale and its state."
  - "The equivalence map lists every classic screen against ours, with the file that
    implements it and the gap that remains."
  - "The open interface defects are listed with the chapter that explains each."

### S5 — Wire it into the server and the host files
- **Status**: done
- **DependsOn**: [S3]
- **Files**: `mcp-vertex.config.json`, `README.md`, `docs/mcp-vertex/REPO-RULES.md`, `AGENTS.md`, `CLAUDE.md`, `docs/wiki/README.md`
- **Gate**: unit
- acceptance:
  - "`docs/wiki` is a root of the `docs` plugin, so `mcp-vertex_docs_list`,
    `_docs_read` and `_docs_search` reach it with no extra configuration."
  - "`REPO-RULES.md` carries a 'read before touching the UI' table and states the hard
    rules the wiki produced."
  - "`AGENTS.md` and `CLAUDE.md` point at the normative chapter in one line each,
    without turning into content files."
  - "`docs/wiki/README.md` routes an agent to the right chapter by task."

### S6 — Name the target version and catalogue the visual references
- **Status**: done
- **DependsOn**: [S3, S4]
- **Files**: `docs/wiki/pcfutbol/00-objetivo-y-alcance.md`, `docs/wiki/pcfutbol/13-version-objetivo-6-0-y-7-0.md`, `docs/wiki/pcfutbol/14-catalogo-visual.md`, `docs/wiki/pcfutbol/99-fuentes.md`, `docs/wiki/README.md`, `docs/wiki/pcfutbol/01-historia-y-versiones.md`, `docs/wiki/pcfutbol/04-gramatica-visual.md`, `docs/wiki/pcfutbol/12-mejoras-para-pc-dynasty.md`, `docs/wiki/pc-dynasty/mapa-de-equivalencias.md`, `docs/mcp-vertex/REPO-RULES.md`, `README.md`, `AGENTS.md`, `CLAUDE.md`
- **Gate**: unit
- acceptance:
  - "A normative chapter states the target — PC Fútbol 6.0 with 7.0's
    improvements — gives each other release an explicit role (4.0 and 5.0 as the
    origin of ideas, 2000 and 2001 as evolution, 2005 onwards as warnings), and
    carries the rules for resolving a conflict between them."
  - "A chapter describes the target version screen by screen with original
    wireframes: main menu, the persistent top band, the Liga Manager route from
    name entry to pre-season to the lineup screen that acts as the hub, the
    offers board, both dossiers, the historical comparison and the walk-out."
  - "The attribute column set the original repeated everywhere — EN VE RE AG CA
    EF MO ME ROL DEM — is recorded once and referenced from every screen that
    shows it."
  - "Six components found in the reference are added to the grammar: tab strip,
    row tab, view switch, staff plate, slot list and segment indicator."
  - "A visual catalogue links every reference image at its source, says what each
    one shows and where its wireframe lives, and states the policy: references
    are linked and described, never copied into the repository."
  - "The two lineages of interface that circulate as 'PC Fútbol' — the classic
    Liga Manager and the later quadrant desk — are told apart, so nobody rebuilds
    the wrong one by accident."
  - "Every host pointer names the objective chapter, not only the grammar."

## acceptance

- An agent that has never seen this repository can, from `mcp-vertex_docs_search`
  alone, find the normative grammar and the trap list before writing UI code.
- Every factual claim carries provenance; every deduction is marked as such.
- No third-party asset, artwork, font, sound, screenshot or trademark enters the
  repository, and the wiki says so on its front page and in its sources chapter.
- The equivalence map gives a truthful account of what is missing, including for
  screens this project has already shipped.
- The wiki states, in the first screen an agent sees, which version it is trying
  to look like and what everything else is there for.
