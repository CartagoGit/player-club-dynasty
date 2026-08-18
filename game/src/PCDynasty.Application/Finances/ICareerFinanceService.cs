using PCDynasty.Application.Workspace;

namespace PCDynasty.Application.Finances;

public enum CareerFinanceMode
{
    New = 0,
    Continue = 1,
}

public interface ICareerFinanceService
{
    ClubFinanceOverview Open(ManagedClubContext managed, ClubRow club, CareerFinanceMode mode);

    ClubFinanceOverview GetOverview(ManagedClubContext managed);
}
