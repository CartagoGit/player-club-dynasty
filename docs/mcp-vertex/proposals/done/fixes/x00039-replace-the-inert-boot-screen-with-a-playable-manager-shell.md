---
id: x00039
title: "Replace the inert boot screen with a playable manager shell"
kind: fix
status: done
type: proposal
track: presentation
date: 2026-08-18
---

# x00039 — Replace the inert boot screen with a playable manager shell

## Goal

Open PC Dynasty on an original classic-manager main menu, let the player start or continue a career, and enter a fully wired workspace backed by the starter world.

## why

The current scene serializes paths on PresentationBootstrap but creates MainController with empty paths, so the domain-backed UI never replaces the technical placeholder.

## non-goals

- Copy PC Fútbol branding, artwork or exact layouts.
- Move domain rules into Godot.
- Implement every manager subsystem in this proposal.

## Slices

- global_gate: e2e

### S1 — Define an engine-neutral shell navigation contract
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/Shell/ManagerShell.cs`, `game/tests/PCDynasty.Application.Tests/Shell/ManagerShellTests.cs`
- **Gate**: type
- acceptance:
  - "Shell routes and transitions are immutable, explicit and independent from Godot."
  - "Invalid transitions are rejected by tests."
- review-state: done
- review-implementer: root
- review-reviewer: fresh-delivery-verifier
- review-log: requested_changes by delivery-verifier — Shell guards exist but no test exercises InvalidRoute. Add tests invoking transitions from the wrong route and assert rejection, Failure InvalidRoute, and unchanged state.
- review-log: approved by fresh-delivery-verifier — Fresh independent re-review approved: invalid-route guards are now pinned from MainMenu and CareerSetup with Accepted false, exact InvalidRoute and identical unchanged state; valid transition coverage remains green.
### S2 — Build and wire the Godot main menu and manager workspace
- **Status**: done
- **Files**: `game/godot/Scripts/PresentationBootstrap.cs`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Shell/MainMenuController.cs.uid`, `game/godot/Scripts/Presentation/MainController.cs`, `game/godot/Scripts/Presentation/MainController.cs.uid`, `game/godot/Scenes/Main.tscn`, `game/godot/Tests/SmokeBoot.gd`, `game/godot/Tests/SmokeBoot.gd.uid`
- **Gate**: e2e
- acceptance:
  - "Startup shows a polished original main menu rather than technical placeholder text."
  - "New career exposes real fictional clubs and enters a wired manager workspace."
  - "Continue, data editor, options and exit provide real actions or explicit disabled states."
  - "A Godot smoke script proves the workspace controller receives valid paths."
- review-state: done
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: approved by delivery-verifier — Independent review approved the original classic-manager menu, fictional club setup, fully wired workspace, explicit actions/disabled states and sequential new-return-continue Godot smoke.
### S3 — Persist and restore the selected career profile
- **Status**: done
- **Files**: `game/src/PCDynasty.Presentation.Core/CareerProfileStore.cs`, `game/tests/PCDynasty.Presentation.Tests/CareerProfileStoreTests.cs`
- **Gate**: type
- acceptance:
  - "The selected club and career metadata round-trip through an external SQLite save."
  - "A missing save is represented explicitly and does not crash startup."
- review-state: done
- review-implementer: root
- review-reviewer: delivery-verifier
- review-log: approved by delivery-verifier — Independent review approved engine-neutral profile contracts and external SQLite round-trip, replacement/reopen behavior and safe missing-save startup.
## acceptance

- Shell routes and transitions are immutable, explicit and independent from Godot.
- Invalid transitions are rejected by tests.
- Startup shows a polished original main menu rather than technical placeholder text.
- New career exposes real fictional clubs and enters a wired manager workspace.
- Continue, data editor, options and exit provide real actions or explicit disabled states.
- A Godot smoke script proves the workspace controller receives valid paths.
- The selected club and career metadata round-trip through an external SQLite save.
- A missing save is represented explicitly and does not crash startup.
