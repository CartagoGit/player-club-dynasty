---
id: c00005
title: "Sanitise the proposal registry, the roadmap and the product gates"
kind: chore
status: review
type: proposal
track: infrastructure
date: 2026-08-18
plan: q00001
audit-source: a00001
---

# c00005 — Sanitise the proposal registry, the roadmap and the product gates

## Goal

Make the proposal registry resolvable, the roadmap visible and the product gates
strong enough that a slice cannot claim a visible capability it never wired into the
executable.

## why

Audit `a00001` found a registry defect that silently stopped all planning: two
documents claimed `id: r00001`, the catalogue resolved the finished refactor and the
board returned zero actionable proposals, leaving the roadmap invisible and obsolete.
The audit also found that the gates cannot detect the failure mode that produced
eleven island modules — a slice can be `done`, fully tested, and still be unreachable
from `game/godot/Scenes/Main.tscn`, because CI never builds Godot, never runs
SmokeBoot and only publishes benchmark reports instead of comparing them against the
versioned budgets.

## non-goals

- Implementing any repair stage; this slice only prepares the registry and the gates.
- Rewriting historical proposals under `done/`.
- Replacing `bun run validate` as the delendai gate; the product gate is added
  alongside it, not instead of it.

## Slices

- global_gate: e2e

### S1 — Give the roadmap a unique id and replace its obsolete content
- **Status**: done
- **Files**: `docs/delendai/proposals/ready/roadmap/q00001-pc-dynasty-repair-portfolio-to-a-shipped-classic-manager.md`
- **Gate**: type
- acceptance:
  - "The roadmap is a `plan` proposal with the unique id `q00001` and no longer collides with the `r00001` refactor."
  - "The removed `WorldService.Instance` pattern, the eight-club assumption and the sixty-four-hour estimate are gone."
  - "Its slices are the audit's dependency-ordered stages, each naming the child proposal that owns it."

### S2 — Publish the repair portfolio as parseable proposals
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `docs/delendai/proposals/ready/x00041-correct-managed-club-identity-and-command-authority.md`, `docs/delendai/proposals/ready/f00030-establish-the-canonical-indexed-world-loader.md`, `docs/delendai/proposals/ready/f00031-ship-playable-league-content-and-round-trip-authoring.md`, `docs/delendai/proposals/ready/f00032-unify-career-external-persistence-and-transactions.md`, `docs/delendai/proposals/ready/f00033-build-the-calendar-and-deterministic-world-orchestrator.md`, `docs/delendai/proposals/ready/f00034-integrate-footballpack-lineups-and-competition-formats.md`, `docs/delendai/proposals/ready/f00035-connect-authoritative-economy-contracts-and-market.md`, `docs/delendai/proposals/ready/f00036-connect-the-player-dossier-knowledge-and-scouting.md`, `docs/delendai/proposals/ready/f00037-connect-training-chemistry-and-the-person-lifecycle.md`, `docs/delendai/proposals/ready/f00038-connect-staff-academy-facilities-and-club-evolution.md`, `docs/delendai/proposals/ready/f00039-connect-the-manager-career-and-club-authority.md`, `docs/delendai/proposals/ready/f00040-rebuild-the-workspace-as-an-original-classic-decision-loop.md`, `docs/delendai/proposals/ready/f00041-complete-the-macro-world-continuity-and-eighty-year-history.md`, `docs/delendai/proposals/ready/v00004-close-real-performance-and-self-contained-distribution.md`
- **Gate**: type
- acceptance:
  - "Each audit stage from two to fifteen has exactly one proposal with a unique id, explicit dependencies and measurable acceptance."
  - "`bun run delendai:validate` reports zero fatal errors and no duplicate ids."

### S3 — Lint this workspace's proposals and gate visible capabilities
- **Status**: done
- **DependsOn**: [S2]
- **Files**: `scripts/lint-proposals.script.ts`, `scripts/check-visible-capability-routes.script.ts`, `scripts/check-visible-capability-routes.script.spec.ts`, `scripts/visible-capability-routes.baseline.json`, `vitest.config.ts`, `package.json`
- **Gate**: unit
- acceptance:
  - "`bun run validate` lints the game's own proposal store, not the delendai checkout's, and fails on duplicate ids."
  - "A proposal that declares a visible capability must name at least one file inside the Godot presentation project."
  - "A fixture proposal that claims a Godot-visible capability without any presentation file fails the check."
  - "A domain-only proposal, a plan and an audit pass untouched."
  - "The checks run inside `bun run validate` and exit non-zero on violation."

