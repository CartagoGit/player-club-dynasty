---
id: a00001
title: "Audit initial-plan compliance across runtime architecture proposals and playable Godot delivery"
kind: audit
status: done
type: proposal
track: architecture
date: 2026-08-18
shipped-in: [f446dd7, fd2d631]
---

# a00001 — Audit initial-plan compliance across runtime architecture proposals and playable Godot delivery

## Goal

Produce an evidence-backed compliance baseline that distinguishes isolated domain prototypes from functionality actually orchestrated, persisted and visible in the Godot game, then define the repair portfolio required before new feature expansion.

## why

The executable is visible but the current proposal portfolio and green unit tests overstate integration: several completed engines are unreachable from runtime, the active roadmap is invisible because of an identifier collision, and critical initial-plan invariants are not end-to-end enforced.

## non-goals

- Implement the repairs inside the audit slice.
- Copy any protected manager interface or asset.
- Retire valid domain prototypes merely because they are not yet integrated.

## Slices

- global_gate: e2e

### S1 — Publish the initial-plan to runtime compliance matrix
- **Status**: done
- **Files**: `docs/architecture/0010-initial-plan-runtime-compliance-audit.md`
- **Gate**: e2e
- acceptance:
  - "The audit traces INITIAL-PLAN, ADRs and completed proposals to concrete runtime code, tests, benchmarks and visible Godot behavior."
  - "Every finding distinguishes implemented-and-integrated, implemented-but-disconnected, partial, missing and proposal-registry defects with file evidence."
  - "The report covers SOLID and naming, Godot/Core boundaries, external content and saves, deterministic orchestration, actual gameplay loop, classic-manager UX, scale benchmarks, CI and Git hygiene."
  - "The report defines a dependency-ordered repair portfolio with measurable acceptance and explicitly preserves validated work."
- review-state: changes_requested
- review-implementer: root
- review-reviewer: review_world_fix_fresh
- review-log: requested_changes by review_world_fix_fresh — Falta matriz propuesta done a aceptación y consumidor runtime; la secuencia tiene dependencias invertidas entre contenido, orquestador y sistemas; faltan editor/modding, LifeEvent, staff/mercado, fanbase/estadio, sanciones y memoria de 80 años; varios criterios carecen de gates y umbrales; taxonomía inconsistente.
- review-log: changes_addressed in fd2d631 — La matriz done → aceptación → consumidor productivo, el reordenamiento de dependencias, las promesas que faltaban y los umbrales por etapa están publicados en `docs/architecture/0010-initial-plan-runtime-compliance-audit.md`.
- review-log: approved by claude-opus-5 session 2026-08-18 — Verificado documento a documento: la matriz cubre todos los IDs en `done/`, la cartera ordena contenido antes de orquestador y sistemas, cada etapa declara gate con umbral medible y la taxonomía integrado/parcial/isla/ausente/defectuoso se aplica de forma consistente. La cartera queda instanciada como `q00001` más catorce propuestas hijas.
## acceptance

- The audit traces INITIAL-PLAN, ADRs and completed proposals to concrete runtime code, tests, benchmarks and visible Godot behavior.
- Every finding distinguishes implemented-and-integrated, implemented-but-disconnected, partial, missing and proposal-registry defects with file evidence.
- The report covers SOLID and naming, Godot/Core boundaries, external content and saves, deterministic orchestration, actual gameplay loop, classic-manager UX, scale benchmarks, CI and Git hygiene.
- The report defines a dependency-ordered repair portfolio with measurable acceptance and explicitly preserves validated work.

## verified state

Measured on 2026-08-18 against `main` at `78971b5`:

- `dotnet test game/PCDynasty.sln -c Release` is green across nine suites, including
  nineteen architecture tests and forty-nine presentation tests.
- Repository hygiene is green: no generated artifact is tracked.
- The executable boots, opens a career, navigates, saves a lineup, plays synthetic
  results, signs a player and continues.
- No self-contained export exists: there is no `export_presets.cfg`, and the launcher
  still requires an installed SDK and Godot.
- Reachability from `game/godot/PCDynasty.Presentation.csproj` was computed
  transitively; `PCDynasty.Career`, `PCDynasty.Careers`, `PCDynasty.Club`,
  `PCDynasty.Football` and `PCDynasty.World` have no productive consumer.

## findings

1. The runtime bypasses the pack pipeline: content skips validator and compiler and
   coerces invalid integers to zero.
2. There is no temporal orchestrator: no career date, no `AdvanceWorld`, seeds derived
   from the wall clock and a shared mutable instance.
3. The application layer duplicates football rules that `FootballContestPack` and
   `FootballScoringRules` already own.
4. Presentation owns mutable career history and can display results that were never
   persisted.
5. Godot handlers perform synchronous input and output inside the frame budget.
6. The productive path is unindexed: squad, club and market lookups are linear scans.
7. Two save contract families coexist and the validated world store, migrator and
   deterministic protocol have no consumer.
8. Registry defect: two documents claimed `id: r00001`, so the board returned zero
   actionable proposals and the roadmap stayed invisible and obsolete.

## scoreboard

| State | Product capabilities |
| --- | --- |
| integrated | 0 |
| partial | 10 |
| island | 8 |
| defective | 4 |
| absent | 3 |

The repair portfolio is published as the plan `q00001` with fourteen child proposals,
ordered by dependency; `c00005` owns the registry and gate repair that unblocks it.
