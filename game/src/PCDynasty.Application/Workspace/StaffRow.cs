namespace PCDynasty.Application.Workspace;

/// <summary>
/// One employee who is neither a player nor a manager. The role is the
/// occupation declared by the content pack, so new roles need no code change.
/// </summary>
public sealed record StaffRow(
    string ExternalId,
    string DisplayName,
    string Role,
    int BirthYear,
    string CountryShortName);
