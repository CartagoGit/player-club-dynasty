---
id: f00046
title: "Self-contained locale JSON files with strict activation"
kind: feat
status: done
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00044
---

# f00046 — Self-contained locale JSON files with strict activation

## Goal

Replace the legacy gettext `.po` / `.pot` translation pipeline introduced by f00043 S5
with a self-contained JSON pipeline: every language is one `xx.json` file under
`res://locale/langs/`, the file is the source of truth, the player can drop a new
language in the same folder, the shell validates it strictly on activation, and
the C# code consumes the strings through typed records that are compile-time
checked.

## why

The current `.po` flow has four sharp edges that show up the moment the shell
ships:

1. **No compile-time safety.** `Tr("SQAU_EYEBROW")` compiles, ships, and the
   player sees the raw key. Refactor-safe renaming is impossible.
2. **One monolithic file per language.** Every panel, every screen, every
   button mixes in a single `es.po`. A translator needs a custom editor
   (Poedit) to navigate; an external contributor copying the Spanish file
   to add French must edit one giant file.
3. **Two storage layers.** The C# code uses `Tr()`, the gettext catalogues are
   compiled into a binary `.translation` resource by Godot, and the run-time
   activation flows through the `TranslationServer` API. Two layers, two
   sources of truth, two ways to drift.
4. **External contributions require touching the C# code.** Adding French
   means: write the `.po`, recompile, regenerate the `.translation` resource,
   wire it in C#. The contributor cannot ship a new language as a single
   drop-in file.

The proposed pipeline answers all four: JSON is the single source of truth,
typed records give compile-time safety, one file per language is the
contribution unit, and the loader reads whatever sits in `res://locale/langs/`
without recompiling the C# code.

## non-goals

- Bundling more than one language in the same `xx.json` file.
- Migrating the runtime catalog to a separate "manifest.json" — the lang id
  lives inside each file, the loader indexes by filename.
- Adding a translation toolchain (Crowdin, Transifex, Poedit bridges). External
  contributors use any text editor; CI validates the shape.
- Re-skinning the runtime Godot translation server. The shell bypasses it
  entirely after this proposal lands.
- Keeping the `.po` files as a parallel source. The f00046 cleanup step
  deletes them; nothing in the codebase reads them after S11.

## file format

### Location

```
res://locale/langs/
  es.json     ← compiled in by the generator
  en.json     ← compiled in by the generator
  fr.json     ← added by a third party
  it.json     ← added by a third party
  ...
```

All files live in the same folder. No manifest, no registry. The loader
walks the directory and indexes each file by its filename stem.

### Schema (the canonical example, Spanish)

```json
{
  "lang": "es",
  "icon": "flags/es.svg",
  "name": "Español",

  "menu": {
    "title": "TU CARRERA EMPIEZA AQUÍ",
    "eyebrow": "PC DYNASTY · DIRECCIÓN DEPORTIVA"
  },
  "options": {
    "title": "OPCIONES DE PRESENTACIÓN",
    "fullscreen": "Pantalla completa",
    "reducedMotion": "Transiciones reducidas y navegación inmediata",
    "denseTables": "Tablas compactas de alta densidad"
  },
  "squad": {
    "eyebrow": "GESTIÓN DE PLANTILLA",
    "title": "PLANTILLA",
    "summaryOwned": "Tu club · {0} · {1} jugadores en la plantilla activa",
    "summaryBrowse": "Explorando · {0} · {1} jugadores en esta plantilla",
    "actionOpenMarket": "ABRIR MERCADO",
    "empty": "Esta plantilla aún no tiene jugadores.",
    "colPlayer": "JUGADOR",
    "colNation": "NAC.",
    "colAge": "EDAD",
    "colTec": "TEC",
    "colFor": "FOR",
    "colFis": "FIS",
    "colMen": "MEN",
    "colVal": "MED",
    "colAction": "ACCIÓN",
    "rowInspect": "INSPECCIONAR"
  },
  "market": { "...": "..." },
  "history": { "...": "..." },
  "competitions": { "...": "..." },
  "world": { "...": "..." },
  "picker": { "...": "..." }
}
```

