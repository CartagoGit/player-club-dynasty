---
id: f00037
title: "Connect training chemistry and the person lifecycle"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00031, f00032, f00033, f00034, f00036
---

# f00037 — Connect training, chemistry and the person lifecycle

## Goal

Turn training, development, wear, injury, recovery, chemistry, motivation, life events,
retirement, youth arrival and positional conversion into deterministic calendar units with
persisted facts and measurable effects on the match.

## why

`PCDynasty.Careers` and the training, chemistry and motivation types are green in tests and
have no use case, no persistence in a career, no screen and no effect on a contest, because
until now there was no calendar to trigger them. With the orchestrator in place they become
schedulable, which is what turns a season into a story instead of a table of results.

## non-goals

- Revealing a fixed public potential value.
- Owning academy intake and staff progression; `f00038` does.
- Adding medical minigames.

## Slices

- global_gate: e2e

### S1 — Schedule development, wear, injury and recovery
- **Status**: ready
- **Files**: `game/src/PCDynasty.Application/Careers/DevelopmentSchedule.cs`, `game/src/PCDynasty.Careers/Development/`, `game/src/PCDynasty.Careers/Injuries/`, `game/tests/PCDynasty.Careers.Tests/DevelopmentScheduleTests.cs`, `game/benchmarks/budgets.json`
- **Gate**: perf
- acceptance:
  - "Development, wear, injury and recovery run as calendar units producing persisted facts."
  - "160,000 development steps complete within 1,000 ms and reproduce in serial and parallel."
  - "Fixtures cover injury, recovery, life outside football, retirement with replacement and valid or invalid positional conversion."

### S2 — Make a training plan change effective attributes and risk
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Simulation/Training/`, `game/src/PCDynasty.Application/Training/TrainingPlanService.cs`, `game/godot/Scripts/Workspace/Screens/TrainingScreen.cs`, `game/tests/PCDynasty.Application.Tests/Training/TrainingPlanServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "A weekly plan measurably changes effective attributes and injury risk."
  - "Chemistry and motivation reach the contest snapshot and change its inputs."
  - "SmokeBoot advances a week and shows cause, change and history without revealing hidden information."

## acceptance

- 160,000 development steps stay within 1,000 ms and reproduce in serial and parallel.
- A training plan produces measurable attribute and risk changes that survive save and load.
- The advancement view explains cause and effect without leaking hidden values.
