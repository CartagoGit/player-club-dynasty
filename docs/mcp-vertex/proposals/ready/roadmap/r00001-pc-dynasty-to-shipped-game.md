---
id: r00001
title: "PC Dynasty — Roadmap to shipped, playable, fully functional game"
kind: feat
status: ready
type: proposal
track: roadmap
date: 2026-08-11
---

# r00001 — PC Dynasty: roadmap to a playable, fully functional game

## Goal

Get **PC Dynasty** to the point where a player can:

1. Open the executable.
2. Pick a club from a real, navigable world (Aurora Quay, Cinder Valley, Forge Lantern, …) with **real player squads** (Alven Roake, Brina Sorel, …), **real attributes**, **real competitions** (Boreal Series, Ember League, Horizon Cup).
3. See the squad, the kit placeholder (or real kit), the crest placeholder (or real crest), the manager.
4. Advance the season, play matches (deterministic simulation), see standings, see scorers, see finances.
5. Make a transfer, train a player, scout a youth, set a lineup.
6. Save, quit, reload — same world, same facts.

The reference target is **PC Fútbol 6 in scope, with more depth**: deeper attributes (already in domain: technique/form/physical/mental), more deterministic simulation, persistent history, and moddable content via the declarative pack system (f00014, f00025). **All assets are procedurally generated or community-sourced** — no copyrighted material is ever shipped.

This roadmap covers **the bridge** between the 25 done domain slices (f00001–f00026) and a buildable, playable `.exe`. The domain is already production-grade. What is missing is the **presentation wire-up** and **game-loop closure**.

## Why now

The user opened the project expecting a playable game and saw an inert text-only shell. Inspection revealed:

- 25 features `done` covering engine, simulation, careers, market, training, packs, presentation **shell**.
- `PresentationBootstrap.cs` only writes a literal string. No domain services are instantiated at startup.
- `ClubOverviewController.cs` has a `Bind(ClubOverviewView)` method but **no caller** wires real data into it.
- `data/clubs.json` is empty (`clubs: []`); the live data lives in `game/content/starter-world/data/*.json` but is never loaded by the bootstrap.
- `Content.Importers` (JsonContentReader, CsvContentReader, SqliteContentReader) exist but are not invoked at startup.

The "first vertical slice" (ADR 0001) was completed at the **domain** level. The presentation is still in placeholder mode. The roadmap closes that gap.

## non-goals

- Replacing the engine-neutral domain with anything Godot-aware.
- Copying PC Fútbol 6 assets, names, brands, or text (ADR 0006 invariant 4).
- Shipping copyrighted kits/crests/portraits (community sources or generated, always attributed).
- Mobile or web targets (ADR 0001: desktop only).
- Multiplayer or online leagues.
- 3D graphics. The visual style stays 2D (Godot 2D + the manager design system from f00017).

## Scope (what "shipped" means)

A shipped build is a `.exe` (Windows) and a Linux equivalent that:

- Loads the starter-world pack on first launch.
- Lets the user pick a club and a manager.
- Runs a complete season deterministically, advancing day by day or week by week.
- Plays matches, with results, scorers, cards, injuries, attendance, finances.
- Has a market with offers, accept/reject, wages.
- Has a training plan that improves attributes week by week.
- Has a scouting system that surfaces youth candidates.
- Persists saves, supports multiple careers.
- Ships with one full world (fictional, CC0) plus the empty slot for community packs.

This is the **MVP**. Anything beyond (Champions League continental cups, hall of fame, rivalries narrative, animated commentary, 3D match view) is **post-MVP** and gets its own slice later.

## Inventory: what's actually done

Confirmed during this proposal's authoring:

| Area | Slices done | State |
|---|---|---|
| Engine foundation | f00001, f00002 | Godot.NET + sport-pack abstractions |
| Domain atoms | f00003, f00004, f00006 | Person, attributes, knowledge, read models, competition graphs, versioned rules |
| Simulation | f00005, f00012, f00013 | Deterministic contest, deterministic protocol, macro/detailed fidelity |
| Persistence | f00009, f00015 | Save infrastructure + migration, contracts + fixtures |
| Content | f00010, f00014, f00025, f00026 | Import + validation, declarative packs + rules compiler, content editor, **starter-world CC0** |
| History | f00011, f00016 | World history + query projections, macro world advancement |
| Presentation shell | f00017 | Godot presentation + manager design system (text-only placeholder) |
| Game systems | f00018, f00019, f00020, f00021, f00022, f00023, f00024 | Lineup interaction, careers/development/injuries, economy/market/negotiations, facilities/academy, scouting, training, manager lifecycle |

