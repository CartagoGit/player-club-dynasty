using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Engine-neutral facade that exposes the imported world as immutable UI rows.
/// Loading and projection details remain in focused internal collaborators.
/// </summary>
public sealed class WorldService : IManagerWorld
{
    private readonly WorldContentCatalog _catalog;

    public WorldService(IContentReader reader, string contentRoot)
    {
        _catalog = new WorldContentCatalog(reader, contentRoot);
    }

    public Task ReloadAsync(CancellationToken cancellationToken = default) =>
        _catalog.ReloadAsync(cancellationToken);

    /// <summary>
    /// Hash of the compiled content, independent of the format it came from.
    /// A save can bind to it and refuse to open against different data.
    /// </summary>
    public string ContentHash => _catalog.GetSnapshot().ContentHash;

    public IReadOnlyList<ClubRow> ListClubs() =>
        WorldClubProjection.ListClubs(_catalog.GetSnapshot());

    public IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldPeopleProjection.ListPlayers(_catalog.GetSnapshot(), clubExternalId);
    }

    public IReadOnlyList<ManagerRow> ListManagers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldPeopleProjection.ListManagers(_catalog.GetSnapshot(), clubExternalId);
    }

    public IReadOnlyList<StaffRow> ListStaff(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldPeopleProjection.ListStaff(_catalog.GetSnapshot(), clubExternalId);
    }

    public IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldCompetitionProjection.ListForClub(_catalog.GetSnapshot(), clubExternalId);
    }

    public IReadOnlyList<CompetitionRow> ListAllCompetitions() =>
        WorldCompetitionProjection.ListAll(_catalog.GetSnapshot());

    public ClubRow? FindClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldClubProjection.FindClub(_catalog.GetSnapshot(), clubExternalId);
    }
}
