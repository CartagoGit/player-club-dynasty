using PCDynasty.Application.Workspace;

namespace PCDynasty.Application.Finances;

public enum CareerFinanceMode
{
    New = 0,
    Continue = 1,
}

public interface ICareerFinanceService
{
    ClubFinanceOverview Open(ClubRow club, CareerFinanceMode mode);

    ClubFinanceOverview GetOverview(string clubExternalId);
}
