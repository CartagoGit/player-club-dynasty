using System.Collections.Immutable;
using PCDynasty.Content;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// One loaded world, indexed for constant-time reads.
/// </summary>
/// <remarks>
/// Audit a00001 measured the previous path as roughly O(clubs × people): every
/// squad query walked all people, finding a club rebuilt the whole list, and
/// the market repeated both per club. The indexes are built once per load, so
/// a screen pays for what it renders and nothing more.
/// </remarks>
internal sealed class WorldSnapshot
{
    private static readonly ImmutableArray<NormalizedContentEntity> NoEntities = [];

    internal WorldSnapshot(CompiledContentPack pack, string contentHash)
    {
        Pack = pack ?? throw new ArgumentNullException(nameof(pack));
        ContentHash = contentHash ?? throw new ArgumentNullException(nameof(contentHash));

        PeopleByClub = ByClub(EntitiesOf("person"));
        CountryNames = WorldCountryNames.Build(EntitiesOf("country"));
    }

    internal CompiledContentPack Pack { get; }

    /// <summary>Identifies the exact content this world was compiled from.</summary>
    internal string ContentHash { get; }

    internal ImmutableDictionary<string, ImmutableArray<NormalizedContentEntity>> PeopleByClub { get; }

    internal ImmutableDictionary<string, string> CountryNames { get; }

    internal ImmutableArray<NormalizedContentEntity> Clubs => EntitiesOf("club");

    internal ImmutableArray<NormalizedContentEntity> Competitions => EntitiesOf("competition");

    internal ImmutableArray<NormalizedContentEntity> PeopleOf(string clubExternalId) =>
        PeopleByClub.TryGetValue(clubExternalId, out ImmutableArray<NormalizedContentEntity> people)
            ? people
            : NoEntities;

    internal NormalizedContentEntity? Entity(string externalId) =>
        Pack.TryGetEntity(externalId, out NormalizedContentEntity? entity) ? entity : null;

    private ImmutableArray<NormalizedContentEntity> EntitiesOf(string kindId) =>
        Pack.EntitiesByKind.TryGetValue(kindId, out ImmutableArray<NormalizedContentEntity> entities)
            ? entities
            : NoEntities;

    private static ImmutableDictionary<string, ImmutableArray<NormalizedContentEntity>> ByClub(
        ImmutableArray<NormalizedContentEntity> people) =>
        people
            .SelectMany(person => person.References
                .Where(reference => reference.RelationId == "club")
                .Select(reference => (Club: reference.TargetExternalId, Person: person)))
            .GroupBy(pair => pair.Club, StringComparer.Ordinal)
            .ToImmutableDictionary(
                group => group.Key,
                group => group
                    .Select(pair => pair.Person)
                    .OrderBy(person => person.ExternalId, StringComparer.Ordinal)
                    .ToImmutableArray(),
                StringComparer.Ordinal);
}