**Domain inventory**: 25 .NET assemblies, 26 test projects, 224 test files, all green. **No domain regressions expected** from this roadmap.

**Presentation inventory**: 5 scenes (`Main`, `ClubOverview`, `PlayerProfile`, `LineupSelection`, `PackEditor`), 12 scripts (SidebarNavigator, MetricCard, DetailDrilldown, ClubOverviewController, …), `Theme/`, theme assets already authored. **The shell exists, the wire-up does not.**

**Data inventory**: `game/content/starter-world/data/` ships **clubs** (~10 fictional), **people** (~15 players + 5 managers), **competitions** (Boreal Series, Ember League, Horizon Cup), **countries** (Norvane, Soldara, Velmora), **rules** (continental cup, domestic league, knockout). All fictional, all CC0.

## Phases

The roadmap is structured in **6 phases**. Each phase is a self-contained batch of slices with its own acceptance gate. A phase is "done" when every slice in it is `done` and the gate is green.

### Phase 0 — Foundation (DONE)

The 25 features above. Confirmed green by `dotnet test game/PCDynasty.sln -c Release`. Nothing to add.

### Phase 1 — Presentation wire-up (BRIDGE)

Closes the gap between domain and UI. After Phase 1 the executable opens and shows **real clubs with real players** instead of placeholder text.

| ID | Title | Est. | Depends on |
|---|---|---|---|
| r00001.s1 | Bootstrap loads starter-world and exposes a `WorldService` singleton | 1h | none |
| r00001.s2 | `Main.tscn` reads real clubs list from the bootstrap and renders it | 1h | s1 |
| r00001.s3 | `ClubOverviewController.Bind` is called with a real `ClubOverviewView` projected from `PCDynasty.Application` | 2h | s1, s2 |
| r00001.s4 | `PlayerProfile.tscn` lists the squad for the selected club with attributes | 2h | s3 |
| r00001.s5 | Sidebar navigation actually switches between Resumen / Plantilla / Competiciones | 1h | s2, s3 |
| r00001.s6 | Architecture test: presentation cannot reference Godot in domain layer (already passes, keep green) | 0.5h | none |

**Gate**: open the executable on Windows or Linux, click a club, see a real squad with attributes. No demo strings anywhere.

### Phase 2 — Game loop closure (MVP)

The minimum to call it a "game".

| ID | Title | Est. | Depends on |
|---|---|---|---|
| r00001.s7 | Career bootstrap: pick a club + a manager, persist `CareerState` | 3h | s3 |
| r00001.s8 | Day/week advancement: `WorldService.AdvanceWeek()` deterministic | 2h | s7 |
| r00001.s9 | Match engine wired: schedule, simulate, record facts, update standings | 3h | s7, s8 |
| r00001.s10 | Season boundary: end of season → relegation/promotion, awards, finances reset | 2h | s9 |
| r00001.s11 | Save/load: serialize `CareerState` round-trip deterministically | 3h | s7, s8 |
| r00001.s12 | Multi-career: list, switch, delete from a manager shell menu | 2h | s11 |

**Gate**: start a career, advance 5 seasons, the same matches reproduce after reload, standings + finances make sense, save/load works.

### Phase 3 — Manager depth (PCF6 parity, no fluff)

| ID | Title | Est. | Depends on |
|---|---|---|---|
| r00001.s13 | Lineup selection UI bound to real squad + formation picker | 3h | s4 |
| r00001.s14 | Match tactics: formation, mentality, instructions — feed into contest engine | 3h | s13, s9 |
| r00001.s15 | Training plan: weekly schedule + attribute progression UI | 3h | s4 |
| r00001.s16 | Scouting report UI: candidates list, attributes, recommendation | 2h | s4 |
| r00001.s17 | Market UI: offers, clauses, negotiations, wage demands | 3h | s4 |
| r00001.s18 | Youth intake: end-of-season graduates from academies | 2h | s10 |

**Gate**: pick a club, set lineup, set tactics, train, scout, buy/sell, advance season. Everything clicks, everything persists.

### Phase 4 — Assets

Assets are **community-sourced or generated**, never copyrighted.

| ID | Title | Est. | Depends on |
|---|---|---|---|
| r00001.s19 | Crest generator: SVG generation from club colors + initials | 4h | none |
| r00001.s20 | Kit generator: 2D sprite from club colors + template | 6h | none |
| r00001.s21 | Portrait placeholder: initials-in-circle with attribute-driven color tint | 2h | none |
| r00001.s22 | Community packs: documented workflow + a curated example pack | 3h | s19, s20, s21 |
| r00001.s23 | Pack importer wired in bootstrap so multiple packs can be loaded | 2h | s1 |

