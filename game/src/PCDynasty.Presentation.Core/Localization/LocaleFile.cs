using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The complete, validated shape of a single <c>xx.json</c> file in
/// <c>res://locale/langs/</c>. The C# record is the source of truth:
/// the compile-time JSON generator in S9 serialises it, the strict
/// loader in S2 deserialises it, and the runtime renders the result.
/// </summary>
/// <remarks>
/// The three root fields (<see cref="Lang"/>, <see cref="Icon"/>,
/// <see cref="Name"/>) are the metadata the registry and the picker
/// use to enumerate languages without loading the whole file. The
/// remaining properties are the per-section bundles the screens
/// read at runtime. Adding a section means adding a property here
/// AND a sub-record under <c>Localization/Sections</c>; the generator
/// and the loader will then surface the new shape automatically.
/// </remarks>
public sealed record LocaleFile(
    string Lang,
    string Icon,
    string Name,
    MenuSection Menu,
    OptionsSection Options,
    SquadSection Squad,
    MarketSection Market,
    HistorySection History,
    CompetitionsSection Competitions,
    WorldSection World,
    PickerSection Picker)
{
    /// <summary>
    /// Deserialises a <c>xx.json</c> document into a <see cref="LocaleFile"/>.
    /// Throws <see cref="JsonException"/> when the document is malformed;
    /// the strict loader catches it and returns a typed
    /// <see cref="LocaleLoadResult"/> instead of bubbling.
    /// </summary>
    public static LocaleFile FromJson(string json) =>
        JsonSerializer.Deserialize<LocaleFile>(json, LocalizationJson.Read)
        ?? throw new JsonException("Locale JSON is empty or returned null.");

    /// <summary>Serialises the file back to JSON in the canonical on-disk shape.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, LocalizationJson.Write);
}
