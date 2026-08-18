using System.Collections.Immutable;

namespace PCDynasty.Content.Worlds;

/// <summary>
/// Where a world comes from: the format, the path and the documents to read.
/// </summary>
/// <remarks>
/// The bootstrap used to hard-code the starter directory and a JSON reader, so
/// the promise of externally loadable data was theoretical (audit a00001). A
/// described source is the seam that makes an external directory, spreadsheet
/// or database a first-class choice instead of a code change.
/// </remarks>
public sealed class ContentSourceDescriptor
{
    /// <summary>Documents a directory-shaped world is split into.</summary>
    public static readonly ImmutableArray<string> DefaultDocumentNames =
    [
        "clubs.json",
        "people.json",
        "competitions.json",
        "countries.json",
        "rules.json",
    ];

    private ContentSourceDescriptor(
        string formatId,
        string path,
        string displayName,
        string providerId,
        int schemaVersion,
        bool isIntegrated,
        ImmutableArray<string> documentPaths)
    {
        FormatId = formatId;
        Path = path;
        DisplayName = displayName;
        ProviderId = providerId;
        SchemaVersion = schemaVersion;
        IsIntegrated = isIntegrated;
        DocumentPaths = documentPaths;
    }

    public string FormatId { get; }

    /// <summary>Directory for a document set, file for a single-source format.</summary>
    public string Path { get; }

    public string DisplayName { get; }

    public string ProviderId { get; }

    public int SchemaVersion { get; }

    /// <summary>True for the world shipped with the game.</summary>
    public bool IsIntegrated { get; }

    public ImmutableArray<string> DocumentPaths { get; }

    /// <summary>The world shipped with the game, read as JSON documents.</summary>
    public static ContentSourceDescriptor Integrated(
        string contentRoot,
        string providerId = "starter-world",
        int schemaVersion = 1) =>
        ForDirectory(contentRoot, providerId, schemaVersion, "Mundo integrado", isIntegrated: true);

    /// <summary>
    /// A world the player points at. The format is inferred from the path: a
    /// directory holds JSON documents, a file is read by its extension.
    /// </summary>
    public static ContentSourceDescriptor External(
        string path,
        string providerId = "external-world",
        int schemaVersion = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string normalized = path.Trim();
        string extension = System.IO.Path.GetExtension(normalized).ToLowerInvariant();

        return extension switch
        {
            "" => ForDirectory(normalized, providerId, schemaVersion, DisplayNameOf(normalized), isIntegrated: false),
            ".json" => ForFile(normalized, "json", providerId, schemaVersion),
            ".csv" => ForFile(normalized, "csv", providerId, schemaVersion),
            ".sqlite" or ".db" => ForFile(normalized, "sqlite", providerId, schemaVersion),
            _ => throw new ArgumentException(
                $"'{extension}' is not a supported world format. Use a directory of JSON documents, or a .json, .csv, .sqlite or .db file.",
                nameof(path)),
        };
    }

    private static ContentSourceDescriptor ForDirectory(
        string root,
        string providerId,
        int schemaVersion,
        string displayName,
        bool isIntegrated)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);

        string normalized = root.Trim();
        return new ContentSourceDescriptor(
            "json",
            normalized,
            displayName,
            providerId.Trim(),
            schemaVersion,
            isIntegrated,
            DefaultDocumentNames
                .Select(name => System.IO.Path.Combine(normalized, "data", name))
                .ToImmutableArray());
    }

    private static ContentSourceDescriptor ForFile(
        string path,
        string formatId,
        string providerId,
        int schemaVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);

        return new ContentSourceDescriptor(
            formatId,
            path,
            DisplayNameOf(path),
            providerId.Trim(),
            schemaVersion,
            isIntegrated: false,
            [path]);
    }

    private static string DisplayNameOf(string path)
    {
        string name = System.IO.Path.GetFileName(path.TrimEnd(
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar));
        return string.IsNullOrWhiteSpace(name) ? path : name;
    }

    public WorldPackSource ToPackSource(IEnumerable<string> numericValueKeys) =>
        new(ProviderId, SchemaVersion, DocumentPaths, numericValueKeys);
}
