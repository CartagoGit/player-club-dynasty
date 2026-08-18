using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

internal static class WorldClubProjection
{
    internal static IReadOnlyList<ClubRow> ListClubs(WorldContentSnapshot world)
    {
        Dictionary<string, string> countryNames = BuildCountryNames(world.Countries);
        Dictionary<string, int> playersPerClub = CountPlayers(world.People);

        return world.Clubs.Entities
            .Where(entity => entity.KindId == "club")
            .Select(entity => ProjectClub(entity, countryNames, playersPerClub))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static IReadOnlyList<PlayerRow> ListPlayers(
        WorldContentSnapshot world,
        string clubExternalId) =>
        world.People.Entities
            .Where(entity => entity.KindId == "person" && BelongsToClub(entity, clubExternalId))
            .Select(ProjectPlayer)
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();

    internal static ClubRow? FindClub(WorldContentSnapshot world, string clubExternalId) =>
        ListClubs(world).FirstOrDefault(club =>
            string.Equals(club.ExternalId, clubExternalId, StringComparison.Ordinal));

    private static ClubRow ProjectClub(
        NormalizedContentEntity entity,
        Dictionary<string, string> countryNames,
        Dictionary<string, int> playersPerClub)
    {
        string displayName = WorldContentValueReader.Read(entity, "display-name", entity.ExternalId);
        string country = entity.References
            .Where(reference => reference.RelationId == "country")
            .Select(reference => countryNames.GetValueOrDefault(reference.TargetExternalId))
            .FirstOrDefault(name => name is not null) ?? "—";
        playersPerClub.TryGetValue(entity.ExternalId, out int squadSize);

        return new ClubRow(
            entity.ExternalId,
            displayName,
            WorldContentValueReader.Read(entity, "short-name", displayName),
            WorldContentValueReader.Read(entity, "city", "—"),
            country,
            WorldContentValueReader.Read(entity, "founded-year", "—"),
            WorldContentValueReader.ReadInt(entity, "prestige"),
            WorldContentValueReader.ReadInt(entity, "fanbase"),
            squadSize);
    }

    private static PlayerRow ProjectPlayer(NormalizedContentEntity entity)
    {
        int technique = WorldContentValueReader.ReadInt(entity, "technique");
        int form = WorldContentValueReader.ReadInt(entity, "form");
        int physical = WorldContentValueReader.ReadInt(entity, "physical");
        int mental = WorldContentValueReader.ReadInt(entity, "mental");

        return new PlayerRow(
            entity.ExternalId,
            WorldContentValueReader.Read(entity, "display-name", entity.ExternalId),
            WorldContentValueReader.ReadInt(entity, "birth-year"),
            technique,
            form,
            physical,
            mental,
            (technique + form + physical + mental) / 4);
    }

    private static bool BelongsToClub(NormalizedContentEntity person, string clubExternalId) =>
        person.References.Any(reference =>
            reference.RelationId == "club"
            && reference.TargetExternalId == clubExternalId);

    private static Dictionary<string, string> BuildCountryNames(NormalizedContentModel countries) =>
        countries.Entities
            .Where(entity => entity.KindId == "country")
            .ToDictionary(
                entity => entity.ExternalId,
                entity => WorldContentValueReader.Read(
                    entity,
                    "short-name",
                    WorldContentValueReader.Read(entity, "display-name", entity.ExternalId)),
                StringComparer.Ordinal);

    private static Dictionary<string, int> CountPlayers(NormalizedContentModel people) =>
        people.Entities
            .Where(entity => entity.KindId == "person"
                && WorldContentValueReader.Read(entity, "occupation", string.Empty) == "player")
            .SelectMany(entity => entity.References)
            .Where(reference => reference.RelationId == "club")
            .GroupBy(reference => reference.TargetExternalId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
}
