namespace PCDynasty.Application.Workspace;

/// <summary>
/// Read-only manager world needed by presentation adapters.
/// </summary>
public interface IManagerWorld
{
    IReadOnlyList<ClubRow> ListClubs();

    IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId);

    IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId);

    IReadOnlyList<CompetitionRow> ListAllCompetitions();

    ClubRow? FindClub(string clubExternalId);
}

public sealed record ClubRow(
    string ExternalId,
    string DisplayName,
    string ShortName,
    string City,
    string CountryShortName,
    string FoundedYear,
    int Prestige,
    int Fanbase,
    int SquadSize);

public sealed record PlayerRow(
    string ExternalId,
    string DisplayName,
    int BirthYear,
    int Technique,
    int Form,
    int Physical,
    int Mental,
    int Overall);

public sealed record CompetitionRow(
    string ExternalId,
    string DisplayName,
    string Format,
    string Frequency,
    IReadOnlyList<string> Rivals);