Three metadata keys live at the root: `lang` (id único), `icon` (path to a
`.svg` or `.png` in `res://locale/langs/flags/`), `name` (cómo se llama el
idioma a sí mismo, en su propio idioma). Every other top-level key is a
typed section that maps to a sub-record in C# — nested objects are allowed
without limit, so a section can be `menu → title`, `menu → header →
eyebrow`, `menu → header → actions → newCareer`, etc.

## C# model

```csharp
namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// A complete locale file. The shape is enforced by the C# record: any
/// missing or extra key is caught at deserialisation time, not at runtime
/// in the player's session.
/// </summary>
public sealed record LocaleFile(
    string Lang,
    string Icon,
    string Name,
    MenuSection Menu,
    OptionsSection Options,
    SquadSection Squad,
    MarketSection Market,
    HistorySection History,
    CompetitionsSection Competitions,
    WorldSection World,
    PickerSection Picker);

public sealed record MenuSection(string Title, string Eyebrow);
public sealed record OptionsSection(
    string Title,
    string Fullscreen,
    string ReducedMotion,
    string DenseTables);
public sealed record SquadSection(
    string Eyebrow,
    string Title,
    string SummaryOwned,
    string SummaryBrowse,
    string ActionOpenMarket,
    string Empty,
    string ColPlayer,
    string ColNation,
    string ColAge,
    string ColTec,
    string ColFor,
    string ColFis,
    string ColMen,
    string ColVal,
    string ColAction,
    string RowInspect);
// ... one per section; nested sub-records allowed.
```

Renaming a record property is a compile-time error until the JSON catches
up. Adding a property regenerates the JSON with an empty value; deleting
one deletes the JSON key. The C# record is the source of truth.

## activation algorithm

The shell runs this algorithm exactly once at boot, before any screen is
built:

```text
prefs = read presentation-preferences.json
if prefs.activeLocaleId exists:
  result = loadLocaleStrict(prefs.activeLocaleId)
  if result.ok: apply result.file as active locale
  else:
    en = loadLocaleStrict("en")
    if en.ok:
      show error panel A (recoverable, copy in English)
      apply en as active locale
    else:
      show error panel B (fatal, copy in English)
      quit
else:
  so = read OS locale (Godot.OS.GetLocale())
  result = loadLocaleStrict(so.languageCode)
  if result.ok:
    apply result.file as active locale
    write prefs.activeLocaleId = result.file.Lang
  else:
    en = loadLocaleStrict("en")
    if en.ok:
      show error panel A
      apply en as active locale
      write prefs.activeLocaleId = "en"
    else:
      show error panel B
      quit
```

`loadLocaleStrict(id)` is the only load path that counts as a true load.
It reads the JSON, deserializes it, validates every required key against
the typed `LocaleFile` record, and returns either `LocaleFile` or
`LocaleLoadResult { ok=false, missingKeys=[...], extraKeys=[...],
invalidFields=[...] }`. It never silently falls back to anything.

The OS locale is consulted **only** when `prefs.activeLocaleId` is missing.
After the first successful activation, the player's choice wins every
subsequent boot.

## locale picker (options panel)

When the player opens the language picker:

1. The panel walks `res://locale/langs/` and reads **only** the three
   metadata keys (`lang`, `icon`, `name`) of every file. The rest of the
   file is not deserialized.
2. Files whose metadata cannot be parsed (no `lang`, no `icon`, or `icon`
   path does not resolve) are skipped with a warning in the log; they
   never appear in the picker.
