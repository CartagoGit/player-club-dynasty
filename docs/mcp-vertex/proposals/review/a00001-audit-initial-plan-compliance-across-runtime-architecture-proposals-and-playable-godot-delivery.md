---
id: a00001
title: "Audit initial-plan compliance across runtime architecture proposals and playable Godot delivery"
kind: audit
status: review
type: proposal
track: architecture
date: 2026-08-18
shipped-in: f446dd7
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
- **Status**: pending
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
## acceptance

- The audit traces INITIAL-PLAN, ADRs and completed proposals to concrete runtime code, tests, benchmarks and visible Godot behavior.
- Every finding distinguishes implemented-and-integrated, implemented-but-disconnected, partial, missing and proposal-registry defects with file evidence.
- The report covers SOLID and naming, Godot/Core boundaries, external content and saves, deterministic orchestration, actual gameplay loop, classic-manager UX, scale benchmarks, CI and Git hygiene.
- The report defines a dependency-ordered repair portfolio with measurable acceptance and explicitly preserves validated work.
