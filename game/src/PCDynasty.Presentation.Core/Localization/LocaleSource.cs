namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Where a locale file came from. The registry in S3 tags every loaded
/// file with its source so a built-in language always wins over an
/// external one with the same id.
/// </summary>
public enum LocaleSource
{
    /// <summary>The file was written by the compile-time JSON generator (S9) and shipped with the binary.</summary>
    BuiltIn,

    /// <summary>The file was added to <c>res://locale/langs/</c> by a third party after the build.</summary>
    External,
}
