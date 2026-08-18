namespace PCDynasty.Presentation.Core;

/// <summary>
/// The list of languages the shell can render in. The catalog is the single
/// source of truth for the options panel and for the
/// <c>TranslationServer</c> lookup; an unknown id always falls back to the
/// default instead of throwing.
/// </summary>
public sealed class LocaleCatalog
{
    public IReadOnlyList<LocaleEntry> Locales { get; }

    public LocaleCatalog(IEnumerable<LocaleEntry> locales)
    {
        ArgumentNullException.ThrowIfNull(locales);
        LocaleEntry[] array = locales.ToArray();
        if (array.Length == 0)
        {
            throw new ArgumentException("Catalog must contain at least one locale.", nameof(locales));
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (LocaleEntry entry in array)
        {
            if (!seen.Add(entry.Id.Value))
            {
                throw new ArgumentException(
                    $"Duplicate locale id '{entry.Id.Value}' in catalog.",
                    nameof(locales));
            }
        }

        Locales = array;
    }

    public LocaleEntry Default => Locales[0];

    public LocaleEntry Resolve(LocaleId id)
    {
        foreach (LocaleEntry entry in Locales)
        {
            if (entry.Id == id)
            {
                return entry;
            }
        }

        return Default;
    }

    public LocaleEntry Resolve(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Default;
        }

        return Resolve(new LocaleId(id.Trim()));
    }

    /// <summary>The default catalog the options panel displays.</summary>
    public static LocaleCatalog Shipped { get; } = new(
    [
        SpanishFlag.Build(),
        BritishFlag.Build(),
    ]);
}

/// <summary>The Spanish flag: three horizontal stripes, red–yellow–red, with the yellow twice as tall.</summary>
internal static class SpanishFlag
{
    public static LocaleEntry Build() => new(
        id: LocaleId.Spanish,
        displayName: "Español",
        nativeDisplayName: "Español",
        countryName: "España",
        godotLocaleCode: "es",
        flag: new FlagGeometry(
            primary: ThemeColor.FromHex("#AA151B"),
            secondary: ThemeColor.FromHex("#F1BF00"),
            tertiary: ThemeColor.FromHex("#AA151B"),
            operations:
            [
                FlagOperation.Rect(ThemeColor.FromHex("#AA151B"), 0f, 0f, 1f, 0.25f),
                FlagOperation.Rect(ThemeColor.FromHex("#F1BF00"), 0f, 0.25f, 1f, 0.5f),
                FlagOperation.Rect(ThemeColor.FromHex("#AA151B"), 0f, 0.75f, 1f, 0.25f),
            ]));
}

/// <summary>
/// The Union Jack, simplified to a recognisable stylisation: a blue ground,
/// a white cross, a red cross on top of the white, a white saltire and a
/// red saltire on top of the white.
/// </summary>
internal static class BritishFlag
{
    public static LocaleEntry Build() => new(
        id: LocaleId.English,
        displayName: "English",
        nativeDisplayName: "English",
        countryName: "United Kingdom",
        godotLocaleCode: "en",
        flag: new FlagGeometry(
            primary: ThemeColor.FromHex("#012169"),
            secondary: ThemeColor.FromHex("#FFFFFF"),
            tertiary: ThemeColor.FromHex("#C8102E"),
            operations: BuildUnionJack()));

    private static IEnumerable<FlagOperation> BuildUnionJack()
    {
        ThemeColor blue = ThemeColor.FromHex("#012169");
        ThemeColor white = ThemeColor.FromHex("#FFFFFF");
        ThemeColor red = ThemeColor.FromHex("#C8102E");

        // The blue ground.
        yield return FlagOperation.Rect(blue, 0f, 0f, 1f, 1f);

        // The white saltire (X), drawn corner to corner.
        yield return FlagOperation.Line(white, 0f, 0f, 1f, 1f, 0.12f);
        yield return FlagOperation.Line(white, 1f, 0f, 0f, 1f, 0.12f);

        // The red saltire on top of the white, narrower.
        yield return FlagOperation.Line(red, 0f, 0f, 1f, 1f, 0.06f);
        yield return FlagOperation.Line(red, 1f, 0f, 0f, 1f, 0.06f);

        // The white cross (St. Andrew's cross).
        yield return FlagOperation.Rect(white, 0.40f, 0f, 0.20f, 1f);
        yield return FlagOperation.Rect(white, 0f, 0.40f, 1f, 0.20f);

        // The red cross (St. George's cross) on top of the white, narrower.
        yield return FlagOperation.Rect(red, 0.45f, 0f, 0.10f, 1f);
        yield return FlagOperation.Rect(red, 0f, 0.45f, 1f, 0.10f);
    }
}
