namespace PCDynasty.Application.Workspace;

/// <summary>
/// Raised when a command targets a club the career does not manage.
/// </summary>
public sealed class ClubAuthorityException : InvalidOperationException
{
    public ClubAuthorityException(string managedClubExternalId, string targetClubExternalId)
        : base($"This career manages '{managedClubExternalId}' and cannot command '{targetClubExternalId}'.")
    {
        ManagedClubExternalId = managedClubExternalId;
        TargetClubExternalId = targetClubExternalId;
    }

    public string ManagedClubExternalId { get; }

    public string TargetClubExternalId { get; }
}
