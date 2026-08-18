using PCDynasty.Application.Matches;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Workspace.Persistence;

namespace PCDynasty.Presentation.Careers;

internal sealed record CareerRuntime(
    CareerSession Session,
    ICareerLineupService Lineups,
    CareerHistoryPersistence History);