3. Files with a duplicate `lang` are deduplicated: the first one wins
   (built-in before external), the others are skipped with a warning
   that names the file and the reason ("duplicate id 'es': es_external.json
   ignored, es.json wins").
4. The visible list is sorted by `lang` (the id), not by `name`. Spanish
   always sits between English and French no matter what `name` says.

When the player taps a flag in the picker:

1. Show a "Loading language…" spinner (per-screen overlay).
2. Call `loadLocaleStrict(selectedLang)` on the candidate.
3. If the load is OK: apply the locale to the running scene, close the
   spinner, mark the choice as the "active draft" but do not persist yet.
4. If the load fails: close the spinner, show error panel C (recoverable,
   non-fatal), no state change.
5. When the player taps "Guardar y cerrar" (Save and close): persist the
   active draft to `prefs.activeLocaleId` and close.
6. When the player taps "Restaurar" (Revert): discard the active draft,
   return to the locale the player had when the panel opened, close.

The picker does not validate the JSON until the player taps an entry. A
corrupted third-party language is invisible in the list (skipped at
metadata-read time) and non-selectable.

## error panels

The shell has three error panels, all rendered in English because they
describe a loading failure and the player is reading them precisely
because the active locale is not trustworthy.

### A. Recoverable, fallback to English

```
Translation error
The language file 'fr.json' is invalid and could not be loaded.
Missing keys:
  • market.col.price
  • squad.col.player
Extra keys (ignored):
  • market.unlock_secret
The game will use English as the active language.
Path: res://locale/langs/fr.json
[ Use English ]
```

### B. Fatal, English itself is broken

```
Fatal translation error
Both the active language ('fr') and the fallback ('en') failed to load.
The game cannot continue without a working English file.
en.json issues:
  • menu.title is missing
  • options.col.fullscreen has the wrong type
Path: res://locale/langs/en.json
[ Quit ]
```

### C. Recoverable, picker rejected the selected language

```
Translation error
The language file 'fr.json' is invalid.
Missing keys:
  • market.col.price
This language cannot be selected. Try another or fix the file.
Path: res://locale/langs/fr.json
[ OK ]
```

All three are **scrollable** when the list of missing or extra keys is
long: a `ScrollContainer` wraps the list and the panel itself has a
fixed maximum height so it never overflows the viewport, no matter how
many keys are missing.

## Slices

- global_gate: e2e

### S1 — Typed LocaleFile and sub-records
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/Localization/LocaleFile.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/MenuSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/OptionsSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/SquadSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/MarketSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/HistorySection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/CompetitionsSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/WorldSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/Sections/PickerSection.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleFileTests.cs`
- **Gate**: e2e
- acceptance:
  - "`LocaleFile` is a record with the root metadata fields and one sub-record
    per section."
  - "The records are immutable (init-only) and serialise cleanly with
    `System.Text.Json` under camelCase property naming."
  - "A test deserialises a hand-written JSON and asserts every property is
    populated."

### S2 — Strict locale loader
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Presentation.Core/Localization/LocaleLoader.cs`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleLoadResult.cs`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleLoadResult.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleLoaderTests.cs`
- **Gate**: e2e
- acceptance:
  - "`LocaleLoader.Load(id)` returns `LocaleLoadResult.Ok(file)` when the
    file exists, the `lang` matches the id, the icon resolves, and every
    required key is present."
  - "`LocaleLoader.Load(id)` returns `LocaleLoadResult.Invalid(...)` with
    the concrete list of missing keys, extra keys and invalid fields when
    anything is wrong. It never throws past the result type."
  - "`LocalePeek(id)` returns just the three root metadata keys, parsing
    only the prefix. This is what the options panel uses to enumerate
    languages without loading the full file."
  - "Tests cover: valid file, missing `lang`, mismatched `lang`/id,
    missing icon, missing required section, extra unknown keys."

### S3 — Locale registry with collision detection
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Presentation.Core/Localization/LocaleRegistry.cs`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleMetadata.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleRegistryTests.cs`
- **Gate**: e2e
- acceptance:
  - "`LocaleRegistry.Scan(folder)` walks every `xx.json` in the folder and
    returns one `LocaleMetadata` per file, deduped by `lang`."
  - "Built-in files (the ones the generator writes) are scanned first; an
    external `fr.json` whose `lang` collides with a built-in is skipped
    with a `GD.PushWarning` (or `ILogger` warning under the test runner)."
  - "Files with a malformed root (no `lang`, no `icon`, missing icon path)
    are skipped with a warning that names the file."
  - "Tests assert: one file → one entry, two files with the same `lang` →
    one entry + one warning, no files → empty list."

### S4 — Activation service
- **Status**: done
- **DependsOn**: [S1, S2, S3]
- **Files**: `game/src/PCDynasty.Presentation.Core/Localization/LocaleActivationService.cs`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleActivationOutcome.cs`, `game/src/PCDynasty.Presentation.Core/Localization/SystemLocaleProbe.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleActivationServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "`LocaleActivationService.Activate(prefs, folder)` runs the activation
    algorithm above and returns an `LocaleActivationOutcome` with one of
    three states: `Activated(LocaleFile)`, `Recovered(LocaleFile, reason)`,
    `Fatal(reason)`. It never throws."
  - "`SystemLocaleProbe.CurrentLanguageCode()` returns the OS locale
    (e.g. `es`, `en`, `fr`) without throwing when the OS call fails."
  - "Tests cover every branch: prefs with valid id, prefs with broken id
    (English recovers), no prefs with matching OS locale, no prefs with
    non-matching OS locale (English recovers), English itself broken
    (fatal)."

### S5 — Translation error panel
- **Status**: done
- **DependsOn**: [S1, S2, S4]
- **Files**: `game/godot/Scripts/Shell/ApplicationErrorPanel.cs`, `game/godot/Scripts/Shell/ApplicationErrorPanel.cs.uid`, `game/tests/PCDynasty.Presentation.Tests/Localization/TranslationErrorPanelShapeTests.cs`
- **Gate**: e2e
- acceptance:
  - "`TranslationErrorPanel.Build(outcome)` returns the right panel shape
    for the three outcomes (A, B, C)."
  - "The missing/extra/invalid key lists live inside a `ScrollContainer`
    with a fixed maximum height so a file with hundreds of missing keys
    does not overflow the viewport."
  - "The copy is in English regardless of the active locale (because the
    panel text is hardcoded English inside `errors.json`, a file that is
    not localised and not validated as a locale)."
  - "Tests assert the panel respects the maximum height and that the
    scroll container is present in the tree."

