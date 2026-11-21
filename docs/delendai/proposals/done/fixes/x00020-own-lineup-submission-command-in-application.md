---
id: x00020
title: "Own lineup submission command in Application"
kind: fix
status: done
type: proposal
track: application
date: 2026-08-11
---

# x00020 — Own lineup submission command in Application

## Goal

Define the immutable, sport-neutral Application command that presentation uses to submit a lineup without leaking match rules into Godot.

## why

The lineup presentation slice requires an Application-owned command, but no such contract exists and placing it in the Godot assembly would invert the boundary.

## non-goals

- Validating sport-specific lineup rules or executing the command.

## Slices

- global_gate: type

### S1 — Define and verify lineup command contract
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/Matches/SubmitLineupCommand.cs`, `game/tests/PCDynasty.Application.Tests/SubmitLineupCommandTests.cs`
- **Gate**: type
- acceptance:
  - "The immutable command defensively copies a unique ordered participant selection and opaque tactical options."
  - "The command contract contains no Godot or sport-specific vocabulary."

## acceptance

- The immutable command defensively copies a unique ordered participant selection and opaque tactical options.
- The command contract contains no Godot or sport-specific vocabulary.
