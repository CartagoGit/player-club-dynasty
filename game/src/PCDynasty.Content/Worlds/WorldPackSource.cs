using System.Collections.Immutable;

namespace PCDynasty.Content.Worlds;

/// <summary>
/// Describes one world to load: where its documents are, which schema version
/// they must declare, and which attribute keys are numeric.
/// </summary>
/// <remarks>
/// The numeric keys are supplied by the caller rather than baked in, so the
/// content layer stays sport-neutral: football declares technique and form,
/// another pack declares whatever it uses.
/// </remarks>
public sealed class WorldPackSource
{
    public WorldPackSource(
        string providerId,
        int schemaVersion,
        IEnumerable<string> documentPaths,
        IEnumerable<string>? numericValueKeys = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentNullException.ThrowIfNull(documentPaths);

        ProviderId = providerId.Trim();
        SchemaVersion = schemaVersion;
        DocumentPaths = documentPaths
            .Select(path =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(path);
                return path.Trim();
            })
            .ToImmutableArray();
        if (DocumentPaths.Length == 0)
        {
            throw new ArgumentException("A world needs at least one document.", nameof(documentPaths));
        }

        NumericValueKeys = (numericValueKeys ?? [])
            .Select(key =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                return key.Trim();
            })
            .ToImmutableSortedSet(StringComparer.Ordinal);
    }

    public string ProviderId { get; }

    public int SchemaVersion { get; }

    public ImmutableArray<string> DocumentPaths { get; }

    public ImmutableSortedSet<string> NumericValueKeys { get; }
}