### S6 — Wire activation into the bootstrap
- **Status**: done
- **DependsOn**: [S4, S5]
- **Files**: `game/godot/Scripts/PresentationBootstrap.cs`
- **Gate**: e2e
- acceptance:
  - "`PresentationBootstrap._Ready` runs `LocaleActivationService` before
    building any screen."
  - "On `Activated` or `Recovered` the bootstrap continues normally; the
    `MainMenuController` reads the active locale through the service."
  - "On `Fatal` the bootstrap shows the panel B and quits on
    `[ Quit ]`; the menu screen never builds."
  - "On `Recovered` the bootstrap shows panel A and continues with English
    when the player taps `[ Use English ]`."

### S7 — Language picker: metadata-only scan + strict load on tap
- **Status**: done
- **DependsOn**: [S3, S4]
- **Files**: `game/godot/Scripts/Shell/LocaleSection.cs`, `game/godot/Scripts/Shell/LocaleSpinner.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleActivationServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "`LocaleSection` reads the registry on `_Ready` and renders one button
    per `LocaleMetadata`, sorted by `lang`."
  - "Tapping a button shows `LocaleSpinner` (`Loading language…`), then
    calls `LocaleActivationService.TryActivate(lang)`; OK applies the
    locale, failure shows panel C and keeps the previous active locale."
  - "The flag icon comes from the metadata's icon path; if the path is
    missing, the existing procedural `FlagIcon` is used as a fallback."

### S8 — Options panel: Save and close / Revert
- **Status**: done
- **DependsOn**: [S7]
- **Files**: `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scenes/Main.tscn`, `game/src/PCDynasty.Presentation.Core/OptionsPanelState.cs`, `game/tests/PCDynasty.Presentation.Tests/Localization/OptionsPanelStateTests.cs`
- **Gate**: e2e
- acceptance:
  - "The options panel snapshot is taken on `OpenOptions`; the
    `MainMenuController` rolls back to the snapshot on `[Restaurar]` and
    persists the working state on `[Guardar y cerrar]`."
  - "Language, theme, fullscreen, reducedMotion and denseTables all share
    the same draft/persist model."
  - "Tests cover: open options, change theme + language, tap Revert →
    everything goes back; tap Save → preferences.json is updated."

### S9 — Compile-time JSON generator
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `tools/scripts/generate-locale.script.ts`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleFileGenerator.cs`, `tools/scripts/__tests__/generate-locale.spec.ts`
- **Gate**: e2e
- acceptance:
  - "`bun run tools/scripts/generate-locale.script.ts` serialises every
    `LocaleFile` record to "res://locale/langs/{es,en}.json"."
  - "The generator refuses to run if any record is missing a required
    key (it has the same strict validation as the loader)."
  - "CI runs the generator in dry-run mode and fails if a checked-in
    `xx.json` would drift from the record."
  - "Tests cover: regenerate twice → identical output; tweak a record →
    the next regeneration diffs only the affected key."

