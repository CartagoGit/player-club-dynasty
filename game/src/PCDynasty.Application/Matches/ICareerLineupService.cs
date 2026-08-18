using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Matches;

public interface ICareerLineupService
{
    /// <summary>Saves a plan for the managed club; any other club is rejected.</summary>
    LineupPlan Submit(ManagedClubContext managed, SubmitLineupCommand command);

    bool TryGetLatest(string clubExternalId, out LineupPlan? plan);
}
