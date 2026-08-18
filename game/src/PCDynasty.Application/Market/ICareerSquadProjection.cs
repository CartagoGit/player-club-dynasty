using PCDynasty.Application.Workspace;

namespace PCDynasty.Application.Market;

public interface ICareerSquadProjection
{
    IReadOnlyList<PlayerRow> ListManagedSquad(ManagedClubContext managed);
}
