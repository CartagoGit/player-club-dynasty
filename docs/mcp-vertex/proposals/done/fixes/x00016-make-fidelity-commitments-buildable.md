---
id: x00016
title: "Make fidelity commitments buildable"
kind: fix
status: done
type: proposal
track: world
date: 2026-08-11
---

# x00016 — Make fidelity commitments buildable

## Goal

Assign the World production/test projects and solution registration to the fidelity proposal.

## why

The proposal cannot create or run its World project because its project and test boundaries are not owned.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct fidelity proposal ownership
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00013-define-macro-and-detailed-fidelity-commitments.md`
- **Gate**: none
- acceptance:
  - "The fidelity contract slice owns World project, test project, fixture-copy configuration and solution registration."

## acceptance

- The fidelity contract slice owns World project, test project, fixture-copy configuration and solution registration.
