---
id: f00035
title: "Connect authoritative economy contracts and market"
kind: feat
status: ready
type: proposal
track: product
date: 2026-08-18
plan: q00001
audit-source: a00001
depends-on: f00031, f00032, f00033, f00034
---

# f00035 — Connect authoritative economy, contracts and market

## Goal

Make one transfer fact debit the buyer, credit the seller, move ownership and rosters and
create a contract, and extend the economy with offers, counter-offers, wages, clauses,
sales, loans, macro replacement, inflation, prizes, maintenance, debt and financial
sanctions, all consumed through versioned read models and asynchronous commands.

## why

The visible market is a purchase that debits the buyer only: it never credits the selling
club and never changes world ownership, so the signed player keeps appearing in his source
club. The validated `PCDynasty.Market` and `PCDynasty.Economy` engines take no part in the
game, and `CareerTransferMarketService` mixes candidate search, squad projection,
valuation, negotiation, timestamps, debits and persistence coordination in one class.
Finances are a starting balance and a purchase debit; they do not evolve with sporting
activity, wages, prizes or maintenance.

## non-goals

- Agents, media narrative or contract negotiation minigames.
- Owning the manager's own contract; `f00039` does.
- Facilities investment; `f00038` does.

## Slices

- global_gate: e2e

### S1 — Make the transfer one conserving fact
- **Status**: ready
- **Files**: `game/src/PCDynasty.Contracts/Careers/CareerTransferRecord.cs`, `game/src/PCDynasty.Application/Market/TransferSettlement.cs`, `game/src/PCDynasty.Market/Negotiation/`, `game/tests/PCDynasty.Application.Tests/Market/TransferConservationTests.cs`
- **Gate**: unit
- acceptance:
  - "A settled transfer debits the buyer, credits the seller, moves the player between rosters and creates a contract in one transaction."
  - "Money and ownership conservation hold before and after, and a failure rolls back everything."
  - "Both clubs project the same transfer identically."

### S2 — Extend negotiation, contracts and the club ledger
- **Status**: ready
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Market/Contracts/`, `game/src/PCDynasty.Economy/Ledgers/`, `game/src/PCDynasty.Application/Finances/`, `game/tests/PCDynasty.Economy.Tests/SeasonReconciliationTests.cs`
- **Gate**: unit
- acceptance:
  - "Offers, counter-offers, wages, clauses, sales and loans are modelled and persisted."
  - "Wages, prizes, maintenance, inflation, debt and financial sanctions move the ledger over time."
  - "The annual close reconciles the ledger to zero by double entry."

### S3 — Operate the market from read models and async commands
- **Status**: ready
- **DependsOn**: [S2]
- **Files**: `game/godot/Scripts/Workspace/Screens/MarketScreen.cs`, `game/godot/Scripts/Workspace/Screens/FinancesScreen.cs`, `game/src/PCDynasty.Application/Market/ICareerTransferMarketService.cs`, `game/benchmarks/budgets.json`
- **Gate**: e2e
- acceptance:
  - "Screens consume versioned read models and issue asynchronous commands; no query or transaction runs inside a frame."
  - "A macro market over 6,400 clubs holds a median at or below 250 ms while no interaction blocks more than 8 ms."
  - "SmokeBoot buys, sells, continues the career and verifies both rosters and both balances."

## acceptance

- One transfer fact conserves money and ownership, with identical projections from both
  clubs and full rollback on failure.
- The annual close reconciles the ledger to zero.
- The macro market over 6,400 clubs stays at or below a 250 ms median with no interaction
  blocking beyond 8 ms.
