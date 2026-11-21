---
id: c00004
title: "Align Godot metadata with version control"
kind: chore
status: done
type: proposal
track: repository
date: 2026-08-11
---

# c00004 — Align Godot metadata with version control

## Goal

Commit stable Godot resource UID metadata while ignoring only regenerable imported translation output.

## why

Godot requires .uid sidecars in version control to preserve resource references, whereas binary .translation files are generated import artifacts.

## non-goals

- Ignoring .uid files or authored translation sources.

## Slices

- global_gate: none

### S1 — Track UIDs and ignore generated translations
- **Status**: done
- **Files**: `.gitignore`, `game/godot/Scripts/Main.cs.uid`
- **Gate**: none
- acceptance:
  - "The script UID is tracked and not ignored."
  - "Generated .translation files are ignored."
  - "Godot project remains loadable headlessly."

## acceptance

- The script UID is tracked and not ignored.
- Generated .translation files are ignored.
- Godot project remains loadable headlessly.