### S10 — E2E locale tests
- **Status**: done
- **DependsOn**: [S1, S2, S3, S4, S5, S6, S7, S8, S9]
- **Files**: `game/tests/PCDynasty.Presentation.Tests/Localization/LocaleE2ETests.cs`
- **Gate**: e2e
- acceptance:
  - "Boot the registry with a real folder of `xx.json` files; the
    activation service returns `Activated` for the valid locale, `Fatal`
    when the saved locale is broken and the English fallback is also
    broken, `Recovered` in every other case."
  - "The picker rejects a tampered file by surfacing panel C, not panel A
    or B."
  - "Generating twice with the same records produces byte-identical JSON."

### S11 — Drop the legacy .po pipeline
- **Status**: done
- **DependsOn**: [S9, S10]
- **Files**: (retired with the .po pipeline in S11) (delete) (delete) (delete), `game/godot/Scripts/Shell/LocaleSession.cs` (delete), `game/godot/Scripts/Shell/ThemePaletteApplier.cs` (drop the `LocaleApplier.RegisterTranslations` call), `game/godot/Scripts/PresentationBootstrap.cs` (drop the `res://locale/` legacy path), `game/godot/Scripts/Shell/MainMenuController.cs` (drop the `Tr()`-based `LocaleSection` constructor), `docs/delendai/proposals/ready/f00043-extend-options-panel-with-language-and-theme-preferences.md` (mark S5 done, retire the `.po` slice)
- **Gate**: e2e
- acceptance:
  - "No `.po` or `.pot` file remains in the repository (`git ls-files |
    grep '\\.po\\?'` returns empty)."
  - "No `LocaleApplier` file remains."
  - "The `Tr()` keyword still exists in the codebase only inside the
    `errors.json` reader and the smoke-boot (where it is intentional,
    because the smoke-boot talks to Godot's translation server directly)."
  - "The new JSON pipeline is the only translation source the game
    reads."

## acceptance

- Every visible string in the shell comes from "res://locale/langs/&lt;active&gt;.json".
- The C# record tree is the single source of truth: the JSON is generated
  from it.
- Adding a new language is one file: drop `fr.json` into
  `res://locale/langs/`, restart, the language appears in the picker.
- A broken `xx.json` never silently works: the player sees a scrollable
  error panel in English with the concrete list of missing keys, the file
  path, and the fallback decision.
- The activation service never throws; every failure mode is encoded as
  a value (`Activated`, `Recovered`, `Fatal`).
- The options panel offers `Revert` and `Save and close`; no destructive
  third button.
- No `.po` or `.pot` file remains in the repository.

- review-implementer: parallel agent
- review-log: corrected by claude-opus-5 — The proposal closed with all eleven slices still marked ready and eight declared files that do not exist. The declarations now name the files that shipped (OptionsPanelState moved to Presentation.Core, TranslationErrorPanel shipped as ApplicationErrorPanel plus TranslationErrorPanelShape, the E2E and panel tests carry their real names) and the `.po` artefacts are recorded as retired by S11 rather than missing. The runtime gap the proposal left behind — screens asking Godot for keys nobody registers, so the game rendered MARKET_NOTICE_ACCEPTED on screen — is fixed under `x00042` S5.