**Gate**: every club shows a unique crest placeholder + kit placeholder + player portrait placeholder. No asset is a literal placeholder rectangle.

### Phase 5 — Polish, release

| ID | Title | Est. | Depends on |
|---|---|---|---|
| r00001.s24 | Windows `.exe` self-contained publish via `dotnet publish` | 1h | s11 |
| r00001.s25 | Linux equivalent + WSL smoke test | 1h | s24 |
| r00001.s26 | README: how to install, how to play, how to mod | 2h | all |
| r00001.s27 | Benchmarks re-baseline with real workloads | 2h | all |

**Gate**: someone downloads the `.exe`, opens it, plays a season, makes a save, quits, reloads — without reading any source.

### Phase 6 (post-MVP, future roadmap)

Out of scope here but tracked for the next roadmap revision:

- Continental cups + European-style multi-country competitions
- Manager career narrative (relationships, press, rivalries)
- Animated 2D match view (positions moving on a pitch)
- Audio (commentary, ambient)
- Mod workshop tools (in-game editor)

Each will get its own proposal when Phase 5 is shipped.

## Slices

- global_gate: dotnet (Phase 1)
- global_gate: dotnet (Phase 2)
- global_gate: dotnet (Phase 3)
- global_gate: dotnet (Phase 4)
- global_gate: dotnet (Phase 5)
- global_gate: e2e_manual (Phase 5)

### S1 — Bootstrap loads starter-world and exposes a `WorldService`

**Status**: ready

- **Files**:
  - `game/godot/Scripts/Bootstrap/GameBootstrap.cs` (new)
  - `game/godot/Scripts/Bootstrap/WorldService.cs` (new)
  - `game/tests/PCDynasty.Presentation.Tests/WorldServiceTests.cs` (new — fixture test)
- **Behaviour**:
  - `GameBootstrap._Ready()` locates `game/content/starter-world/`, calls `JsonContentReader`, hydrates an in-memory `WorldState`, exposes `WorldService.Instance` (or DI-equivalent).
  - Reads `clubs.json`, `people.json`, `competitions.json`, `countries.json`, `rules.json`. Validates per f00010.
  - Logs `[WorldService] Loaded N clubs, M people, K competitions`.
- **Acceptance**: integration test asserts `WorldService.Instance.Clubs.Count() >= 8` after bootstrap. `dotnet test` green.
- **Gate**: typecheck green, new tests green, existing 224 tests still green.

### S2 — Main scene renders real clubs list

**Status**: ready (depends on S1)

- **Files**:
  - `game/godot/Scripts/MainSceneController.cs` (new)
  - `game/godot/Scenes/Main.tscn` (modify)
- **Behaviour**:
  - Replaces the `PresentationBootstrap` literal text with a `VBoxContainer` listing the 8+ clubs: name, city, country short-name, founded year.
  - Each row clickable, emits `ClubSelected(clubId)`.
- **Acceptance**: open the `.exe`, see real club names (Aurora Quay Athletic, Cinder Valley Union, …) instead of placeholder text.
- **Gate**: visual confirmation + screenshot in the slice commit message.

### S3 — ClubOverview binds real ClubOverviewView

**Status**: ready (depends on S1, S2)

- **Files**:
  - `game/godot/Scripts/Clubs/ClubOverviewController.cs` (extend)
  - `game/godot/Scripts/Clubs/ClubOverviewProjector.cs` (new — domain → view)
- **Behaviour**:
  - When `ClubSelected` fires, swap to `ClubOverview.tscn`, project the selected club's `Application.ClubSummary` into a `ClubOverviewView` (displayName, founded, city, prestige, fanbase, top metrics).
  - The existing `Bind(ClubOverviewView)` API is reused — no changes to the controller.
- **Acceptance**: click Aurora Quay → see real stats. The "Sin alertas" / "Sin acciones pendientes" placeholders are still shown because no career exists yet (acceptable for Phase 1).
- **Gate**: visual + screenshot.

### S4 — PlayerProfile lists real squad

**Status**: ready (depends on S3)

- **Files**:
  - `game/godot/Scripts/Players/PlayerProfileController.cs` (new)
  - `game/godot/Scripts/Players/PlayerSquadProjector.cs` (new)
  - `game/godot/Scenes/Players/PlayerProfile.tscn` (modify)
- **Behaviour**:
  - When `ClubSelected` fires (or from a "Ver plantilla" button), show the squad for that club: name, position, technique, form, physical, mental, overall rating.
  - Uses `Application.Players.PlayerSummary` projection.
