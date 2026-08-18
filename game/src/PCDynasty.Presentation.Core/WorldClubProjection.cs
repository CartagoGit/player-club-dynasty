using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>Projects clubs. People belong to <see cref="WorldPeopleProjection"/>.</summary>
internal static class WorldClubProjection
{
    internal static IReadOnlyList<ClubRow> ListClubs(WorldSnapshot world) =>
        world.Clubs
            .Select(entity => ProjectClub(entity, world))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();

    /// <summary>Indexed lookup: finding a club no longer rebuilds the list.</summary>
    internal static ClubRow? FindClub(WorldSnapshot world, string clubExternalId)
    {
        NormalizedContentEntity? entity = world.Entity(clubExternalId);
        return entity is null || entity.KindId != "club" ? null : ProjectClub(entity, world);
    }

    private static ClubRow ProjectClub(NormalizedContentEntity entity, WorldSnapshot world)
    {
        string displayName = WorldContentValueReader.Read(entity, "display-name", entity.ExternalId);
        int squadSize = CountPlayers(world, entity.ExternalId);

        return new ClubRow(
            entity.ExternalId,
            displayName,
            WorldContentValueReader.Read(entity, "short-name", displayName),
            WorldContentValueReader.Read(entity, "city", WorldCountryNames.Unknown),
            WorldCountryNames.Of(entity, world.CountryNames),
            WorldContentValueReader.Read(entity, "founded-year", WorldCountryNames.Unknown),
            WorldContentValueReader.ReadInt(entity, "prestige"),
            WorldContentValueReader.ReadInt(entity, "fanbase"),
            squadSize);
    }

    /// <summary>Squad size counts players only, never managers or staff.</summary>
    private static int CountPlayers(WorldSnapshot world, string clubExternalId) =>
        world.PeopleOf(clubExternalId)
            .Count(person =>
                WorldPeopleProjection.Occupation(person) == WorldPeopleProjection.PlayerOccupation);
}
