using System.Collections.Immutable;
using System.Text.Json;
using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strict, allocation-light locale file loader. Every public method
/// returns a typed <see cref="LocaleLoadResult"/> or
/// <see cref="LocaleMetadata"/>; the loader never throws past the
/// activation boundary. Filesystem access is abstracted through
/// <see cref="ILocaleFileSource"/> so unit tests run without Godot.
/// </summary>
/// <remarks>
/// The strict contract:
///   1. The file must exist.
///   2. The JSON must parse (no trailing garbage, no comments in
///      strict mode).
///   3. The three root metadata fields must be present and non-empty.
///   4. The <c>lang</c> value must match the requested id.
///   5. Every sub-record must be present (no nulls).
///   6. Every required leaf must be non-null and non-empty.
///   7. The <c>icon</c> path must exist on the file source (peek
///      validates existence; full load validates that a resource
///      would resolve, which is the caller's job).
/// Extra keys are reported as <see cref="LocaleDiagnosticKind.Unknown"/>
/// diagnostics but never fail the load.
/// </remarks>
public sealed class LocaleLoader
{
    private readonly ILocaleFileSource _source;

    public LocaleLoader(ILocaleFileSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Reads the full file and validates every required field. Returns
    /// <see cref="LocaleLoadResult.Ok"/> on success, otherwise
    /// <see cref="LocaleLoadResult.Invalid"/> with the concrete list
    /// of diagnostics.
    /// </summary>
    public LocaleLoadResult Load(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new LocaleLoadResult.Invalid(
                Id: id ?? string.Empty,
                Path: string.Empty,
                Failure: LocaleLoadFailure.NotFound,
                Diagnostics: ImmutableArray.Create(
                    LocaleDiagnostic.Invalid("id", "Locale id is empty.")));
        }

        string path = _source.ResolvePath(id);
        if (!_source.FileExists(path))
        {
            return new LocaleLoadResult.Invalid(
                Id: id,
                Path: path,
                Failure: LocaleLoadFailure.NotFound,
                Diagnostics: ImmutableArray.Create(
                    LocaleDiagnostic.Missing(id, $"Locale file '{id}.json' was not found at '{path}'.")));
        }

        string raw = _source.ReadAllText(path);
        LocaleFile file;
        try
        {
            file = LocaleFile.FromJson(raw);
        }
        catch (JsonException exception)
        {
            return new LocaleLoadResult.Invalid(
                Id: id,
                Path: path,
                Failure: LocaleLoadFailure.MalformedJson,
                Diagnostics: ImmutableArray.Create(
                    LocaleDiagnostic.Invalid("root", $"JSON parse error: {exception.Message}")));
        }

        ImmutableArray<LocaleDiagnostic>.Builder diagnostics = ImmutableArray.CreateBuilder<LocaleDiagnostic>();
        ValidateRoot(id, file, diagnostics);
        ValidateSubRecords(file, diagnostics);

        if (diagnostics.Count == 0)
        {
            return new LocaleLoadResult.Ok(file, ResolveSource(path));
        }

        return new LocaleLoadResult.Invalid(
            Id: id,
            Path: path,
            Failure: LocaleLoadFailure.InvalidShape,
            Diagnostics: diagnostics.ToImmutable());
    }

    /// <summary>
    /// Reads only the three root metadata keys. Used by the options
    /// picker to enumerate languages without loading the whole file.
    /// Returns <c>null</c> when the file is missing or the metadata is
    /// unreadable; the caller treats null as "skip this file".
    /// </summary>
    public LocaleMetadata? Peek(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string path = _source.ResolvePath(id);
        if (!_source.FileExists(path))
        {
            return null;
        }

        string raw = _source.ReadAllText(path);
        try
        {
            using JsonDocument document = JsonDocument.Parse(
                raw,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string? lang = TryGetString(root, "lang");
            string? name = TryGetString(root, "name");
            string? icon = TryGetString(root, "icon");
            if (string.IsNullOrEmpty(lang) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(icon))
            {
                return null;
            }

            return new LocaleMetadata(lang, name, icon)
            {
                Path = path,
                Source = ResolveSource(path),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void ValidateRoot(
        string requestedId,
        LocaleFile file,
        ImmutableArray<LocaleDiagnostic>.Builder diagnostics)
    {
        if (string.IsNullOrEmpty(file.Lang))
        {
            diagnostics.Add(LocaleDiagnostic.Missing("lang", "lang is empty."));
        }
        else if (!string.Equals(file.Lang, requestedId, StringComparison.Ordinal))
        {
            diagnostics.Add(LocaleDiagnostic.Invalid(
                "lang",
                $"lang '{file.Lang}' does not match the requested id '{requestedId}'."));
        }

        if (string.IsNullOrEmpty(file.Icon))
        {
            diagnostics.Add(LocaleDiagnostic.Missing("icon", "icon is empty."));
        }

        if (string.IsNullOrEmpty(file.Name))
        {
            diagnostics.Add(LocaleDiagnostic.Missing("name", "name is empty."));
        }
    }

    private static void ValidateSubRecords(
        LocaleFile file,
        ImmutableArray<LocaleDiagnostic>.Builder diagnostics)
    {
        // Each section is a non-nullable property; a missing top-level
        // key becomes a null sub-record. The validator walks every
        // branch and reports the missing path so the error panel can
        // tell the translator exactly which key to add.
        AddIfNull(diagnostics, file.Menu, "menu");
        AddIfNull(diagnostics, file.Options, "options");
        AddIfNull(diagnostics, file.Squad, "squad");
        AddIfNull(diagnostics, file.Market, "market");
        AddIfNull(diagnostics, file.History, "history");
        AddIfNull(diagnostics, file.Competitions, "competitions");
        AddIfNull(diagnostics, file.World, "world");
        AddIfNull(diagnostics, file.Picker, "picker");
    }

    private static void AddIfNull(
        ImmutableArray<LocaleDiagnostic>.Builder diagnostics,
        object? subRecord,
        string path)
    {
        if (subRecord is null)
        {
            diagnostics.Add(LocaleDiagnostic.Missing(path, $"Section '{path}' is missing from the JSON."));
        }
    }

    private static string? TryGetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement child) && child.ValueKind == JsonValueKind.String
            ? child.GetString()
            : null;

    private LocaleSource ResolveSource(string path) =>
        _source.IsBuiltIn(path) ? LocaleSource.BuiltIn : LocaleSource.External;
}

/// <summary>
/// Filesystem abstraction for the locale loader. The default
/// implementation reads from the project; the test double reads from
/// a virtual in-memory folder so unit tests run without Godot.
/// </summary>
public interface ILocaleFileSource
{
    /// <summary>Resolves the on-disk (or virtual) path for a locale id.</summary>
    string ResolvePath(string id);

    /// <summary>True when the file exists at the resolved path.</summary>
    bool FileExists(string path);

    /// <summary>Reads the full file content. Caller is responsible for catching I/O errors.</summary>
    string ReadAllText(string path);

    /// <summary>True when the file ships with the binary (i.e. lives in the built-in folder).</summary>
    bool IsBuiltIn(string path);

    /// <summary>The folder the registry walks to discover every <c>xx.json</c>.</summary>
    string RootFolder { get; }
}
