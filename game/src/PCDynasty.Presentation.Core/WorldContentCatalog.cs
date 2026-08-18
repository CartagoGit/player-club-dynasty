using PCDynasty.Content.Abstractions;

namespace PCDynasty.Presentation.Core;

internal sealed class WorldContentCatalog
{
    private static readonly string[] FileNames =
    [
        "clubs.json",
        "people.json",
        "competitions.json",
        "countries.json",
        "rules.json",
    ];

    private readonly IContentReader _reader;
    private readonly string _contentRoot;
    private WorldContentSnapshot? _snapshot;

    internal WorldContentCatalog(IContentReader reader, string contentRoot)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        if (string.IsNullOrWhiteSpace(contentRoot))
        {
            throw new ArgumentException("Content root cannot be empty.", nameof(contentRoot));
        }

        _contentRoot = contentRoot;
    }

    internal async Task ReloadAsync(CancellationToken cancellationToken)
    {
        var models = new Dictionary<string, NormalizedContentModel>(StringComparer.Ordinal);
        foreach (string fileName in FileNames)
        {
            ContentReadResult result = await ReadFileAsync(fileName, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(result, fileName);
            models.Add(fileName, result.Model!);
        }

        var snapshot = new WorldContentSnapshot(
            models["clubs.json"],
            models["people.json"],
            models["competitions.json"],
            models["countries.json"],
            models["rules.json"]);
        Interlocked.Exchange(ref _snapshot, snapshot);
    }

    internal WorldContentSnapshot GetSnapshot() =>
        Volatile.Read(ref _snapshot)
        ?? throw new InvalidOperationException(
            "WorldService has not loaded its content. Call ReloadAsync() first.");

    private ValueTask<ContentReadResult> ReadFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string path = Path.Combine(_contentRoot, "data", fileName);
        var request = new ContentReadRequest(path, "starter-world", expectedSchemaVersion: 1);
        return _reader.ReadAsync(request, cancellationToken);
    }

    private static void EnsureSuccess(ContentReadResult result, string fileName)
    {
        if (result.IsSuccess)
        {
            return;
        }

        string errors = string.Join("; ", result.Diagnostics
            .Where(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error)
            .Select(diagnostic => $"[{diagnostic.Code}] {diagnostic.Message}"));
        throw new InvalidOperationException($"Failed to load {fileName}: {errors}");
    }
}

internal sealed record WorldContentSnapshot(
    NormalizedContentModel Clubs,
    NormalizedContentModel People,
    NormalizedContentModel Competitions,
    NormalizedContentModel Countries,
    NormalizedContentModel Rules);

internal static class WorldContentValueReader
{
    internal static string Read(
        NormalizedContentEntity entity,
        string key,
        string fallback) =>
        entity.Values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

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
