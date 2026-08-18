namespace PCDynasty.Application.Workspace;

/// <summary>
/// Read-only manager world needed by presentation adapters.
/// People are exposed per occupation: a squad is never a bag of persons.
/// </summary>
public interface IManagerWorld
{
    IReadOnlyList<ClubRow> ListClubs();

    /// <summary>Players registered with the club. Managers and staff are excluded.</summary>
    IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId);

    /// <summary>People employed by the club to manage it.</summary>
    IReadOnlyList<ManagerRow> ListManagers(string clubExternalId);

    /// <summary>Every other employee of the club, each with its own role.</summary>
    IReadOnlyList<StaffRow> ListStaff(string clubExternalId);

    IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId);

    IReadOnlyList<CompetitionRow> ListAllCompetitions();

    ClubRow? FindClub(string clubExternalId);
}
