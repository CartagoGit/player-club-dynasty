---
id: x00029
title: "Wire declarative editor services into Godot presentation"
kind: fix
status: done
type: proposal
track: modding
date: 2026-08-11
---

# x00029 — Wire declarative editor services into Godot presentation

## Goal

Reference editor application services from Godot presentation and track generated C# source metadata.

## why

The Godot editor slice consumes PCDynasty.Editor but its project and source metadata files are omitted from the feature slice.

## non-goals

- Implement editor screen behavior.

## Slices

- global_gate: type

### S1 — Reference editor services
- **Status**: done
- **Files**: `game/godot/PCDynasty.Presentation.csproj`
- **Gate**: type
- acceptance:
  - "Godot presentation compiles against declarative editor application services."

### S2 — Track editor controller metadata
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Editor/PackEditorController.cs.uid`, `game/godot/Scripts/Editor/ValidationReportPanel.cs.uid`
- **Gate**: type
- acceptance:
  - "Godot C# source identifiers for editor controllers are stable and tracked."

## acceptance

- Godot presentation compiles against declarative editor application services.
- Godot C# source identifiers for editor controllers are stable and tracked.
