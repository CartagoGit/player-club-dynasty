---
id: x00040
title: "Guarantee a positive deterministic asking price for sparse world players"
kind: fix
status: done
type: proposal
track: runtime
date: 2026-08-18
---

# x00040 — Guarantee a positive deterministic asking price for sparse world players

## Goal

Keep validated public market models strict while ensuring every world player receives a positive deterministic minimum asking price, including sparse imported records.

## why

The Godot end-to-end gate exposed valid starter-world player projections whose current ratings are all zero. The transfer service calculated a zero asking price and failed before rendering Mercado.

## non-goals

- Relaxing TransferCandidate validation
- Fabricating player ratings in presentation
- Changing starter-world content

## Slices

- global_gate: e2e

### S1 — Floor sparse player valuations in the application market
- **Status**: done
- **Files**: `game/src/PCDynasty.Application/Market/CareerTransferMarketService.cs`, `game/tests/PCDynasty.Application.Tests/Market/CareerTransferMarketServiceTests.cs`
- **Gate**: e2e
- acceptance:
  - "Asking prices remain deterministic and use a named positive minimum rather than allowing zero."
  - "A player with zero projected ratings appears as a valid candidate at the minimum asking price."
  - "Existing affordability, duplicate, stale-snapshot and deterministic-order tests remain green."
  - "Application Release build and the previously failing Godot smoke path reach the market screen."
- review-state: done
- review-implementer: root
- review-reviewer: review_world_fix_fresh
- review-log: requested_changes by review_tactics — AskingPrice sums ratings as int before checked long arithmetic; promote/normalize before summation and add extreme imported-rating coverage so Mercado cannot fail at this edge.
- review-log: approved by review_world_fix_fresh — Fresh approval: imported ratings normalized before strict candidate construction, valuation promoted to long before sum, zero and int.Max cases deterministic without overflow; Release application/Godot and isolated smoke green.
## acceptance

- Asking prices remain deterministic and use a named positive minimum rather than allowing zero.
- A player with zero projected ratings appears as a valid candidate at the minimum asking price.
- Existing affordability, duplicate, stale-snapshot and deterministic-order tests remain green.
- Application Release build and the previously failing Godot smoke path reach the market screen.