### S4 — Remove machine-specific paths from the workspace
- **Status**: done
- **DependsOn**: [S2]
- **Files**: `scripts/delendai-root.script.ts`, `scripts/delendai-root.script.spec.ts`, `scripts/delendai-run.script.ts`, `scripts/link-delendai.script.ts`, `.delendai-root`, `package.json`, `plugins/delendai_player-club-dynasty/package.json`, `delendai.config.json`, `.vscode/mcp.json`, `.vscode/settings.json`, `README.md`, `docs/delendai/REPO-RULES.md`
- **Gate**: unit
- acceptance:
  - "No committed manifest, config or script names one developer's absolute checkout path."
  - "The checkout is discovered from an environment variable, a relative pointer file, the linked package or a sibling directory, and an explicit but broken source is reported instead of silently replaced."
  - "`@delendai/core` stays a live symlink to the checkout rather than a copy, recreated on install."
  - "Specs cover relative pointers, sibling discovery, environment override and a broken explicit source."

### S5 — Make CI build Godot, run SmokeBoot and compare budgets
- **Status**: done
- **DependsOn**: [S3]
- **Files**: `.github/workflows/game-quality.yml`, `game/scripts/run-godot-smoke.sh`, `game/godot.version`, `game/benchmarks/budgets.json`, `game/benchmarks/interaction-budgets.json`, `scripts/compare-benchmark-budgets.script.ts`, `scripts/compare-benchmark-budgets.script.spec.ts`, `.gitignore`, `game/README.md`
- **Gate**: e2e
- acceptance:
  - "The Godot version is pinned in one file that the launcher, the smoke gate and CI all read."
  - "CI provisions the pinned Godot .NET editor, imports the project and builds it headless."
  - "CI runs the SmokeBoot scene and fails on a non-zero exit or on engine errors in its output."
  - "The gate builds the assembly Godot actually loads: Godot runs the Debug build, so a Release-only build let the smoke validate stale code."
  - "Every budget names the benchmark that measures it, and the comparison fails when a result breaches its budget or has no published budget."
  - "A budget with no measurement is reported as uncovered and becomes an error under `--require-complete-coverage`, which `v00004` will turn on."
  - "A clean checkout leaves no tracked generated artifacts after the smoke run."
  - "The benchmark suite actually runs in CI: without a filter BenchmarkDotNet printed its interactive menu, exited zero and measured nothing, so the job was green and empty."
  - "A budget's `operations` field is the batch its threshold is written for, so a per-operation median is compared against the same batch."

## acceptance

- Every proposal id in the store resolves to exactly one document and the roadmap is
  reachable from the board.
- The audit's repair portfolio exists as ready, parseable proposals in dependency
  order.
- The product gate rejects a proposal that claims a Godot-visible capability with no
  route into the presentation project, proven by a negative fixture.
- No committed file pins the workspace to one machine's directory layout.
- CI builds the Godot project, runs SmokeBoot and compares benchmarks against the
  versioned budgets, failing on breach or on a missing budget.

- review-implementer: claude-opus-5
- review-log: ci_verified by claude-opus-5 — GitHub Actions confirms both new jobs on `main`: 'Build, architecture and tests' and 'Godot build and SmokeBoot' are green, and the run log shows `run-godot-smoke: SmokeBoot passed.` The budget comparison failed on every push until this fix, which is the gate working: `run-benchmarks.sh` never passed a filter, so BenchmarkDotNet printed its selection menu, exited zero and produced no results. With the filter defaulted to `*`, fifteen workloads are compared and all sit inside budget.
- review-log: verified by claude-opus-5 — `bun run validate` green (proposal lint over this workspace, visible-capability gate, typecheck, plugin tests, 23 script tests); `dotnet test game/PCDynasty.sln -c Release` green; `game/scripts/run-godot-smoke.sh --provision` provisioned Godot 4.7.1 mono, built the presentation and passed SmokeBoot headlessly; `scripts/compare-benchmark-budgets.script.ts` compared the published results against the versioned budgets; repository hygiene green.
