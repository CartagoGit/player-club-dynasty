---
id: f00051
title: "Turn the main menu into a board of modes, and ship the responsibility ladder"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-19
plan: q00001
depends-on: f00048
---

# f00051 — Turn the main menu into a board of modes, and ship the responsibility ladder

## Goal

Replace a menu of six session commands with a **board of destinations**, and
give the career the reference's best idea: the same competition offered at three
different levels of responsibility, so the player chooses *how much of the game
they want to play* rather than how hard it should be.

## why

The main menu today reads NUEVA CARRERA · CONTINUAR · CARGAR CARRERA · EDITOR DE
DATOS · OPCIONES · SALIR. Five of those six are session management; exactly one
of them is a mode. The player's report was blunt: *"los modos de juego son muy
muy pocos"*.

[`02-modos-de-juego.md`](../../../wiki/pcfutbol/02-modos-de-juego.md) records
what the reference actually shipped. Two findings drive this proposal.

**The disc was a suite, not a screen.** PC Fútbol 6.0's menu was a grid of nine
illustrated destinations plus a row of small icons. Part of the perceived value
came from the program being a place you could go for several different reasons.
Our menu cannot express that, because it is a column of buttons.

**The responsibility ladder is the best idea the saga had**, and the one its
imitators misread. Liga Entrenador, Liga Manager and Liga Pro-Manager are the
same league; what changes is which decisions are yours:

| Mode | The player runs | The club runs |
|---|---|---|
| Entrenador | lineup, tactics, training | transfers, finances |
| Manager | all of it, and picks the club | nothing |
| Pro-Manager | all of it, but **does not pick a club** — offers arrive by reputation | which offers arrive |

It is not a difficulty selector. A player who hates finances does not lose the
game, they lose a screen. That is worth building because it multiplies what we
already have: the workspace, the market, the finances and the lineup all exist —
a mode decides which of them are the player's.

Pro-Manager also carries a mechanic nothing else in the plan does: **reputation
that survives the career**. You come back, you are recognised, and the offers
you get are the ones you earned.

## non-goals

- ProQuinielas, Infofútbol, Los goles de la Liga, Seguimiento of a real season.
  They belonged to a product tied to a real league we deliberately do not have.
  Chapter 02 records why they mattered; the transferable principle is "a manager
  is more than its career loop", not the specific sections.
- Playing the match. The arcade, the visionado and the interactive views are
  their own body of work; this proposal decides *who decides*, not how a match
  is watched.
- The illustrated art of the reference's tiles. The board is built in our own
  grammar with our own plates; nothing is copied.
- Multi-manager sessions with passwords. Recorded in chapter 02, out of scope.

## Slices

### S1 — Model the mode as a first-class career property
- **Status**: ready
- **Files**: `game/src/PCDynasty.Contracts/Careers/CareerMode.cs`, `game/src/PCDynasty.Contracts/Careers/CareerProfile.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteCareerProfileStore.cs`, `game/tests/PCDynasty.Careers.Tests/CareerModeTests.cs`
- **Gate**: unit
- acceptance:
  - "`CareerMode` names the three rungs and declares, per rung, which workspace areas the player commands."
  - "`CareerProfile` carries the mode; a profile written before this slice loads with the mode that matches today's behaviour (Manager) instead of failing."
  - "The mode is chosen once, at career creation, and cannot be changed afterwards — a career is the record of decisions taken under one set of rules."

### S2 — Let the mode decide which areas are the player's
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/godot/Scripts/Workspace/ManagerWorkspaceController.cs`, `game/godot/Scripts/Workspace/WorkspaceSection.cs`, `game/src/PCDynasty.Application/Workspace/*`
- **Gate**: e2e
- acceptance:
  - "In Entrenador the market and finances rails are absent, not disabled-and-taunting; the areas the player does not run are not part of their desk."
  - "The decisions those areas would have made are still made — the club signs and spends on its own — and the player is told what happened, so an automated area is a source of news rather than a hole."
  - "The workspace reads the mode once at `Configure`; no screen queries the mode while rendering."
  - "The smoke opens a career in each mode and asserts the rail carries exactly the areas that mode commands."

### S3 — Build the destination board
- **Status**: ready
- **Files**: `game/godot/Scenes/Main.tscn`, `game/godot/Scripts/Shell/MainMenuController.cs`, `game/godot/Scripts/Shell/DestinationBoard.cs`, `game/godot/Theme/DestinationTile.cs`
- **Gate**: e2e
- acceptance:
  - "The menu is a grid of destination plates, each with its own caption colour by family, over a bottom rail of small commands (options, credits, exit) — the shape chapter 13 records, in our grammar."
  - "Every destination is visible at once; nothing is behind a submenu."
  - "A destination that is not built yet is present and says when it arrives, rather than being hidden — the board is the map of the game, and a map with holes is more honest than a map that lies about its size."
  - "The board survives a palette change and does not overflow at 1280×720 or at 1920×1080."

### S4 — Ship the friendly match
- **Status**: ready
- **DependsOn**: [S3]
- **Files**: `game/godot/Scripts/Shell/FriendlyMatchSetup.cs`, `game/godot/Scenes/Main.tscn`, `game/src/PCDynasty.Application/Matches/*`
- **Gate**: e2e
- acceptance:
  - "Two clubs are picked from the loaded world and a match is resolved with no career, no money and no consequence."
  - "It reuses the career's own simulation; a friendly is the same match with nothing written down."
  - "It is reachable from the board in two clicks and returns to the board."

### S5 — Ship Pro-Manager and its reputation
- **Status**: ready
- **DependsOn**: [S1, S2, S3]
- **Files**: `game/src/PCDynasty.Application/Careers/ManagerReputation.cs`, `game/src/PCDynasty.Persistence.Sqlite/SqliteManagerReputationStore.cs`, `game/godot/Scripts/Careers/OfferBoardScreen.cs`
- **Gate**: e2e
- acceptance:
  - "Starting a Pro-Manager career shows a board of offers instead of a club list; the player accepts one."
  - "Which offers arrive is a function of stored reputation, and reputation is written at the end of each season — so a second career starts where the first left the manager, not where the world did."
  - "The offer plate shows enough about each club to decide: prestige, division, squad size, money."
  - "A player with no history is offered only what the bottom of the ladder would offer."

### S6 — Make the archive a destination
- **Status**: ready
- **DependsOn**: [S3]
- **Files**: `game/godot/Scripts/Shell/ArchiveScreen.cs`, `game/godot/Scripts/Workspace/Screens/HistoryScreen.cs`
- **Gate**: e2e
- acceptance:
  - "The history and the world's competitions are reachable from the board without opening a career, because they are about the world, not about one manager."
  - "It reuses `HistoryScreen`'s table rather than growing a second one."

## acceptance

- The main menu is a board of destinations and every one of them either works or
  says when it will.
- A career is started at a chosen level of responsibility, and the workspace
  carries exactly the areas that level commands.
- A manager's reputation outlives the career it was earned in.
- The friendly match makes the simulation reachable without a career.
