---
id: f00001
title: "Establish Godot and .NET game foundation"
kind: feat
status: ready
type: proposal
track: architecture
date: 2026-08-11
---

# f00001 — Establish Godot and .NET game foundation

## Goal

Choose the runtime boundary and bootstrap the smallest testable PC Dynasty vertical-slice foundation without coupling the simulation core to the presentation engine.

## why

The initial plan requires a proposal before substantial implementation and the repository has no runnable game scaffold yet.

## non-goals

- Implementing the full football simulation, economy, world model, or modding system.
- Selecting or integrating Unity, Electron, or Tauri.
- Shipping a complete Godot interface.

## Slices

- global_gate: type

### S1 — Record runtime and dependency decision
- **Status**: done
- **Files**: `docs/architecture/0001-godot-dotnet-boundaries.md`
- **Gate**: none
- acceptance:
  - "Explains the engine choice, supported platforms, core/presentation dependency rule, project layout, and prerequisites."
  - "States the web-export limitation for Godot C# and the contingency if browser delivery becomes required."

### S2 — Bootstrap the engine-neutral simulation core
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/PCDynasty.sln`, `game/Directory.Build.props`, `game/src/PCDynasty.Core/PCDynasty.Core.csproj`, `game/src/PCDynasty.Core/Domain/PersonId.cs`, `game/src/PCDynasty.Core/Domain/AttributeId.cs`, `game/src/PCDynasty.Core/Domain/AttributeVector.cs`, `game/tests/PCDynasty.Core.Tests/PCDynasty.Core.Tests.csproj`, `game/tests/PCDynasty.Core.Tests/Domain/AttributeVectorTests.cs`
- **Gate**: type
- acceptance:
  - "The core has no Godot references."
  - "Tests pin numeric ID-based attribute storage, including validation at the input boundary."
  - "The solution builds and tests with the documented .NET SDK."

### S3 — Create the Godot presentation adapter shell
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/project.godot`, `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scripts/Main.cs`, `game/godot/Scenes/Main.tscn`
- **Gate**: type
- acceptance:
  - "The presentation project references the core in one direction only."
  - "A minimal scene can display a core-derived read model without the core referencing Godot."

## acceptance

- Explains the engine choice, supported platforms, core/presentation dependency rule, project layout, and prerequisites.
- States the web-export limitation for Godot C# and the contingency if browser delivery becomes required.
- The core has no Godot references.
- Tests pin numeric ID-based attribute storage, including validation at the input boundary.
- The solution builds and tests with the documented .NET SDK.
- The presentation project references the core in one direction only.
- A minimal scene can display a core-derived read model without the core referencing Godot.
