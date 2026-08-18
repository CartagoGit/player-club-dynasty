---
id: p00002
title: "Bring workspace rendering inside the latency budget"
kind: perf
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00042
---

# p00002 — Bring workspace rendering inside the latency budget

## Goal

Bring every workspace screen back under the **16 ms "abrir ficha materializada"** budget in
[0009-performance-reference-and-budgets.md](../../architecture/0009-performance-reference-and-budgets.md).
Today, **Tactics, Squad, Market, Competitions, History, Overview** all breach the budget on the
canonical 512-club × 25-player starter-world; some by an order of magnitude. The cause is the
same in every screen: the render path runs an SQL open, a full-world projection walk, or a
PackedScene instantiation that ought to live behind a cached read model.

## why

Audit a00001's [0007-interaction-latency-and-work-budget.md](../../architecture/0007-interaction-latency-and-work-budget.md)
forbids Godot from opening SQLite, projecting the whole world or running the match simulator
in the render path. The shell design principle ("navigate and expand without starting heavy
work") is enforced by that doc. The architecture gate already exists; the code currently
ignores it. Every navigation into Squad, Market, Finances, Competitions, History or Tactics
either opens a new `SqliteConnection` or walks every club × every player. The technical
investigator's audit is the evidence (filed in this proposal's sibling git history).

## non-goals

- Refactoring Godot to be async everywhere. The 16 ms budget is what we must hold; it does
  not require changing the threading model.
- Rewriting the world projection to be incremental. A simple "compute once at load, invalidate
  on settle" cache is enough.
- Touching module screens that are already inside budget (World, ClubPicker).
- Removing the on-disk SQLite stores. They stay; the goal is to stop calling them from the
  render path.

## Slices

- global_gate: e2e

### S1 — Precompute a world player index and make projections cache hits
- **Status**: ready
- **Files**: `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldPeopleProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldCompetitionProjection.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldServiceTests.cs`
- **Gate**: perf
- acceptance:
  - "`WorldService` exposes `IReadOnlyDictionary<string, IReadOnlyList<PlayerRow>> PlayersByClub` and `IReadOnlyList<ClubRow> Clubs` as immutable snapshots recomputed only when the world reloads."
  - "`ListPlayers(clubId)` returns the dictionary slot in O(1) instead of re-projecting the entity every call."
  - "`ListAllCompetitions()` returns a memoised `IReadOnlyList<CompetitionRow>` invalidated by `ReloadAsync`."
  - "Existing tests stay green; a new test asserts the projections return the same instance on second call."
  - "Smoke-boot time-to-OwnershipOverview drops measurably."

### S2 — Move SQLite out of the render path and into an in-memory career state
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Market/CareerTransferMarketService.cs`, `game/src/PCDynasty.Application/Market/CareerLineupService.cs`, `game/src/PCDynasty.Application/Economy/CareerFinanceService.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerTransferStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerLineupStore.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerFinanceStore.cs`, `game/tests/PCDynasty.Application.Tests/Market/*`, `game/tests/PCDynasty.Persistence.Sqlite.Tests/*`
- **Gate**: perf
- acceptance:
  - "`CareerTransferMarketService` keeps an in-memory `HashSet<string>` of acquired player ids plus an `ImmutableArray<TransferCandidate>` of the universe; both are invalidated only on `SubmitOffer` settle or `WorldService` reload."
  - "`ListCandidates` and `ListManagedSquad` never read SQLite."
  - "`CareerLineupService` keeps the most recent plan per club in memory; `TryGetLatest` reads from memory first and falls back to the store only on cold start."
  - "`CareerFinanceService` keeps the per-club overview in memory and invalidates on every `Record(FinanceEntry)` write."
  - "Existing finance / lineup / market tests stay green; new tests prove the services return cached values after a settle."

### S3 — Cache the workspace subtree per screen and add a render-key guard
- **Status**: ready
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/godot/Scripts/Workspace/Screens/SquadScreen.cs`, `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`, `game/godot/Scripts/Workspace/Screens/ClubOverviewScreen.cs`, `game/godot/Scripts/Workspace/Screens/CompetitionsScreen.cs`, `game/godot/Scripts/Workspace/Screens/FinancesScreen.cs`
- **Gate**: perf
- acceptance:
  - "Every screen holds its last render key (managed club id + settled-state version); selecting the same sidebar entry with the same key is a no-op."
  - "Selecting a different sidebar entry clears only the changed subtree, not the entire content scroll."
  - "`RenderHeader` reads finance from the in-memory cache via the controller, not from `SqliteCareerFinanceStore`."
  - "Smoke-boot still passes."

### S4 — Preload the Tactics PackedScene once and reuse the controller instance
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Matches/LineupSelectionController.cs`
- **Gate**: perf
- acceptance:
  - "`TacticsScreen` loads `LineupSelection.tscn` exactly once per workspace lifetime and keeps the instantiated `LineupSelectionController` as a persistent child."
  - "Re-rendering tactics calls a new `ResetView()` on the existing controller instead of re-instantiating."
  - "The smoke-boot path `Workspace/Layout/ContentMargin/ContentScroll/Content/LineupSelection` still resolves to the same controller instance across two navigations."

### S5 — Benchmark and verify the latency budget
- **Status**: ready
- **DependsOn**: [S1, S2, S3, S4]
- **Files**: `game/benchmarks/PCDynasty.Benchmarks/`, `game/benchmarks/interaction-budgets.json`, `scripts/compare-benchmark-budgets.script.ts`, `game/godot/Tests/SmokeBoot.gd`
- **Gate**: perf
- acceptance:
  - "A BenchmarkDotNet case measures each screen's `Render` method against the production starter-world catalog; the p50 lands below 16 ms and the p95 below 32 ms on the pinned machine."
  - "`scripts/compare-benchmark-budgets.script.ts` runs in CI and fails when any screen regresses above its budget."
  - "`SmokeBoot.gd` adds the same three benchmark timings using `Time.get_ticks_msec` so a 30-second CI signal covers at least the top-3 screens."
  - "Budget entries are listed in `game/benchmarks/interaction-budgets.json` so future contributors see the target."

## acceptance

- `ManagerWorkspaceController.RenderCurrent()` does not call `SqliteConnection.Open`, never
  calls `IDisposable` on any of the SQLite stores, never instantiates a `LineupSelectionController`,
  and never walks the full world club × people projection.
- The p50 + p95 of every screen's `Render` lands inside the budgets in
  [0009-performance-reference-and-budgets.md](../../architecture/0009-performance-reference-and-budgets.md).
- The smoke-boot still passes, runs in <60 seconds, and gates on the new timings.
- No SQLite store is removed; SQLite is the cold-storage path that the in-memory cache
  hydrates from on `Configure`.
