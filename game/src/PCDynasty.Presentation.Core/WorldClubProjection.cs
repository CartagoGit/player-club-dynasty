using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>Projects clubs. People belong to <see cref="WorldPeopleProjection"/>.</summary>
internal static class WorldClubProjection
{
    internal static IReadOnlyList<ClubRow> ListClubs(WorldContentSnapshot world)
    {
        Dictionary<string, string> countryNames = WorldCountryNames.Build(world.Countries);
        Dictionary<string, int> playersPerClub = CountPlayers(world.People);

        return world.Clubs.Entities
            .Where(entity => entity.KindId == "club")
            .Select(entity => ProjectClub(entity, countryNames, playersPerClub))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static ClubRow? FindClub(WorldContentSnapshot world, string clubExternalId) =>
        ListClubs(world).FirstOrDefault(club =>
            string.Equals(club.ExternalId, clubExternalId, StringComparison.Ordinal));

    private static ClubRow ProjectClub(
        NormalizedContentEntity entity,
        Dictionary<string, string> countryNames,
        Dictionary<string, int> playersPerClub)
    {
        string displayName = WorldContentValueReader.Read(entity, "display-name", entity.ExternalId);
        playersPerClub.TryGetValue(entity.ExternalId, out int squadSize);

        return new ClubRow(
            entity.ExternalId,
            displayName,
            WorldContentValueReader.Read(entity, "short-name", displayName),
            WorldContentValueReader.Read(entity, "city", WorldCountryNames.Unknown),
            WorldCountryNames.Of(entity, countryNames),
            WorldContentValueReader.Read(entity, "founded-year", WorldCountryNames.Unknown),
            WorldContentValueReader.ReadInt(entity, "prestige"),
            WorldContentValueReader.ReadInt(entity, "fanbase"),
            squadSize);
    }

    /// <summary>Squad size counts players only, never managers or staff.</summary>
    private static Dictionary<string, int> CountPlayers(NormalizedContentModel people) =>
        people.Entities
            .Where(entity => entity.KindId == "person"
                && WorldPeopleProjection.Occupation(entity) == WorldPeopleProjection.PlayerOccupation)
            .SelectMany(entity => entity.References)
            .Where(reference => reference.RelationId == "club")
            .GroupBy(reference => reference.TargetExternalId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
}
