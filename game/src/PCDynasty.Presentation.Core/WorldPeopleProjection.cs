using System.Collections.Immutable;
using System.Globalization;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Projects the people of a club by occupation. Audit a00001 found the squad
/// was every person referencing the club, which put managers on the pitch with
/// zero-valued attributes; occupation is what the content pack declares, so a
/// new role needs content, not code.
/// </summary>
internal static class WorldPeopleProjection
{
    internal const string PlayerOccupation = "player";
    private const string ManagerOccupation = "manager";

    internal static IReadOnlyList<PlayerRow> ListPlayers(
        WorldSnapshot world,
        string clubExternalId)
    {
        return People(world, clubExternalId, occupation => occupation == PlayerOccupation)
            .Select(entity => ProjectPlayer(entity, world.CountryNames))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static IReadOnlyList<ManagerRow> ListManagers(
        WorldSnapshot world,
        string clubExternalId)
    {
        return People(world, clubExternalId, occupation => occupation == ManagerOccupation)
            .Select(entity => ProjectManager(entity, world.CountryNames))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static IReadOnlyList<StaffRow> ListStaff(
        WorldSnapshot world,
        string clubExternalId)
    {
        return People(
                world,
                clubExternalId,
                occupation => occupation != PlayerOccupation && occupation != ManagerOccupation)
            .Select(entity => ProjectStaff(entity, world.CountryNames))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal static string Occupation(NormalizedContentEntity entity) =>
        WorldContentValueReader.Read(entity, "occupation", string.Empty);

    /// <summary>Indexed: the club's own people, not every person in the world.</summary>
    private static IEnumerable<NormalizedContentEntity> People(
        WorldSnapshot world,
        string clubExternalId,
        Func<string, bool> occupationFilter) =>
        world.PeopleOf(clubExternalId).Where(entity => occupationFilter(Occupation(entity)));

    private static PlayerRow ProjectPlayer(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, string> countryNames)
    {
        int technique = WorldContentValueReader.ReadInt(entity, "technique");
        int form = WorldContentValueReader.ReadInt(entity, "form");
        int physical = WorldContentValueReader.ReadInt(entity, "physical");
        int mental = WorldContentValueReader.ReadInt(entity, "mental");
        int birthYear = WorldContentValueReader.ReadInt(entity, "birth-year");

        return new PlayerRow(
            entity.ExternalId,
            WorldContentValueReader.Read(entity, "display-name", entity.ExternalId),
            birthYear,
            BirthDateOf(entity, birthYear),
            WorldCountryNames.Of(entity, countryNames),
            technique,
            form,
            physical,
            mental,
            (technique + form + physical + mental) / 4);
    }

    private static ManagerRow ProjectManager(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, string> countryNames) =>
        new(
            entity.ExternalId,
            WorldContentValueReader.Read(entity, "display-name", entity.ExternalId),
            WorldContentValueReader.ReadInt(entity, "birth-year"),
            WorldCountryNames.Of(entity, countryNames),
            WorldContentValueReader.ReadInt(entity, "reputation"));

    private static StaffRow ProjectStaff(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, string> countryNames) =>
        new(
            entity.ExternalId,
            WorldContentValueReader.Read(entity, "display-name", entity.ExternalId),
            WorldContentValueReader.Read(entity, "occupation", WorldCountryNames.Unknown),
            WorldContentValueReader.ReadInt(entity, "birth-year"),
            WorldCountryNames.Of(entity, countryNames));

    /// <summary>
    /// Exact birth date when the pack declares one; otherwise the first day of
    /// the declared year, so an age is never invented from a literal.
    /// </summary>
    private static DateOnly BirthDateOf(NormalizedContentEntity entity, int birthYear)
    {
        string declared = WorldContentValueReader.Read(entity, "birth-date", string.Empty);
        if (DateOnly.TryParse(declared, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsed))
        {
            return parsed;
        }

        return birthYear > 0 ? new DateOnly(birthYear, 1, 1) : DateOnly.MinValue;
    }
}
