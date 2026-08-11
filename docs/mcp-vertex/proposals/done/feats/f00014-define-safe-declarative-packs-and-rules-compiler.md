---
id: f00014
title: "Define safe declarative packs and rules compiler"
kind: feat
status: done
type: proposal
track: content
date: 2026-08-11
---

# f00014 — Define safe declarative packs and rules compiler

## Goal

Separate audited engine extensions from community declarative packs and compile permitted rules into a constrained intermediate representation.

## why

Packs need extensibility without granting arbitrary code execution or making the rules engine unbounded.

## non-goals

- A Turing-complete mod language or runtime scripting from community packs.

## Slices

- global_gate: type

### S1 — Define pack trust and capability model
- **Status**: done
- **Files**: `game/src/PCDynasty.Content.Abstractions/Packs/PackTrustLevel.cs`, `game/src/PCDynasty.Content.Abstractions/Packs/PackCapability.cs`, `game/tests/PCDynasty.Content.Tests/PackCapabilityTests.cs`
- **Gate**: type
- acceptance:
  - "Audited engine extensions and declarative community packs have distinct permitted capabilities."

### S2 — Define declarative rule intermediate representation
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Rules/Compilation/RuleIntermediateRepresentation.cs`, `game/src/PCDynasty.Rules/Compilation/RuleComplexityLimits.cs`, `game/tests/PCDynasty.Rules.Tests/RuleIntermediateRepresentationTests.cs`
- **Gate**: type
- acceptance:
  - "Rules compile to a bounded, validated IR with no arbitrary execution."

### S3 — Implement pack security validation
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Content/PCDynasty.Content.csproj`, `game/src/PCDynasty.Content/Validation/PackSecurityValidator.cs`, `game/tests/PCDynasty.Content.Tests/PackSecurityValidatorTests.cs`
- **Gate**: type
- acceptance:
  - "Forbidden paths, code and excessive complexity are rejected deterministically."

## acceptance

- Audited engine extensions and declarative community packs have distinct permitted capabilities.
- Rules compile to a bounded, validated IR with no arbitrary execution.
- Forbidden paths, code and excessive complexity are rejected deterministically.
