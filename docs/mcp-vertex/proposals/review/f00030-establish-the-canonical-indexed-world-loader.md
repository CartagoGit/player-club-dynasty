---
id: f00030
title: "Establish the canonical indexed world loader"
kind: feat
status: review
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: x00041
---

# f00030 — Establish the canonical indexed world loader

## Goal

Give the game a single world loader in the application layer that composes reader,
normalisation, validator and compiler into an immutable, hashed and indexed snapshot,
and let the player choose an integrated or external JSON, CSV or SQLite source.

## why

The runtime bypasses the pipeline the project already built and tested. The bootstrap
pins `starter-world`, builds a `JsonContentReader` directly and hands normalised models
to the world service; the catalogue reads five fixed file names and never invokes
`PackValidator` or `PackCompiler`, and its tolerant value reader silently turns invalid
integers into zero — which is why a manager with no attributes was rendered as a
zero-value player. Nothing carries a StableId or a content hash, so a save cannot bind
to the content it was produced from. Every lookup is a linear scan: listing a squad
walks all people, finding a club rebuilds the whole list, and the market walks clubs and
re-queries people per club, which cannot reach the target world scale.

## non-goals

- Extending the starter content itself; `f00031` owns playable content.
- Changing save schemas; `f00032` owns career persistence.
- Adding new screens; the loader keeps the current surfaces working.

## Slices

- global_gate: e2e

### S1 — Compose the canonical loading pipeline behind one loader
- **Status**: done
- **Files**: `game/src/PCDynasty.Content/Worlds/WorldPackLoader.cs`, `game/src/PCDynasty.Content/Worlds/WorldPackSource.cs`, `game/src/PCDynasty.Content/Worlds/WorldPackLoadResult.cs`, `game/src/PCDynasty.Content/Worlds/WorldContentHash.cs`, `game/src/PCDynasty.Presentation.Core/WorldContentCatalog.cs`, `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/tests/PCDynasty.Content.Tests/WorldPackLoaderTests.cs`, `game/tests/PCDynasty.Presentation.Tests/WorldLoadingDiagnosticsTests.cs`
- **Gate**: unit
- acceptance:
  - "The loader runs reader, merge, validator and compiler in that order and returns an immutable compiled pack."
  - "Invalid content fails with a diagnostic that names the entity, the attribute and the source, and is never coerced to zero."
  - "Merging the documents makes cross-document references verifiable, which per-file validation could not do."
  - "The result exposes a content hash a save can bind to."
  - "The loader lives in the content layer, not in Application, so the architecture gate that forbids Application from referencing content adapters stays green."

### S2 — Index the snapshot for constant-time access
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Presentation.Core/WorldSnapshot.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldPeopleProjection.cs`, `game/src/PCDynasty.Presentation.Core/WorldCompetitionProjection.cs`, `game/benchmarks/PCDynasty.Benchmarks/WorldLoadBenchmarks.cs`, `game/benchmarks/PCDynasty.Benchmarks/PCDynasty.Benchmarks.csproj`, `game/benchmarks/budgets.json`, `game/tests/PCDynasty.Benchmark.Tests/WorldQueryBudgetTests.cs`
- **Gate**: perf
- acceptance:
  - "Lookups by id and by club are indexed once per load; a squad query reads the club's own people and finding a club no longer rebuilds the club list."
  - "Indexed queries over 160,000 people keep a median at or below 16 ms."
  - "A cold compile and index of the target-scale world stays at or below 2,000 ms."
  - "The workloads are declared in the versioned budgets file, compared in CI, and guarded by a fast test gate that does not need a benchmark run."

### S3 — Let the game choose an integrated or external content source
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Content/Worlds/ContentSourceDescriptor.cs`, `game/src/PCDynasty.Content.Importers/ContentReaderFactory.cs`, `game/src/PCDynasty.Presentation.Core/ContentSourcePreference.cs`, `game/src/PCDynasty.Presentation.Core/WorldService.cs`, `game/src/PCDynasty.Presentation.Core/WorldContentCatalog.cs`, `game/godot/Scripts/Shell/ContentSourceSection.cs`, `game/godot/Scripts/Shell/ContentSourceSection.cs.uid`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Tests/SmokeBoot.gd`, `game/tests/PCDynasty.Presentation.Tests/ContentSourceSelectionTests.cs`, `game/tests/PCDynasty.Content.Integration.Tests/EquivalentSourceHashTests.cs`
- **Gate**: e2e
- acceptance:
  - "The bootstrap no longer hard-codes a directory or a reader; it resolves a described source and picks the reader from its format."
  - "Equivalent JSON, CSV and SQLite sources produce the same content hash, which excludes the provider label so the same data keeps its identity under any source name."
  - "The player can pick an external source from the options panel and the choice survives a restart; a stale or unsupported choice falls back to the integrated world instead of blocking the game."
  - "Swapping worlds never blocks the frame and never reconnects the menu's signals."
  - "SmokeBoot loads the integrated source, the same world as an external source and back, then plays a full career without runtime errors."

## acceptance

- Invalid content fails with a diagnostic and never becomes a zero value.
- Equivalent JSON, CSV and SQLite fixtures compile to the same hash.
- Indexed lookups over 160,000 people hold a median at or below 16 ms, and the cold
  compile and index of the starter pack stays at or below 2,000 ms.
- The productive path uses the canonical pipeline only; the tolerant reader is gone.

- review-implementer: claude-opus-5
- review-log: partial by claude-opus-5 — S1 and S2 shipped and verified. S1: the runtime now loads through read → merge → validate → compile → hash; a corrupted attribute and a dangling cross-document reference both stop the load with a diagnostic naming the entity, the attribute and the document, proven against the real starter world; equivalent JSON, CSV and SQLite sources compile to the same content hash. S2: the snapshot indexes people by club, entities by id and country names once per load, measured through the game's own WorldService over 6,400 clubs and 160,000 people — squad query 0.009 ms and club lookup 0.001 ms against a 16 ms budget, cold compile and index 1,508 ms against 2,000 ms; all three are versioned workloads compared by the CI gate and by a fast test that needs no benchmark run. S3: the options panel now loads an external world and restores the integrated one, remembering the choice across restarts, and SmokeBoot walks that path. Two real defects surfaced while wiring it and are fixed: awaiting the load on the Godot main thread deadlocked the game, and rebuilding the menu controller made Godot reject the second signal connection, so the world is swapped in place. The smoke gate also stopped silencing build output, which had hidden a Debug-only analyzer failure. 27 suites, format, architecture gates and headless SmokeBoot green.
