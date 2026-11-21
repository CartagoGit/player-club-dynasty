---
id: f00043
title: "Extend the options panel with language and theme preferences"
kind: feat
status: review
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00042
---

# f00043 — Extend the options panel with language and theme preferences

## Goal

Make the options panel the place where the player chooses the language of the
shell and the colour palette of the interface. Both choices are applied
immediately, persisted, and restored on the next launch alongside the existing
`PresentationPreferences` introduced by `f00042` S3.

## why

The player keeps the game's language and visual identity in two places today:
the source code (every label is a hardcoded Spanish string) and a single
`Theme/PCDynastyTheme.tres` resource (every StyleBox uses the same dark-blue
palette). The shell therefore cannot speak English to a player who prefers it,
and a player who dislikes the palette has no way to change it. The owner asked
for many themes and for English and Spanish at minimum, both surfaced in the
options panel with country flags for the language picker.

The initial plan demands both without prescribing how: line 583 treats
`idiomas` as a Person attribute (the speakers of a language), and the
`PresentationPack` clause in §10.5 calls `temas` an explicit overridable
artefact. Wiring the two through the same options panel that `f00042` S3 sets
up delivers the player-facing surface without breaking the architecture: the
domain exposes an immutable catalog; the presentation applies it.

## non-goals

- Migrating every domain string to i18n in this proposal. The vertical slice
  extracts the shell's user-visible strings (main menu, options panel, sidebar
  rail, header, status strip) and the workspace titles. Module screens keep
  their literal copy until a follow-up proposal.
- Translating community content. Packs remain language-agnostic; only the
  shell speaks the chosen locale.
- Theme-aware loadouts. A theme is a single palette, not a layout variation.
- Adding more than two languages in this proposal. The catalog is extensible
  by design so the next proposal can add a third without structural changes.

## Slices

- global_gate: e2e

### S1 — Define swappable theme palettes and a theme catalog
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/ThemePalette.cs`, `game/src/PCDynasty.Presentation.Core/ThemePaletteCatalog.cs`, `game/src/PCDynasty.Presentation.Core/ThemePaletteId.cs`, `game/tests/PCDynasty.Presentation.Tests/ThemePaletteCatalogTests.cs`
- **Gate**: e2e
- acceptance:
  - "A `ThemePalette` is an immutable record with the colour tokens the design system declares."
  - "The catalog exposes at least six palettes: Clásico Oscuro, Blanco Papel, Azul Profundo, Bosque, Atardecer, Tinta."
  - "Every palette keeps a minimum primary-on-background contrast of 4.5:1."
  - "Catalog ids are stable strings; unknown ids fall back to the default palette."

### S2 — Define a locale catalog and a procedurally drawn flag control
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Presentation.Core/LocaleId.cs`, `game/src/PCDynasty.Presentation.Core/LocaleEntry.cs`, `game/src/PCDynasty.Presentation.Core/LocaleCatalog.cs`, `game/godot/Scripts/Shell/FlagIcon.cs`, `game/tests/PCDynasty.Presentation.Tests/LocaleCatalogTests.cs`
- **Gate**: e2e
- acceptance:
  - "A `LocaleId` is a stable string the catalog understands (`es`, `en` for now)."
  - "The catalog exposes a display name, a Godot locale code, and an owning country flag for each entry."
  - "`FlagIcon` is a Godot `Control` that draws the flag from the catalog using nested `ColorRect` columns and stripes — no external image assets enter the repository."
  - "Spanish renders the Spanish tricolour; English renders the Union Jack."

### S3 — Promote PresentationPreferences to a typed, persisted bundle
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Presentation.Core/PresentationPreferences.cs`, `game/src/PCDynasty.Presentation.Core/PresentationPreferencesStore.cs`, `game/tests/PCDynasty.Presentation.Tests/PresentationPreferencesTests.cs`
- **Gate**: e2e
- acceptance:
  - "`PresentationPreferences` exposes `Fullscreen`, `ReducedMotion`, `DenseTables`, `ThemeId`, `LocaleId` with defaults that match the shipped look."
  - "The store serialises to `presentation-preferences.json` next to the existing `content-source.json`."
  - "A missing or corrupt file returns the defaults; an unknown theme or locale id falls back to the catalog's default."
  - "Unit tests cover round-trip, corruption, and unknown ids."

### S4 — Make the design tokens swappable and apply a palette at runtime
- **Status**: done
- **DependsOn**: [S1, S3]
- **Files**: `game/godot/Theme/ManagerDesignTokens.cs`, `game/godot/Theme/ClassicChrome.cs`, `game/godot/Theme/PCDynastyTheme.tres`, `game/godot/Scripts/Shell/ThemePaletteApplier.cs`, `game/tests/PCDynasty.Presentation.Tests/ThemePaletteTests.cs`
- **Gate**: e2e
- acceptance:
  - "`ManagerDesignTokens` resolves every token from the active palette; a static `Palette` field lets the shell swap it at runtime."
  - "`ThemePaletteApplier` walks the current scene tree and rewrites the `StyleBoxFlat` colours and the `Theme` colours that depend on the palette."
  - "Swapping to any palette in the catalog re-skins the menu, the options panel and the workspace header without leaking the old palette anywhere."
  - "The ClassicDark palette is byte-identical to the palette the shell uses before the change."

### S5 — Apply the locale with Godot's TranslationServer and ship `en` + `es`
- **Status**: retired
- **Retired by**: f00046 S11 (self-contained locale JSON supersedes the .po pipeline)
- **DependsOn**: [S3]
- **Files**: `game/godot/locale/game.pot`, `game/godot/locale/en.po`, `game/godot/locale/es.po`, `game/godot/Scripts/Shell/LocaleApplier.cs`, `game/godot/project.godot`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/Components/WorkspaceHeader.cs`
- **Gate**: e2e
- acceptance:
  - "Every visible string in the main menu, the options panel, the sidebar rail, the workspace header and the status strip is wrapped in `tr()`."
  - "`LocaleApplier` registers the translation resources and calls `TranslationServer.SetLocale()` so the active scene re-renders in the chosen language without a restart."
  - "`en.po` and `es.po` carry the same keys; `es` is the source language and matches the original copy verbatim."
  - "A missing `en.po` falls back to the source strings; the game never blocks on a translation error."

### S6 — Add language and theme sections to the options panel
- **Status**: done
- **DependsOn**: [S1, SS2, S3, S4, S5]
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Shell/LocaleSection.cs`, `game/godot/Scripts/Shell/ThemeSection.cs`, `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Tests/SmokeBoot.gd`
- **Gate**: e2e
- acceptance:
  - "The options panel shows a language section (flags + names) and a theme section (palette names + a swatch) above the existing toggles."
  - "Selecting a flag or a palette re-skins the scene immediately and persists on the next launch."
  - "SmokeBoot picks a non-default theme and a non-default locale, returns to the menu and finds the same choices still selected."

## acceptance

- The options panel lets the player choose the language and the palette.
- The chosen language is one of the catalog entries (English and Spanish today,
  extensible without touching the shell).
- The chosen palette is one of the catalog entries (six today, extensible).
- Both choices survive a restart and apply to the running scene without a
  rebuild.
- The visual grammar stays original: every string, palette, icon and flag is
  produced by this project, never lifted from a commercial product.

- review-implementer: parallel agent
- review-log: reconciled by claude-opus-5 — Five slices are done and S5 was retired when `f00046` replaced the `.po` pipeline with the typed locale file; the proposal had stayed in ready/ regardless. The palette and locale sections it added do reach the options panel, but the panel itself still shows every control at once, which `x00042` S6 rebuilds as a menu of sections.
