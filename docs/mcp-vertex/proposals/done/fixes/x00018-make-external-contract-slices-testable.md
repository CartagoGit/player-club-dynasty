---
id: x00018
title: "Make external contract slices testable"
kind: fix
status: done
type: proposal
track: contracts
date: 2026-08-11
---

# x00018 — Make external contract slices testable

## Goal

Assign envelope tests and compatibility fixture configuration to the public contracts proposal.

## why

The envelope slice has no owned acceptance test and the compatibility fixture cannot be copied without owning the test project.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct public contracts ownership
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00015-publish-versioned-external-contracts-and-fixtures.md`
- **Gate**: none
- acceptance:
  - "The first slice owns the contract test project and envelope tests, including generic fixture-copy configuration."

## acceptance

- The first slice owns the contract test project and envelope tests, including generic fixture-copy configuration.
