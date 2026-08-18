namespace PCDynasty.Application.Workspace;

/// <summary>
/// One player of a squad. Only people whose occupation is "player" are
/// projected here, so a manager can never be fielded, valued or sold.
/// </summary>
public sealed record PlayerRow(
    string ExternalId,
    string DisplayName,
    int BirthYear,
    DateOnly BirthDate,
    string CountryShortName,
    int Technique,
    int Form,
    int Physical,
    int Mental,
    int Overall);
