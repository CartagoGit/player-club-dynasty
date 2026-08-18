---
id: f00034
title: "Integrate FootballPack lineups and competition formats"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: x00041, f00031, f00033
---

# f00034 — Integrate FootballPack, lineups and competition formats

## Goal

Make the declarative football pack govern the match: remove the duplicated rules from the
application layer, feed the contest with the real squad, eleven starters, bench,
positions, tactics, form, physical, mental, chemistry and motivation, and let declarative
league and cup formats drive calendar, standings, promotion, relegation, prizes and
sporting sanctions.

## why

The application layer runs a second implementation of football: `MatchSimulator` hard-codes
ninety minutes, goals and a football ruleset while building attributes from prestige,
fanbase and squad size rather than from footballers, and `SeasonSimulator` hard-codes a
round robin with three, one and zero points, treating a cup as a league. Meanwhile
`FootballContestPack` and `FootballScoringRules` exist and are exercised only by tests.
The event classification is also unreachable: with a maximum magnitude of eight, the
thresholds at forty, sixty and eighty make card, shot and save impossible. Saved lineups
are free text with a single participant and never reach the contest, so tactics cannot
affect anything.

## non-goals

- Adding an animated match view; the visible match stays a report and a timeline.
- Owning the market or the economy; `f00035` does.
- Redesigning navigation; `f00040` does.

## Slices

- global_gate: e2e

### S1 — Let the pack own match policy and delete the duplicate rules
- **Status**: ready
- **Files**: `game/src/PCDynasty.Application/Matches/MatchSimulator.cs`, `game/src/PCDynasty.Football/Simulation/FootballContestPack.cs`, `game/src/PCDynasty.Application/Competitions/SeasonSimulator.cs`, `game/tests/PCDynasty.Application.Tests/Matches/MatchPolicyTests.cs`
- **Gate**: unit
- acceptance:
  - "Duration, event vocabulary and points come from the pack; the application layer holds no football constants."
  - "Every event type is reachable in a deterministic fixture, with thresholds derived from the real magnitude range."
  - "The duplicate football rules are deleted in this slice."

### S2 — Model the real lineup, bench, positions and tactics
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Contracts/Commands/SubmitLineupCommand.cs`, `game/src/PCDynasty.Application/Matches/LineupValidator.cs`, `game/godot/Scripts/Workspace/Screens/TacticsScreen.cs`, `game/tests/PCDynasty.Application.Tests/Matches/LineupValidatorTests.cs`
- **Gate**: unit
- acceptance:
  - "A lineup with other than eleven starters or an invalid position is rejected with a diagnostic."
  - "Two valid tactics change the effective inputs of the contest without breaking replay."
  - "The submitted lineup, bench and tactic reach the contest snapshot."

### S3 — Drive league and cup from declarative formats
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Competition/Stages/`, `game/src/PCDynasty.Rules/Compilation/`, `game/src/PCDynasty.Application/Competitions/CompetitionSimulationService.cs`, `game/tests/PCDynasty.Competition.Tests/DeclarativeFormatTests.cs`
- **Gate**: e2e
- acceptance:
  - "League and cup produce different calendars, different progressions and different champions."
  - "Standings, promotion, relegation, prizes and sporting sanctions come from the compiled rules."
  - "Facts name the people involved in goals, cards, injuries, substitutions and ratings."
  - "Replay and save or load preserve facts, table and champion exactly."
  - "A visible match resolves within 8 ms and a 190-match season within 50 ms."

## acceptance

- The application layer contains no football constants; the pack governs the match.
- Invalid lineups are rejected and valid tactics measurably change the contest inputs.
- League and cup produce structurally different competitions, and every event type is
  reachable.
- Timings hold: 8 ms for a visible match, 50 ms for a full season.
