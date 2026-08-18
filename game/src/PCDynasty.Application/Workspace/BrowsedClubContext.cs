namespace PCDynasty.Application.Workspace;

/// <summary>
/// The club currently being looked at, which may be any club in the world.
/// Browsing is read-only by construction: this context reaches queries only,
/// never a command surface.
/// </summary>
public sealed record BrowsedClubContext
{
    public BrowsedClubContext(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        ClubExternalId = clubExternalId;
    }

    public string ClubExternalId { get; }

    public bool IsManaged(ManagedClubContext managed)
    {
        ArgumentNullException.ThrowIfNull(managed);
        return managed.Owns(ClubExternalId);
    }
}
