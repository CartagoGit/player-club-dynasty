using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Matches;

public interface ICareerLineupService
{
    LineupPlan Submit(SubmitLineupCommand command);

    bool TryGetLatest(string clubExternalId, out LineupPlan? plan);
}
