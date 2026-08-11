---
id: f00020
title: "Implement club economy market and negotiations"
kind: feat
status: ready
type: proposal
track: economy
date: 2026-08-11
---

# f00020 — Implement club economy market and negotiations

## Goal

Implement dynamic club finance, distinct valuations, contracts, market rounds and negotiation outcomes using explicit facts and budgets.

## why

A credible manager ecosystem needs money conservation, imperfect valuations and clubs that can rise or decline.

## non-goals

- Real-world financial data, online trading or daily simulated phone calls.

## Slices

- global_gate: e2e

### S1 — Define financial ledgers and annual club state
- **Status**: done
- **Files**: `game/src/PCDynasty.Economy/PCDynasty.Economy.csproj`, `game/src/PCDynasty.Economy/Ledgers/ClubLedger.cs`, `game/tests/PCDynasty.Economy.Tests/ClubLedgerTests.cs`
- **Gate**: type
- acceptance:
  - "Money is conserved and revenues, costs, debt and budgets have explicit ownership."

### S2 — Implement valuations and contracts
- **Status**: done
- **DependsOn**: [S1]
- **Files**: `game/src/PCDynasty.Market/PCDynasty.Market.csproj`, `game/src/PCDynasty.Market/Valuation/PlayerValuation.cs`, `game/src/PCDynasty.Market/Contracts/ContractOffer.cs`
- **Gate**: type
- acceptance:
  - "Market, internal, asking, transfer and contract values remain distinct."

### S3 — Implement negotiation and market rounds
- **Status**: done
- **DependsOn**: [S1, S2]
- **Files**: `game/src/PCDynasty.Market/Negotiation/NegotiationEngine.cs`, `game/src/PCDynasty.Market/Rounds/MarketRoundResolver.cs`, `game/tests/PCDynasty.Market.Tests/MarketRoundResolverTests.cs`
- **Gate**: e2e
- acceptance:
  - "Markets resolve structurally with budgets, needs and deterministic outcomes."

## acceptance

- Money is conserved and revenues, costs, debt and budgets have explicit ownership.
- Market, internal, asking, transfer and contract values remain distinct.
- Markets resolve structurally with budgets, needs and deterministic outcomes.
