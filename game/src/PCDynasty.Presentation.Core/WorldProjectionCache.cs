using System.Collections.Concurrent;
using PCDynasty.Application.Workspace;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Memoised projections of one <see cref="WorldSnapshot"/>.
/// </summary>
/// <remarks>
/// The snapshot was already indexed, so finding a club's people is a
/// dictionary hit. What was not cached is the *projection*: turning those
/// entities into <see cref="ClubRow"/> and <see cref="PlayerRow"/> parses
/// every attribute, allocates a row and sorts the result — on every call.
/// The market alone asked for it once per club per render, so opening it
/// re-projected the whole world.
///
/// A projection is a pure function of an immutable snapshot, so the answer
/// can be kept. The cache is bound to one snapshot instance and a reload
/// installs a new snapshot, which means invalidation is not a rule anybody
/// has to remember: a stale cache cannot be reached.
/// </remarks>
internal sealed class WorldProjectionCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<PlayerRow>> _players =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, IReadOnlyList<ManagerRow>> _managers =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, IReadOnlyList<StaffRow>> _staff =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, IReadOnlyList<CompetitionRow>> _competitionsByClub =
        new(StringComparer.Ordinal);

    private IReadOnlyList<ClubRow>? _clubs;
    private Dictionary<string, ClubRow>? _clubsById;
    private IReadOnlyList<CompetitionRow>? _allCompetitions;

    internal WorldProjectionCache(WorldSnapshot snapshot)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    /// <summary>The snapshot this cache answers for. Identity is the cache key.</summary>
    internal WorldSnapshot Snapshot { get; }

    internal IReadOnlyList<ClubRow> Clubs =>
        _clubs ??= WorldClubProjection.ListClubs(Snapshot);

    internal IReadOnlyList<PlayerRow> PlayersOf(string clubExternalId) =>
        _players.GetOrAdd(clubExternalId, id => WorldPeopleProjection.ListPlayers(Snapshot, id));

    internal IReadOnlyList<ManagerRow> ManagersOf(string clubExternalId) =>
        _managers.GetOrAdd(clubExternalId, id => WorldPeopleProjection.ListManagers(Snapshot, id));

    internal IReadOnlyList<StaffRow> StaffOf(string clubExternalId) =>
        _staff.GetOrAdd(clubExternalId, id => WorldPeopleProjection.ListStaff(Snapshot, id));

    internal IReadOnlyList<CompetitionRow> CompetitionsOf(string clubExternalId) =>
        _competitionsByClub.GetOrAdd(
            clubExternalId,
            id => WorldCompetitionProjection.ListForClub(Snapshot, id));

    internal IReadOnlyList<CompetitionRow> AllCompetitions =>
        _allCompetitions ??= WorldCompetitionProjection.ListAll(Snapshot);

    /// <summary>
    /// The club by its external id. Built from the same projected rows the
    /// list returns, so a caller cannot get two different <c>ClubRow</c>
    /// values for one club depending on which door it came through.
    /// </summary>
    internal ClubRow? FindClub(string clubExternalId)
    {
        _clubsById ??= Clubs.ToDictionary(club => club.ExternalId, StringComparer.Ordinal);
        return _clubsById.TryGetValue(clubExternalId, out ClubRow? club) ? club : null;
    }
}
