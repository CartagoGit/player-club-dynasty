---
id: f00050
title: "Make every failure arrive on screen, in one designed panel"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-19
plan: q00001
depends-on: f00048
---

# f00050 — Make every failure arrive on screen, in one designed panel

## Goal

Give the game one error surface that (a) catches **every** failure, not two of
them, (b) is drawn in the same grammar as the rest of the shell, (c) speaks the
player's language, and (d) hands over something a person can actually send us:
a written report on disk, not a clipboard string they must remember to paste.

## why

`ApplicationErrorPanel` already exists and its value model — `ApplicationError`
with severity, title, body, path, code and an item list — is sound and unit
tested. What is missing is everything around it.

**Almost nothing can reach it.** Two call sites raise an error today:
`PresentationBootstrap` when the world fails to load, and `MainMenuController`
when the language picker rejects a locale. Everything else fails silently:

- an exception thrown inside a screen's `Render` faults the Godot callback and
  prints to a console the player does not have;
- `StartAsync` catches and reports, but any `async void` or continuation after
  it does not;
- a corrupt career database, a failed settle, a missing content pack chosen from
  the data tab — all of them either throw past the UI or are swallowed by a
  `catch (InvalidOperationException)` that substitutes an em dash.

The player's report for this round was *"por si tenemos errores de
funcionamiento"* — the point is precisely the failures nobody wired up.

**It is not in the grammar.** The panel predates `ClassicChrome`; it builds its
own overlay, its own panel and its own buttons. It is now the only surface in
the game that does not use the caption bar, the plate and the command styling —
and it is the surface a player sees on their worst day.

**It is hard-coded English.** "Unexpected error", "Close", "Copy details" and
the body text are literals in `ApplicationError`. Every other string in the game
comes from `LocaleFile`.

**A clipboard is not a bug report.** There is no file to attach, nothing that
survives the process dying, and nothing that records what the player was doing
when it happened.

## non-goals

- Crash *recovery*. A fatal error still ends in "close" or "back to the main
  menu"; this proposal is about the failure being visible and reportable, not
  about resuming from it.
- Telemetry or any network upload. The report is written locally and the player
  decides what to do with it.
- Replacing `ConfirmDialog` (f00048). That answers a question the player asked;
  this reports something the player did not ask for. They share the chrome, not
  the class.
- Localising engine-level messages from Godot itself. Those go in the log
  verbatim.

## Slices

- global_gate: e2e

### S1 — Move the error copy into the locale file
- **Status**: ready
- **Files**: `game/src/PCDynasty.Presentation.Core/Localization/Sections/DiagnosticsSection.cs`, `game/src/PCDynasty.Presentation.Core/Localization/LocaleFile.cs`, `game/src/PCDynasty.Presentation.Core/Localization/ShippedLocaleFiles.cs`, `game/src/PCDynasty.Presentation.Core/Diagnostics/ApplicationError.cs`, `game/godot/locale/langs/built/*.json`
- **Gate**: unit
- acceptance:
  - "`DiagnosticsSection` carries the titles, bodies and button labels for every failure kind the factories build."
  - "`ApplicationError.FromException` and the translation factories take their copy from the section instead of literals."
  - "A locale that omits the section fails the strict loader like any other missing section, rather than falling back to English silently."
  - "`bun tools/scripts/localization/generate-locale.script.ts` is re-run and both built JSONs carry the new block."

### S2 — Redraw the panel in the shell's grammar
- **Status**: ready
- **Files**: `game/godot/Scripts/Shell/ApplicationErrorPanel.cs`, `game/godot/Theme/ClassicChrome.cs`, `game/godot/Theme/ConfirmDialog.cs`
- **Gate**: e2e
- acceptance:
  - "The panel is a floating plate with a caption bar whose colour is the severity, a body, an inset details block and a command row — the same primitives every other surface uses."
  - "Warning and Fatal are distinguishable without reading: the caption bar carries the warning and negative palette colours respectively."
  - "The details list is an `Inset` block that scrolls inside its own container and never pushes the commands off screen."
  - "The panel survives a palette change through `IPaletteAware` like every other dressed control."
  - "A screenshot route renders both severities so the panel is reviewed by looking at it."

### S3 — Catch what currently escapes
- **Files**: `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Shell/FailureSink.cs`, `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`
- **Status**: ready
- **DependsOn**: [S2]
- **Gate**: e2e
- acceptance:
  - "A single `FailureSink` owns the panel: anything anywhere in the presentation calls `Report(ApplicationError)` and the sink decides whether to show, queue or coalesce."
  - "`AppDomain.CurrentDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException` are bridged into the sink, so a faulted task is no longer a silent no-op."
  - "`ManagerWorkspaceController.RenderCurrent` wraps the per-screen render so one broken screen reports itself instead of taking the workspace down."
  - "Two failures in a row do not stack two panels; the second is added to the open one as an item."

### S4 — Write a report the player can send
- **Status**: ready
- **DependsOn**: [S3]
- **Files**: `game/src/PCDynasty.Presentation.Core/Diagnostics/FailureLog.cs`, `game/src/PCDynasty.Presentation.Core/Diagnostics/ApplicationErrorReport.cs`, `game/godot/Scripts/Shell/ApplicationErrorPanel.cs`
- **Gate**: unit
- acceptance:
  - "Every reported failure is appended to `user://logs/failures.log` with a UTC timestamp, the build version, the content hash and the full report text."
  - "The panel shows the absolute path of that file and offers 'open the folder' alongside 'copy details', so the player has something to attach."
  - "The log is capped (size or entry count) and rotates, so a repeating failure cannot fill the disk."
  - "The log path is derived from `user://`, never from a literal — a machine-specific path in a shipped file is a bug."

### S5 — Give the panel somewhere to go
- **Status**: ready
- **DependsOn**: [S3]
- **Files**: `game/godot/Scripts/Shell/ApplicationErrorPanel.cs`, `game/src/PCDynasty.Presentation.Core/Diagnostics/ApplicationError.cs`
- **Gate**: e2e
- acceptance:
  - "`ApplicationError` carries an optional recovery action beyond the primary button: retry, reload the world, or return to the main menu."
  - "A warning raised inside the workspace offers 'return to the main menu' and takes the same guarded path as leaving a career, so a failure cannot discard a session either."
  - "A fatal error offers only 'close'; nothing pretends the game can continue."

### S6 — Prove it in the smoke test
- **Status**: ready
- **DependsOn**: [S1, S2, S3, S4, S5]
- **Files**: `game/godot/Tests/SmokeBoot.gd`, `game/godot/Tests/CaptureShots.gd`, `game/tests/PCDynasty.Presentation.Tests/FailureSinkTests.cs`
- **Gate**: e2e
- acceptance:
  - "The smoke raises a synthetic failure through the sink and asserts the panel appears, carries a sentence rather than a translation key, and dismisses cleanly."
  - "It asserts the log file was written and names a path under `user://`."
  - "`CaptureShots.gd` renders a warning and a fatal panel."

## acceptance

- Any failure raised anywhere in the presentation reaches the player, in their
  language, in the game's own grammar.
- The player leaves the failure holding a file they can send, not a string they
  had to remember to copy.
- No failure can silently end a career session.
