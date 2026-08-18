using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

internal static class WorldCompetitionProjection
{
    internal static IReadOnlyList<CompetitionRow> ListForClub(
        WorldContentSnapshot world,
        string clubExternalId) =>
        Project(world, clubExternalId, filterByClub: true);

    internal static IReadOnlyList<CompetitionRow> ListAll(WorldContentSnapshot world) =>
        Project(world, string.Empty, filterByClub: false);

    private static IReadOnlyList<CompetitionRow> Project(
        WorldContentSnapshot world,
        string clubExternalId,
        bool filterByClub)
    {
        Dictionary<string, string> clubNames = BuildClubNames(world.Clubs);
        return world.Competitions.Entities
            .Where(entity => entity.KindId == "competition")
            .Where(entity => !filterByClub || IncludesClub(entity, clubExternalId))
            .Select(entity => ProjectCompetition(entity, clubExternalId, clubNames))
            .OrderBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static CompetitionRow ProjectCompetition(
        NormalizedContentEntity entity,
        string clubExternalId,
        IReadOnlyDictionary<string, string> clubNames)
    {
        ImmutableArray<string> rivals = entity.References
            .Where(reference => reference.RelationId == "participant")
            .Where(reference => reference.TargetExternalId != clubExternalId)
            .Select(reference => clubNames.GetValueOrDefault(reference.TargetExternalId))
            .Where(name => name is not null)
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToImmutableArray();

        return new CompetitionRow(
            entity.ExternalId,
            WorldContentValueReader.Read(entity, "display-name", entity.ExternalId),
            WorldContentValueReader.Read(entity, "format", "—"),
            WorldContentValueReader.Read(entity, "edition-frequency", "—"),
            rivals);
    }

    private static bool IncludesClub(NormalizedContentEntity competition, string clubExternalId) =>
        competition.References.Any(reference =>
            reference.RelationId == "participant"
            && reference.TargetExternalId == clubExternalId);

    private static Dictionary<string, string> BuildClubNames(NormalizedContentModel clubs) =>
        clubs.Entities
            .Where(entity => entity.KindId == "club")
            .ToDictionary(
                entity => entity.ExternalId,
                entity => WorldContentValueReader.Read(
                    entity,
                    "short-name",
                    WorldContentValueReader.Read(entity, "display-name", entity.ExternalId)),
                StringComparer.Ordinal);
}
