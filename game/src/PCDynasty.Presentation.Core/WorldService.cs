using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Worlds;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Engine-neutral facade that exposes the imported world as immutable UI rows.
/// Loading and projection details remain in focused internal collaborators.
/// </summary>
public sealed class WorldService : IManagerWorld
{
    private readonly WorldContentCatalog _catalog;
    private WorldProjectionCache? _projections;

    /// <summary>Reads the world shipped with the game.</summary>
    public WorldService(IContentReader reader, string contentRoot)
        : this(reader, ContentSourceDescriptor.Integrated(contentRoot))
    {
    }

    /// <summary>Reads whichever world the player selected.</summary>
    public WorldService(IContentReader reader, ContentSourceDescriptor source)
    {
        _catalog = new WorldContentCatalog(reader, source);
    }

    /// <summary>The source this world was read from.</summary>
    public ContentSourceDescriptor Source => _catalog.Source;

    public Task ReloadAsync(CancellationToken cancellationToken = default) =>
        _catalog.ReloadAsync(cancellationToken);

    /// <summary>
    /// Hash of the compiled content, independent of the format it came from.
    /// A save can bind to it and refuse to open against different data.
    /// </summary>
    public string ContentHash => _catalog.GetSnapshot().ContentHash;

    public IReadOnlyList<ClubRow> ListClubs() => Projections().Clubs;

    public IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return Projections().PlayersOf(clubExternalId);
    }

    public IReadOnlyList<ManagerRow> ListManagers(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return Projections().ManagersOf(clubExternalId);
    }

    public IReadOnlyList<StaffRow> ListStaff(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return Projections().StaffOf(clubExternalId);
    }

    public IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return Projections().CompetitionsOf(clubExternalId);
    }

    public IReadOnlyList<CompetitionRow> ListAllCompetitions() => Projections().AllCompetitions;

    public ClubRow? FindClub(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        return Projections().FindClub(clubExternalId);
    }

    /// <summary>
    /// The cache for the snapshot currently loaded. A reload installs a new
    /// snapshot instance, so comparing identity is the whole invalidation
    /// rule: the cache for the previous world simply stops being reachable.
    /// </summary>
    private WorldProjectionCache Projections()
    {
        WorldSnapshot snapshot = _catalog.GetSnapshot();
        WorldProjectionCache? cache = _projections;
        if (cache is null || !ReferenceEquals(cache.Snapshot, snapshot))
        {
            cache = new WorldProjectionCache(snapshot);
            _projections = cache;
        }

        return cache;
    }
}
