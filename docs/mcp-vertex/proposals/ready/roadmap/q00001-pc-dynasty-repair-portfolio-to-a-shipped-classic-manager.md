---
id: q00001
title: "PC Dynasty repair portfolio to a shipped classic manager"
kind: plan
status: ready
type: proposal
track: roadmap
date: 2026-08-18
supersedes: "r00001 roadmap variant (identifier collision with the r00001 refactor)"
audit-source: a00001
---

# q00001 — PC Dynasty: repair portfolio to a shipped classic manager

## Goal

Drive PC Dynasty from "green domain assemblies plus a partial vertical slice" to a
shipped, self-contained desktop manager where every advertised capability is
orchestrated, persisted and reachable from the Godot executable. This plan owns the
dependency order; the child proposals own the work.

## why

Audit `a00001` (`docs/architecture/0010-initial-plan-runtime-compliance-audit.md`)
established with file-level evidence that proposal status cannot be read as product
completion:

- Eleven completed modules are **islands**: `PCDynasty.Career`, `PCDynasty.Careers`,
  `PCDynasty.Club`, `PCDynasty.Football`, `PCDynasty.World`, scouting, training,
  world history, the world store and the deterministic protocol have no productive
  consumer reachable from `game/godot/Scenes/Main.tscn`.
- The runtime bypasses the canonical pipeline: content skips validator/compiler, the
  application layer re-implements football rules that `FootballContestPack` already
  owns, and there is no single `AdvanceWorld` orchestrator or career clock.
- Visible defects contradict invariants: managers are listed as players, three match
  event types are unreachable, a cup is simulated as a league, and a transfer never
  credits the selling club.
- The previous roadmap was invisible because two documents claimed `id: r00001`, and
  its content had already been superseded (it proposed the removed
  `WorldService.Instance` pattern, assumed eight starter clubs when the pack has six,
  and estimated the whole game at ~64 hours without an orchestrator existing).

This plan replaces that roadmap with the audit's dependency-ordered portfolio, one
parseable proposal per stage, so the board can hand out exactly one actionable unit
at a time.

## non-goals

- Re-opening or invalidating the historical scope of proposals already `done`.
- Deleting validated domain modules because they are not yet integrated; every stage
  either connects an existing engine or retires it explicitly.
- Creating a third parallel implementation of any capability. A repair either adopts
  the canonical module or removes the duplicate in the same or a dependent slice.
- Copying PC Fútbol assets, layout, naming or identity. The visual grammar is
  original and governed by `docs/design/manager-ui-style-guide.md`.
- Shipping capabilities behind stubs that only pretend to be integrated.

## Slices

- global_gate: e2e

### S1 — Sanitise the proposal registry, the roadmap and the product gates
- **Status**: review
- **Owner**: `c00005`
- **Files**: `docs/mcp-vertex/proposals/ready/roadmap/q00001-pc-dynasty-repair-portfolio-to-a-shipped-classic-manager.md`, `scripts/lint-proposals.script.ts`, `scripts/check-visible-capability-routes.script.ts`, `scripts/compare-benchmark-budgets.script.ts`, `game/scripts/run-godot-smoke.sh`, `.github/workflows/game-quality.yml`
- **Gate**: e2e
- acceptance:
  - "Every proposal id resolves to exactly one document; the roadmap is reachable from the board."
  - "The audit's fifteen repair stages exist as parseable proposals with measurable gates."
  - "CI installs Godot, builds the Godot project and runs SmokeBoot; benchmark results are compared against versioned budgets instead of only published."
  - "A negative check fails any proposal that declares a visible capability without a route from Main.tscn."

### S2 — Correct managed-club identity and authority
- **Status**: ready
- **DependsOn**: [S1]
- **Owner**: `x00041`
- **Files**: `docs/mcp-vertex/proposals/ready/x00041-correct-managed-club-identity-and-command-authority.md`
- **Gate**: e2e
- acceptance:
  - "Squad and tactics show players only; managers and staff have their own read models."
  - "Commands accept the managed club context only; browsing a rival mutates nothing."

### S3 — Establish the canonical, indexed world loader
- **Status**: ready
- **DependsOn**: [S2]
- **Owner**: `f00030`
- **Files**: `docs/mcp-vertex/proposals/ready/f00030-establish-the-canonical-indexed-world-loader.md`
- **Gate**: e2e
- acceptance:
  - "Reader → normalisation → validator → compiler produces an immutable, hashed, indexed snapshot."
  - "Invalid content fails with a diagnostic and is never coerced to zero."

### S4 — Ship playable content and round-trip authoring
- **Status**: ready
- **DependsOn**: [S3]
- **Owner**: `f00031`
- **Files**: `docs/mcp-vertex/proposals/ready/f00031-ship-playable-league-content-and-round-trip-authoring.md`
- **Gate**: e2e
- acceptance:
  - "Two divisions of twenty clubs with full squads validate and load."
  - "Editor export → import preserves the semantic hash through the canonical pipeline."

### S5 — Unify career, external persistence and transactions
- **Status**: ready
- **DependsOn**: [S3]
- **Owner**: `f00032`
- **Files**: `docs/mcp-vertex/proposals/ready/f00032-unify-career-external-persistence-and-transactions.md`
- **Gate**: e2e
- acceptance:
  - "One save contract family, one manifest, one shared SQLite unit of work."
  - "Fault injection never leaves mixed state; multiple slots and an external database stay isolated."

