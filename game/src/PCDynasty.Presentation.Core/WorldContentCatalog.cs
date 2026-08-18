using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Worlds;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Loads one world through the canonical pipeline and keeps the indexed
/// snapshot the projections read from.
/// </summary>
internal sealed class WorldContentCatalog
{
    private const int SchemaVersion = 1;

    private static readonly string[] DocumentNames =
    [
        "clubs.json",
        "people.json",
        "competitions.json",
        "countries.json",
        "rules.json",
    ];

    /// <summary>
    /// Attribute keys the world declares as integers. Unparsable values fail
    /// the load with a diagnostic instead of silently becoming zero.
    /// </summary>
    private static readonly string[] NumericValueKeys =
    [
        "prestige",
        "fanbase",
        "birth-year",
        "technique",
        "form",
        "physical",
        "mental",
        "reputation",
    ];

    private readonly WorldPackLoader _loader;
    private readonly string _contentRoot;
    private readonly string _providerId;
    private WorldSnapshot? _snapshot;

    internal WorldContentCatalog(IContentReader reader, string contentRoot, string providerId = "starter-world")
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerId);

        _loader = new WorldPackLoader(reader);
        _contentRoot = contentRoot;
        _providerId = providerId;
    }

    internal async Task ReloadAsync(CancellationToken cancellationToken)
    {
        var source = new WorldPackSource(
            _providerId,
            SchemaVersion,
            DocumentNames.Select(name => Path.Combine(_contentRoot, "data", name)),
            NumericValueKeys);

        WorldPackLoadResult result = await _loader
            .LoadAsync(source, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to load the world from '{_contentRoot}': {result.DescribeErrors()}");
        }

        Interlocked.Exchange(ref _snapshot, new WorldSnapshot(result.Pack!, result.ContentHash));
    }

    internal WorldSnapshot GetSnapshot() =>
        Volatile.Read(ref _snapshot)
        ?? throw new InvalidOperationException(
            "WorldService has not loaded its content. Call ReloadAsync() first.");
}

internal static class WorldContentValueReader
{
    internal static string Read(
        NormalizedContentEntity entity,
        string key,
        string fallback) =>
        entity.Values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    /// <summary>
    /// Reads a declared integer. The loader has already rejected unparsable
    /// numbers, so the fallback only covers an attribute the pack omits.
    /// </summary>
    internal static int ReadInt(NormalizedContentEntity entity, string key)
    {
        string value = Read(entity, key, "0");
        return int.TryParse(
            value,
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out int parsed)
            ? parsed
            : 0;
    }
}
