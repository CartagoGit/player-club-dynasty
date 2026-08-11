---
id: f00025
title: "Build declarative content editor and validation workflow"
kind: feat
status: ready
type: proposal
track: modding
date: 2026-08-11
---

# f00025 — Build declarative content editor and validation workflow

## Goal

Build editor-oriented application services and Godot tools for declarative packs, competitions, kits and facilities with validation before export.

## why

Community and internal content needs safe, usable editing without exposing engine internals or arbitrary code.

## non-goals

- Editing raw saves, a general scripting IDE or copying proprietary editor UI.

## Slices

- global_gate: type

### S1 — Create editor document and validation services
- **Status**: done
- **Files**: `game/src/PCDynasty.Editor/PCDynasty.Editor.csproj`, `game/src/PCDynasty.Editor/Documents/PackDocument.cs`, `game/tests/PCDynasty.Editor.Tests/PackDocumentTests.cs`
- **Gate**: type
- acceptance:
  - "Editor models are separate from domain and save models."

### S2 — Create competition graph editor services
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Editor/Competitions/CompetitionGraphEditor.cs`, `game/src/PCDynasty.Editor/Competitions/CompetitionValidationReport.cs`, `game/tests/PCDynasty.Editor.Tests/CompetitionGraphEditorTests.cs`
- **Gate**: type
- acceptance:
  - "Editable graphs validate before producing a pack."

### S3 — Build Godot editor screens
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/Scenes/Editor/PackEditor.tscn`, `game/godot/Scripts/Editor/PackEditorController.cs`, `game/godot/Scripts/Editor/ValidationReportPanel.cs`
- **Gate**: type
- acceptance:
  - "Presentation uses editor application services and never directly writes pack files."

## acceptance

- Editor models are separate from domain and save models.
- Editable graphs validate before producing a pack.
- Presentation uses editor application services and never directly writes pack files.
