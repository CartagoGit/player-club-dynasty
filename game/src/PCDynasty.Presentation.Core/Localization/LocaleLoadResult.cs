using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The result of a strict locale load. The loader never throws: every
/// failure mode is encoded as a value so the activation service can
/// branch on it without try/catch boilerplate.
/// </summary>
public abstract record LocaleLoadResult
{
    public sealed record Ok(LocaleFile File, LocaleSource Source) : LocaleLoadResult;

    /// <summary>
    /// The file is broken in a way the loader can describe concretely.
    /// The diagnostics list every required key the player (or
    /// translator) needs to fix.
    /// </summary>
    public sealed record Invalid(
        string Id,
        string Path,
        LocaleLoadFailure Failure,
        ImmutableArray<LocaleDiagnostic> Diagnostics) : LocaleLoadResult;

    public bool IsOk => this is Ok;
    public bool IsInvalid => this is Invalid;
}

/// <summary>
/// The reason a load failed. The activation service uses the kind to
/// decide whether to fall back to English or to surface a fatal panel.
/// </summary>
public enum LocaleLoadFailure
{
    /// <summary>The file does not exist on disk.</summary>
    NotFound,

    /// <summary>The file exists but the JSON is malformed or has the wrong root shape.</summary>
    MalformedJson,

    /// <summary>The file parsed, but the root metadata or one of the sections failed validation.</summary>
    InvalidShape,
}

/// <summary>
/// A single concrete problem the loader found in a locale file. The
/// path is the JSON Pointer-style address (e.g. <c>market.col.price</c>,
/// <c>icon</c>, <c>lang</c>) so the panel can show "market.col.price
/// is missing" instead of "something is wrong".
/// </summary>
public sealed record LocaleDiagnostic(
    LocaleDiagnosticKind Kind,
    string Path,
    string Message)
{
    public static LocaleDiagnostic Missing(string path, string message) =>
        new(LocaleDiagnosticKind.Missing, path, message);

    public static LocaleDiagnostic Invalid(string path, string message) =>
        new(LocaleDiagnosticKind.Invalid, path, message);

    public static LocaleDiagnostic Unknown(string path, string message) =>
        new(LocaleDiagnosticKind.Unknown, path, message);
}

public enum LocaleDiagnosticKind
{
    /// <summary>A required key is absent from the file.</summary>
    Missing,

    /// <summary>A required key is present but its value is null or empty.</summary>
    Invalid,

    /// <summary>A key is present but the loader does not recognise it. Reported but does not fail the load.</summary>
    Unknown,
}
