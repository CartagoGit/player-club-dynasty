namespace PCDynasty.Application.Workspace;

/// <summary>
/// The single club a career is allowed to mutate.
/// </summary>
/// <remarks>
/// Authority used to be implicit: screens passed whichever club the user was
/// looking at, so browsing a rival put its identifier on command surfaces
/// (audit a00001). Commands now demand this token, and every implementation
/// re-checks it, so a rival club cannot be fielded, financed or traded by
/// accident.
/// </remarks>
public sealed record ManagedClubContext
{
    public ManagedClubContext(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        ClubExternalId = clubExternalId;
    }

    public string ClubExternalId { get; }

    public bool Owns(string clubExternalId) =>
        string.Equals(ClubExternalId, clubExternalId, StringComparison.Ordinal);

    /// <summary>Throws when a command targets a club this career does not manage.</summary>
    public void EnsureOwns(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        if (!Owns(clubExternalId))
        {
            throw new ClubAuthorityException(ClubExternalId, clubExternalId);
        }
    }
}
