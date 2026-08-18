namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The three root keys of a locale file. The registry and the options
/// picker read this lightweight shape to enumerate languages without
/// loading the full file. The icon path is not validated at peek
/// time; the strict loader validates it on activation.
/// </summary>
/// <param name="Id">The lang id; the file's filename stem is the source of truth.</param>
/// <param name="Name">How the language calls itself in its own words.</param>
/// <param name="Icon">A Godot resource path to the flag icon, e.g. <c>flags/es.svg</c>.</param>
public sealed record LocaleMetadata(string Id, string Name, string Icon)
{
    /// <summary>Path the registry used to discover the file (for diagnostics).</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Whether the file is built-in or external; populated by the registry.</summary>
    public LocaleSource Source { get; init; } = LocaleSource.BuiltIn;
}
