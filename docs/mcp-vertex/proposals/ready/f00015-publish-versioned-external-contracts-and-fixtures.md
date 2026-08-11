---
id: f00015
title: "Publish versioned external contracts and fixtures"
kind: feat
status: ready
type: proposal
track: integration
date: 2026-08-11
---

# f00015 — Publish versioned external contracts and fixtures

## Goal

Create a stable contracts surface for commands, results, facts, content import and save manifests with semantic versioning and compatibility fixtures.

## why

External tools, importers and future clients must integrate without depending on mutable entities or internal project structure.

## non-goals

- A public network API or distribution of packages before the contracts stabilize.

## Slices

- global_gate: type

### S1 — Define public contract ownership and versions
- **Status**: done
- **Files**: `game/src/PCDynasty.Contracts/ContractVersion.cs`, `game/src/PCDynasty.Contracts/Commands/CommandEnvelope.cs`, `game/src/PCDynasty.Contracts/Events/EventEnvelope.cs`, `game/tests/PCDynasty.Contracts.Tests/PCDynasty.Contracts.Tests.csproj`, `game/tests/PCDynasty.Contracts.Tests/ContractEnvelopeTests.cs`
- **Gate**: type
- acceptance:
  - "Contracts expose immutable versioned envelopes and causal metadata."

### S2 — Define import and save contract schemas
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Contracts/Content/ContentImportContract.cs`, `game/src/PCDynasty.Contracts/Persistence/SaveManifestContract.cs`, `game/tests/PCDynasty.Contracts.Tests/ContractSchemaTests.cs`
- **Gate**: type
- acceptance:
  - "External data and save metadata contracts remain independent from domain entities."

### S3 — Add compatibility fixture suite
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/tests/PCDynasty.Contracts.Tests/Fixtures/v1-command.json`, `game/tests/PCDynasty.Contracts.Tests/CompatibilityFixtureTests.cs`
- **Gate**: type
- acceptance:
  - "Breaking, compatible and deprecated contract changes are detected in CI."

## acceptance

- Contracts expose immutable versioned envelopes and causal metadata.
- External data and save metadata contracts remain independent from domain entities.
- Breaking, compatible and deprecated contract changes are detected in CI.
