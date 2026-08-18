---
id: c00001
title: "Define manager engine architecture contracts"
kind: chore
status: done
type: proposal
track: architecture
date: 2026-08-11
---

# c00001 — Define manager engine architecture contracts

## Goal

Turn the initial plan into a reviewable, staged architecture that protects simulation correctness, deterministic orchestration, evolution of saves and packs, and Godot/Core independence before implementation expands.

## why

The user identified the long-lived manager engine architecture and its orchestration as the primary risk. The initial plan explicitly requires architecture, persistence, simulation and performance decisions to be proposed before large implementation.

## non-goals

- Building the complete Core or Godot user interface.
- Choosing detailed football attributes or gameplay balancing.
- Designing every future sport or an unrestricted scripting language.

## Slices

- global_gate: none

### S1 — Define module boundaries and dependency contracts
- **Status**: done
- **Files**: `docs/architecture/0002-manager-engine-module-map.md`
- **Gate**: none
- acceptance:
  - "Defines modules, permitted dependency directions, public ports, and forbidden couplings."
  - "Defines an anti-corruption boundary between Core, persistence, packs and Godot."

### S2 — Define deterministic simulation orchestration
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `docs/architecture/0003-simulation-orchestration.md`
- **Gate**: none
- acceptance:
  - "Specifies time advancement, command processing, fact application, reproducibility, ordering and macro versus detailed simulation."
  - "Includes failure handling and observability boundaries."

### S3 — Define save and pack evolution contracts
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `docs/architecture/0004-save-and-pack-compatibility.md`
- **Gate**: none
- acceptance:
  - "Separates persisted facts from derived views and establishes schema, pack and rule version compatibility."
  - "Defines migration, validation and recovery expectations."

### S4 — Define architecture gates and delivery sequence
- **Status**: done
- **DependsOn**: [S2, S3]
- **Files**: `docs/architecture/0005-architecture-gates-and-roadmap.md`
- **Gate**: none
- acceptance:
  - "Maps the smallest vertical slices to architectural acceptance gates, tests and benchmarks."
  - "Prevents implementation of macro-world systems before their contracts and measurements exist."

## acceptance

- Defines modules, permitted dependency directions, public ports, and forbidden couplings.
- Defines an anti-corruption boundary between Core, persistence, packs and Godot.
- Specifies time advancement, command processing, fact application, reproducibility, ordering and macro versus detailed simulation.
- Includes failure handling and observability boundaries.
- Separates persisted facts from derived views and establishes schema, pack and rule version compatibility.
- Defines migration, validation and recovery expectations.
- Maps the smallest vertical slices to architectural acceptance gates, tests and benchmarks.
- Prevents implementation of macro-world systems before their contracts and measurements exist.
