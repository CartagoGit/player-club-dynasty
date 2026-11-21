---
id: c00003
title: "Ignore generated game engine artifacts"
kind: chore
status: done
type: proposal
track: repository
date: 2026-08-11
---

# c00003 — Ignore generated game engine artifacts

## Goal

Keep generated .NET and Godot editor/build outputs out of version control while preserving all source, project, pack and authored asset files.

## why

Running the first .NET tests produced bin/ and obj/ trees that should never be committed; future Godot work will also create an editor cache.

## non-goals

- Ignoring authored Godot resources, imported content packs or source assets.
- Deleting unrelated work from other agents.

## Slices

- global_gate: none

### S1 — Add .NET and Godot ignore rules
- **Status**: done
- **Files**: `.gitignore`
- **Gate**: none
- acceptance:
  - "Nested .NET bin/ and obj/ outputs are ignored."
  - "Godot .godot/ caches and common local C# user files are ignored."
  - "No authored project or source file becomes ignored."

## acceptance

- Nested .NET bin/ and obj/ outputs are ignored.
- Godot .godot/ caches and common local C# user files are ignored.
- No authored project or source file becomes ignored.
