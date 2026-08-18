namespace PCDynasty.Presentation.Core;

/// <summary>
/// One language the shell can render in. The entry carries the display name,
/// the Godot locale code (used by <c>TranslationServer.SetLocale</c>) and the
/// procedural flag geometry surfaced by the options panel.
/// </summary>
public sealed class LocaleEntry
{
    public LocaleId Id { get; }
    public string DisplayName { get; }
    public string NativeDisplayName { get; }
    public string CountryName { get; }
    public string GodotLocaleCode { get; }
    public FlagGeometry Flag { get; }

    public LocaleEntry(
        LocaleId id,
        string displayName,
        string nativeDisplayName,
        string countryName,
        string godotLocaleCode,
        FlagGeometry flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        if (string.IsNullOrWhiteSpace(godotLocaleCode))
        {
            throw new ArgumentException("Godot locale code must be non-empty.", nameof(godotLocaleCode));
        }

        Id = id;
        DisplayName = displayName;
        NativeDisplayName = nativeDisplayName;
        CountryName = countryName;
        GodotLocaleCode = godotLocaleCode;
        Flag = flag;
    }
}
