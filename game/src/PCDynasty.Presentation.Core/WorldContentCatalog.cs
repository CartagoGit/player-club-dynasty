using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Worlds;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Loads one world through the canonical pipeline and keeps the indexed
/// snapshot the projections read from.
/// </summary>
internal sealed class WorldContentCatalog
{
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
    private readonly ContentSourceDescriptor _source;
    private WorldSnapshot? _snapshot;

    internal WorldContentCatalog(IContentReader reader, ContentSourceDescriptor source)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(source);

        _loader = new WorldPackLoader(reader);
        _source = source;
    }

    internal ContentSourceDescriptor Source => _source;

    internal async Task ReloadAsync(CancellationToken cancellationToken)
    {
        WorldPackLoadResult result = await _loader
            .LoadAsync(_source.ToPackSource(NumericValueKeys), cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to load the world from '{_source.Path}': {result.DescribeErrors()}");
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