### S6 — Build the calendar and the minimal deterministic orchestrator
- **Status**: ready
- **DependsOn**: [S4, S5]
- **Owner**: `f00033`
- **Files**: `docs/mcp-vertex/proposals/ready/f00033-build-the-calendar-and-deterministic-world-orchestrator.md`
- **Gate**: e2e
- acceptance:
  - "A single AdvanceWorld owns date, world version, seed, rules hash, fidelity and work budget."
  - "The five ADR 0003 determinism gates pass and AdvanceDay/Week/ToDate survive save/load."

### S7 — Integrate FootballPack, lineups and competition formats
- **Status**: ready
- **DependsOn**: [S2, S4, S6]
- **Owner**: `f00034`
- **Files**: `docs/mcp-vertex/proposals/ready/f00034-integrate-footballpack-lineups-and-competition-formats.md`
- **Gate**: e2e
- acceptance:
  - "Application stops duplicating football rules; the pack governs minutes, events and points."
  - "League and cup produce different formats; facts name the people involved."

### S8 — Connect authoritative economy, contracts and market
- **Status**: ready
- **DependsOn**: [S4, S5, S6, S7]
- **Owner**: `f00035`
- **Files**: `docs/mcp-vertex/proposals/ready/f00035-connect-authoritative-economy-contracts-and-market.md`
- **Gate**: e2e
- acceptance:
  - "One transfer fact debits the buyer, credits the seller and moves ownership."
  - "Money and ownership conservation hold before and after every rollback."

### S9 — Connect the player dossier, knowledge and scouting
- **Status**: ready
- **DependsOn**: [S2, S4, S5, S6]
- **Owner**: `f00036`
- **Files**: `docs/mcp-vertex/proposals/ready/f00036-connect-the-player-dossier-knowledge-and-scouting.md`
- **Gate**: e2e
- acceptance:
  - "Truth and perception stay separated across the UI contract."
  - "Indexed knowledge queries over 160,000 people stay within 16 ms."

### S10 — Connect training, chemistry and the person lifecycle
- **Status**: ready
- **DependsOn**: [S4, S5, S6, S7, S9]
- **Owner**: `f00037`
- **Files**: `docs/mcp-vertex/proposals/ready/f00037-connect-training-chemistry-and-the-person-lifecycle.md`
- **Gate**: e2e
- acceptance:
  - "Development, wear, injury, recovery, chemistry and motivation are calendar units with persisted facts."
  - "160,000 development steps stay within 1,000 ms and reproduce in serial and parallel."

### S11 — Connect staff, academy, facilities and club evolution
- **Status**: ready
- **DependsOn**: [S4, S5, S6, S8, S10]
- **Owner**: `f00038`
- **Files**: `docs/mcp-vertex/proposals/ready/f00038-connect-staff-academy-facilities-and-club-evolution.md`
- **Gate**: e2e
- acceptance:
  - "Employees age, progress, retire and are replaced across thirty simulated seasons."
  - "Facilities decay, works complete over time and demand never exceeds sellable capacity."

### S12 — Connect the manager career
- **Status**: ready
- **DependsOn**: [S5, S6, S8, S11]
- **Owner**: `f00039`
- **Files**: `docs/mcp-vertex/proposals/ready/f00039-connect-the-manager-career-and-club-authority.md`
- **Gate**: e2e
- acceptance:
  - "Contract, reputation, relationships, objectives, offers, dismissal and unemployment govern which club can be mutated."
  - "Changing club transfers authority with no state leaking from the previous club."

### S13 — Rebuild the workspace as an original classic decision loop
- **Status**: ready
- **DependsOn**: [S2, S5, S6, S7, S8, S9, S10, S11, S12]
- **Owner**: `f00040`
- **Files**: `docs/mcp-vertex/proposals/ready/f00040-rebuild-the-workspace-as-an-original-classic-decision-loop.md`
- **Gate**: e2e
- acceptance:
  - "At most seven grouped areas, a persistent header, an agenda-inbox and an active-section state."
  - "Screens contain no rules and no infrastructure; every component renders empty, loading, ready, stale and error."

### S14 — Complete the macro world, continuity and eighty years of history
- **Status**: ready
- **DependsOn**: [S6, S7, S8, S9, S10, S11, S12]
- **Owner**: `f00041`
- **Files**: `docs/mcp-vertex/proposals/ready/f00041-complete-the-macro-world-continuity-and-eighty-year-history.md`
- **Gate**: e2e
- acceptance:
  - "320 leagues and 6,400 clubs advance without freezing economy, squads, employees or rules."
  - "An eighty-year golden career preserves every historical type within the declared disk and memory ceilings."

### S15 — Close real performance and self-contained distribution
- **Status**: ready
- **DependsOn**: [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14]
- **Owner**: `v00004`
- **Files**: `docs/mcp-vertex/proposals/ready/v00004-close-real-performance-and-self-contained-distribution.md`
- **Gate**: e2e
- acceptance:
  - "Every mandatory workload runs through the game's own composition root and has a versioned budget."
  - "Clean Windows and Linux exports play, save and continue without a preinstalled SDK, runtime or editor."

## acceptance

- Every stage above exists as its own parseable proposal, with a unique id, explicit
  dependencies and measurable gates taken from the audit.
- The board resolves exactly one document per id, and this plan is visible to it.
- No stage is closed while its capability is unreachable from `Main.tscn`, unpersisted
  or unmeasured on the real path.
- The plan closes only when every child proposal is `done` and the shipped build opens,
  plays, saves and continues on a clean machine.
