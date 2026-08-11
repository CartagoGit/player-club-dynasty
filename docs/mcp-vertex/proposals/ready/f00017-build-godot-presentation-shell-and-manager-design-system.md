---
id: f00017
title: "Build Godot presentation shell and manager design system"
kind: feat
status: ready
type: proposal
track: presentation
date: 2026-08-11
---

# f00017 — Build Godot presentation shell and manager design system

## Goal

Create the Godot presentation boundary and a proprietary 2D manager UI design system with rapid navigation, dense readable information and progressive disclosure.

## why

The game needs the usability spirit of classic football managers without copying PC Fútbol layouts, assets, typography or trade dress.

## non-goals

- Copying PC Fútbol UI/assets, 3D match presentation or full gameplay screens.

## Slices

- global_gate: type

### S1 — Create Godot presentation shell
- **Status**: done
- **Files**: `game/godot/project.godot`, `game/godot/PCDynasty.Presentation.csproj`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scenes/Main.tscn`
- **Gate**: type
- acceptance:
  - "Godot references Application contracts one-way and has no domain logic."

### S2 — Define proprietary manager UI tokens and patterns
- **Status**: pending
- **DependsOn**: [S1]
- **Files**: `game/godot/Theme/PCDynastyTheme.tres`, `game/godot/Theme/ManagerDesignTokens.cs`, `docs/design/manager-ui-style-guide.md`
- **Gate**: none
- acceptance:
  - "Defines original colors, typography, spacing, navigation and progressive disclosure patterns."

### S3 — Build reusable navigation and information components
- **Status**: pending
- **DependsOn**: [S1, S2]
- **Files**: `game/godot/Scripts/Components/SidebarNavigator.cs`, `game/godot/Scripts/Components/MetricCard.cs`, `game/godot/Scripts/Components/DetailDrilldown.cs`
- **Gate**: type
- acceptance:
  - "Components are reusable, accessible and render supplied read models only."

## acceptance

- Godot references Application contracts one-way and has no domain logic.
- Defines original colors, typography, spacing, navigation and progressive disclosure patterns.
- Components are reusable, accessible and render supplied read models only.
