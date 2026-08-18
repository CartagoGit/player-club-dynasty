namespace PCDynasty.Application.Workspace;

/// <summary>One club as the interface reads it.</summary>
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
