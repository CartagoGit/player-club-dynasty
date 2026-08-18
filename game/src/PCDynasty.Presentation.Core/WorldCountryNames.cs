using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Country labels shared by the club and people projections. Both need the
/// same lookup, and neither owns it.
/// </summary>
internal static class WorldCountryNames
{
    internal const string Unknown = "—";

    internal static ImmutableDictionary<string, string> Build(
        ImmutableArray<NormalizedContentEntity> countries) =>
        countries
            .ToImmutableDictionary(
                entity => entity.ExternalId,
                entity => WorldContentValueReader.Read(
                    entity,
                    "short-name",
                    WorldContentValueReader.Read(entity, "display-name", entity.ExternalId)),
                StringComparer.Ordinal);

    /// <summary>Country an entity references, or the unknown marker.</summary>
    internal static string Of(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, string> countryNames) =>
        entity.References
            .Where(reference => reference.RelationId == "country")
            .Select(reference => countryNames.GetValueOrDefault(reference.TargetExternalId))
            .FirstOrDefault(name => name is not null) ?? Unknown;
}
