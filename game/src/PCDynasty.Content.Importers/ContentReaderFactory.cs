using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Importers;

/// <summary>
/// Resolves the reader for a declared format, so choosing a source never means
/// choosing a class at a call site.
/// </summary>
public static class ContentReaderFactory
{
    public static IContentReader Create(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        return formatId.Trim().ToLowerInvariant() switch
        {
            "json" => new JsonContentReader(),
            "csv" => new CsvContentReader(),
            "sqlite" => new SqliteContentReader(),
            _ => throw new ArgumentException($"No reader is registered for format '{formatId}'.", nameof(formatId)),
        };
    }

    public static IReadOnlyList<string> SupportedFormats { get; } = ["json", "csv", "sqlite"];
}
