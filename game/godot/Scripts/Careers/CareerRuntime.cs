using PCDynasty.Application.Finances;
using PCDynasty.Application.Matches;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Workspace.Persistence;

namespace PCDynasty.Presentation.Careers;

internal sealed record CareerRuntime(
    CareerSession Session,
    ICareerLineupService Lineups,
    CareerHistoryPersistence History,
    ICareerFinanceService Finances,
    ICareerTransferMarketService TransferMarket,
    ICareerSquadProjection Squad,
    CareerCalendar Calendar);
