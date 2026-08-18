---
id: f00030
title: "Establish the canonical indexed world loader"
kind: feat
status: ready
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

### S1 — Compose the canonical loading pipeline behind one application port
- **Status**: ready
- **Files**: `game/src/PCDynasty.Application/Content/IWorldLoader.cs`, `game/src/PCDynasty.Application/Content/WorldLoader.cs`, `game/src/PCDynasty.Application/Content/WorldSnapshot.cs`, `game/tests/PCDynasty.Application.Tests/Content/WorldLoaderTests.cs`
- **Gate**: unit
- acceptance:
  - "The loader runs reader, normalisation, validator and compiler in that order and returns an immutable snapshot."
  - "Invalid content fails with a diagnostic that names the entity, the attribute and the source, and is never coerced to zero."
  - "The snapshot exposes a content hash and StableIds for every entity."
  - "The tolerant value reader is removed from the productive path in this slice."

### S2 — Index the snapshot for constant-time access
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Application/Content/WorldIndex.cs`, `game/src/PCDynasty.Presentation.Core/WorldClubProjection.cs`, `game/tests/PCDynasty.Application.Tests/Content/WorldIndexTests.cs`, `game/benchmarks/PCDynasty.Benchmarks/WorldLoadBenchmarks.cs`, `game/benchmarks/budgets.json`
- **Gate**: perf
- acceptance:
  - "Lookups by id, by club and by competition are constant or logarithmic, never linear scans."
  - "Indexed queries over 160,000 people keep a median at or below 16 ms."
  - "Cold compile and index of the starter pack stays at or below 2,000 ms."
  - "The workload is declared in the versioned budgets file and compared in CI."

### S3 — Let the game choose an integrated or external content source
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Shell/ContentSourcePicker.cs`, `game/src/PCDynasty.Application/Content/ContentSourceDescriptor.cs`, `game/tests/PCDynasty.Content.Integration.Tests/EquivalentSourceHashTests.cs`
- **Gate**: e2e
- acceptance:
  - "The bootstrap no longer hard-codes a directory or a reader; it resolves a described source."
  - "Equivalent JSON, CSV and SQLite fixtures produce the same content hash."
  - "The player can pick an external source from the shell and the choice survives a restart."
  - "SmokeBoot loads the integrated source and an external source without runtime errors."

## acceptance

- Invalid content fails with a diagnostic and never becomes a zero value.
- Equivalent JSON, CSV and SQLite fixtures compile to the same hash.
- Indexed lookups over 160,000 people hold a median at or below 16 ms, and the cold
  compile and index of the starter pack stays at or below 2,000 ms.
- The productive path uses the canonical pipeline only; the tolerant reader is gone.
