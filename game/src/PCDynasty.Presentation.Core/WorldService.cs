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

    public IReadOnlyList<ClubRow> ListClubs() =>
        WorldClubProjection.ListClubs(_catalog.GetSnapshot());

    public IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return WorldClubProjection.ListPlayers(_catalog.GetSnapshot(), clubExternalId);
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