- **Acceptance**: click Aurora Quay → Plantilla → see Alven Roake (technique 62, form 58, physical 66, mental 61) and others.
- **Gate**: visual + screenshot.

### S5 — Sidebar navigation actually switches screens

**Status**: ready (depends on S2, S3)

- **Files**:
  - `game/godot/Scripts/Components/SidebarNavigator.cs` (extend)
- **Behaviour**:
  - Currently the buttons render but don't switch screens. Wire them to:
    - **Resumen** → ClubOverview
    - **Plantilla** → PlayerProfile
    - **Competiciones** → a new stub screen listing the club's competitions (Phase 1 stub; full implementation in Phase 3)
- **Acceptance**: clicking each sidebar button actually changes the centre panel.
- **Gate**: visual + screenshot.

### S6 — Architecture test: presentation cannot reference Godot in domain

**Status**: ready (probably already passing — confirm and keep green)

- **Files**:
  - `game/tests/PCDynasty.Architecture.Tests/` (extend if needed)
- **Behaviour**: ensure no `using Godot;` outside `PCDynasty.Presentation` and the bench harness.
- **Acceptance**: existing architecture test still passes; add an explicit one if not present.
- **Gate**: `dotnet test game/tests/PCDynasty.Architecture.Tests` green.

(Phases 2–5 slice breakdowns intentionally omitted from this file; they will be added as their phase starts, per the project's "small commits, conventional slices" rule from ADR 0006 §Método de entrega. The Phase table above is the planning contract; the slice files are opened when the phase starts.)

## Acceptance (overall)

Phase 1 is the **first deliverable** and the one we ship first. Phase 1 acceptance:

- `dotnet test game/PCDynasty.sln -c Release` green.
- A new project `PCDynasty.Presentation.Tests` exists and is wired into the sln.
- Opening the executable shows **8+ real clubs** with **real player squads** instead of placeholder text.
- Screenshots in the slice commit messages document the before/after.
- The architecture gate (`PCDynasty.Architecture.Tests`) still passes — domain stays engine-neutral.

When Phase 5 ships, the overall acceptance is: a user downloads the `.exe`, plays a season with a real club, makes a save, reloads, sees the same world — without touching source.

## Risks

| Risk | Mitigation |
|---|---|
| Domain services were never designed to be called from Godot (different assembly boundaries) | S1 builds a thin `GameBootstrap` adapter — domain stays untouched |
| Bootstrap startup time budget (ADR 0009: cold start ≤ 2s, hot load ≤ 500ms) | S1 records the metric; S7+ reuses the same `WorldService` instance, never reloads |
| Starter-world data is small (10 clubs, 15 players) — not enough to "feel" populated | Phase 4 ships asset generation; community packs extend it later |
| Asset sourcing risks (copyright on kits/crests) | S19–S22 ship **generated** placeholders only; community packs are CC0-by-default and validated by the pack editor |
| Roadmap drift: too many slices, no one drives them | Each slice has explicit deps + a gate; the orchestrator proposes the next slice after each close |

## Why "shipped" is realistic in this scope

The 25 done domain slices already implement: deterministic simulation, careers, market, training, scouting, finances, facilities, save/load, declarative packs, content editor, history, persistence, contracts. **No domain work is left to invent.** Phase 1–5 is purely the bridge to the player.

With this proposal as the contract, the work breaks down into:

- **Phase 1 (presentation wire-up)**: 6 slices, ~10h. **The first end-to-end visible result.**
- **Phase 2 (game loop)**: 6 slices, ~15h. Playable career.
- **Phase 3 (manager depth)**: 6 slices, ~16h. PCF6 parity.
- **Phase 4 (assets)**: 5 slices, ~17h. Visual polish without copyright risk.
- **Phase 5 (release)**: 4 slices, ~6h. Windows + Linux + docs.

**Total remaining work after this proposal**: ~64 hours of focused slice execution, plus community asset sourcing in parallel. At one slice per session (the typical agent cadence), the roadmap completes in **5–7 sessions of focused work**, not years. The previous "PCF6 takes years" estimate assumed building everything from scratch; here the domain is already there.

## How this proposal is used

- **Open this file** at the start of any work session that wants to make the game playable.
- **Pick the next ready slice** in dependency order. S1 is the natural first choice.
- **Each slice** is closed with `proposals_close_slice` (the agent's standard flow).
- **After Phase 1 ships**, this proposal transitions to `done/roadmap/` and the next phase opens its own proposal that builds on Phase 1 acceptance.
