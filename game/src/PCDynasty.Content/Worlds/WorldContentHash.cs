using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Worlds;

/// <summary>
/// Hashes normalized content so a world is identified by what it says, not by
/// where it came from. Equivalent JSON, CSV and SQLite sources — and the same
/// pack read as the integrated world or as an external directory — therefore
/// produce the same hash, and a save can bind to it.
/// </summary>
/// <remarks>
/// The provider label is deliberately excluded: it names the source, not the
/// content, and including it would stop a save from opening against the same
/// data offered under a different name. The schema version stays in, because
/// it changes how the values are interpreted.
/// </remarks>
public static class WorldContentHash
{
    public static string Compute(NormalizedContentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var canonical = new StringBuilder();
        canonical.Append(model.SchemaVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');

        foreach (NormalizedContentEntity entity in model.Entities
            .OrderBy(entity => entity.ExternalId, StringComparer.Ordinal))
        {
            canonical.Append(entity.ExternalId).Append('\t').Append(entity.KindId).Append('\n');
            foreach ((string key, string value) in entity.Values
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                canonical.Append('\t').Append(key).Append('=').Append(value).Append('\n');
            }

            foreach (ContentReference reference in entity.References
                .OrderBy(reference => reference.RelationId, StringComparer.Ordinal)
                .ThenBy(reference => reference.TargetExternalId, StringComparer.Ordinal)
                .ThenBy(reference => reference.Required))
            {
                canonical
                    .Append("\t>")
                    .Append(reference.RelationId)
                    .Append('>')
                    .Append(reference.TargetExternalId)
                    .Append(reference.Required ? "!" : "?")
                    .Append('\n');
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))
            .ToLowerInvariant();
    }

    /// <summary>Hash of an already merged entity set, used while loading.</summary>
    internal static string Compute(
        string providerId,
        int schemaVersion,
        ImmutableArray<NormalizedContentEntity> entities) =>
        Compute(new NormalizedContentModel(providerId, schemaVersion, entities));
}
