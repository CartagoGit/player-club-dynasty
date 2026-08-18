using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// JsonSerializer options shared by every locale file. camelCase is the
/// on-disk convention, missing fields are tolerated on read so a
/// future bundle can be added without breaking every existing
/// language, and the writer is indented for diff-friendly commits.
/// </summary>
internal static class LocalizationJson
{
    public static readonly JsonSerializerOptions Read = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static readonly JsonSerializerOptions Write = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
