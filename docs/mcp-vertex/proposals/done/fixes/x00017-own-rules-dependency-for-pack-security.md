---
id: x00017
title: "Own rules dependency for pack security"
kind: fix
status: done
type: proposal
track: content
date: 2026-08-11
---

# x00017 — Own rules dependency for pack security

## Goal

Allow the pack security slice to add the Rules dependency to the Content compiler project.

## why

PackSecurityValidator must consume the bounded Rules IR, but its slice does not own the Content project file.

## non-goals

- TODO: what this proposal deliberately skips.

## Slices

- global_gate: none

### S1 — Correct pack security ownership
- **Status**: done
- **Files**: `docs/mcp-vertex/proposals/ready/f00014-define-safe-declarative-packs-and-rules-compiler.md`
- **Gate**: none
- acceptance:
  - "The security validation slice owns PCDynasty.Content.csproj."

## acceptance

- The security validation slice owns PCDynasty.Content.csproj.
