---
id: f00031
title: "Ship playable league content and round-trip authoring"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00030
---

# f00031 — Ship playable league content and round-trip authoring

## Goal

Grow the starter world into a genuinely playable competition — two divisions of twenty
clubs with full squads, separated managers and staff, positions, contracts, kits,
stadiums, facilities, rules and a calendar — and connect the declarative editor so a
non-programmer can author and exchange packs through the same canonical pipeline.

## why

The visible world is six clubs and eleven players, which cannot satisfy the acceptance
of a playable league, and the pack editor is built but its menu entry is disabled, so
there is no authoring or modding path at all. Import and export must share the canonical
pipeline introduced by `f00030`; otherwise a community pack diverges from the format the
game actually validates, and the promise of an externally loadable database stays
theoretical.

## non-goals

- Copying any real club, competition, person, kit or crest. All content stays fictional
  and openly licensed.
- Implementing the orchestrator or the season rules engine; `f00033` and `f00034` own them.
- Shipping generated art; placeholders remain until the workspace stage defines the
  original visual identity.

## Slices

- global_gate: e2e

### S1 — Author two playable divisions with complete squads
- **Status**: ready
- **Files**: `game/content/starter-world/data/clubs.json`, `game/content/starter-world/data/people.json`, `game/content/starter-world/data/competitions.json`, `game/content/starter-world/data/rules.json`, `game/content/starter-world/data/calendar.json`, `game/content/starter-world/manifest.json`, `game/tests/PCDynasty.Content.Integration.Tests/StarterWorldScaleTests.cs`
- **Gate**: unit
- acceptance:
  - "The pack contains forty clubs across a first and a second division with closed references."
  - "Squads hold between 880 and 1,000 players in total, each with a position, a contract and a birth date."
  - "Managers and staff are separate occupations and are never counted as players."
  - "The validator accepts the pack and rejects a mutated copy with a dangling reference."

### S2 — Model kits, stadiums, facilities and the season calendar
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Content.Abstractions/Packs/StadiumDefinition.cs`, `game/src/PCDynasty.Content.Abstractions/Packs/KitDefinition.cs`, `game/src/PCDynasty.Content.Abstractions/Packs/FacilityDefinition.cs`, `game/src/PCDynasty.Content.Abstractions/Packs/CalendarDefinition.cs`, `game/tests/PCDynasty.Content.Tests/PackDefinitionTests.cs`
- **Gate**: unit
- acceptance:
  - "Kits, stadiums, facilities and the calendar are declarative pack types with validated ranges."
  - "Each definition is immutable, validates its own values and carries a StableId."
  - "The compiler produces deterministic identifiers so two compilations agree on the hash."

### S3 — Enable round-trip authoring through the canonical pipeline
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/godot/Scenes/Editor/PackEditor.tscn`, `game/godot/Scripts/Editor/PackEditorController.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/src/PCDynasty.Editor/Documents/PackExportService.cs`, `game/tests/PCDynasty.Editor.Tests/PackRoundTripTests.cs`
- **Gate**: e2e
- acceptance:
  - "The editor entry is enabled and can create, correct and export clubs, people, kits, stadiums, facilities, competitions and rules."
  - "Export followed by import preserves the semantic hash for every editable type."
  - "An alternative external pack opens in Godot without depending on starter identifiers or names."
  - "An invalid pack surfaces its diagnostics in the editor and can be corrected and re-exported."

## acceptance

- The validator confirms forty clubs and 880 to 1,000 players with closed references
  and valid squads.
- Editor export to import preserves the semantic hash for each editable type.
- An external pack loads in Godot with no dependency on starter identifiers.
- A smoke run creates, corrects and exports an invalid pack successfully.
