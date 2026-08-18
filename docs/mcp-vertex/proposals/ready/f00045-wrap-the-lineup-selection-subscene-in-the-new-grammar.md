---
id: f00045
title: "Wrap the LineupSelection subscene in the new grammar"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00044
---

# f00045 — Wrap the LineupSelection subscene in the new grammar

## Goal

Extend the f00044 grammar (HeaderPanel + ListSection + ZebraRow) to the
`LineupSelection.tscn` subscene that `TacticsScreen` mounts. The subscene is the
last workspace surface that does not follow the title-plate + bordered-panel
shape, and the player sees it on every visit to the Tactics section.

## why

f00044 S6 deliberately left the LineupSelection subscene out of scope because
it has its own private layout (`CandidateRows`, `Submit`, `Status`, `Title`).
The subscene still works, but its title is a single `Label` and its candidate
grid is a `VBoxContainer` of `HBoxContainer` rows. Without the grammar wrapper
the player sees a discontinuity when they navigate from Squad (which is now
panel-wrapped) to Tactics.

## non-goals

- Rewriting the candidate row visuals. The grid stays as it is; the slice
  adds a HeaderPanel above it and a bordered plate around it.
- Changing the `Bind` contract. The controller still receives the same
  `LineupSelectionView` and the same `sendCommand` callback.
- Touching the smoke-boot. The smoke-boot already exercises the
  `LineupSelection` path; the slice is transparent to it.

## Slices

- global_gate: e2e

### S1 — Add a HeaderPanel and a bordered plate to LineupSelection
- **Status**: ready
- **Files**: `game/godot/Scripts/Matches/LineupSelectionController.cs`, `game/godot/Scenes/Matches/LineupSelection.tscn`, `game/godot/locale/{en,es}.po`, `game/godot/locale/game.pot`
- **Gate**: e2e
- acceptance:
  - "The subscene has a `HeaderPanel` at the top carrying the club name (title)
    and the competition name (summary) of the `LineupSelectionView`."
  - "The candidate grid, the option field and the submit button sit inside a
    `ListSection`-shaped bordered plate so the visual rhythm matches Squad and
    Market."
  - "Every new caption flows through `tr()` and the `.po` files carry the
    new keys."
  - "The smoke-boot's `LineupSelection` path still resolves and the
    `Selección enviada.` status assertion still holds."

## acceptance

- The `LineupSelection` subscene opens with the same title-plate chrome the
  other workspace screens use.
- Every visible string flows through `tr()`.
- The f00044 grammar is now uniform across the entire workspace.
