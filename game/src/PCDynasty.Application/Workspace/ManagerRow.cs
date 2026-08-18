namespace PCDynasty.Application.Workspace;

/// <summary>
/// One manager employed by a club. Managers carry reputation rather than
/// playing attributes, which is why they need their own read model.
/// </summary>
public sealed record ManagerRow(
    string ExternalId,
    string DisplayName,
    int BirthYear,
    string CountryShortName,
    int Reputation);
