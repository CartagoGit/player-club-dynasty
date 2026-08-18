namespace PCDynasty.Presentation.Core;

/// <summary>
/// The ordered, read-only list of palettes the shell can render with. The
/// catalog is the single source of truth for the available options; the
/// preferences layer resolves an id through it so unknown ids fall back to the
/// default instead of throwing.
/// </summary>
public sealed class ThemePaletteCatalog
{
    /// <summary>The shipped palettes, in the order they appear in the picker.</summary>
    public IReadOnlyList<ThemePalette> Palettes { get; }

    public ThemePaletteCatalog(IEnumerable<ThemePalette> palettes)
    {
        ArgumentNullException.ThrowIfNull(palettes);
        ThemePalette[] array = palettes.ToArray();
        if (array.Length == 0)
        {
            throw new ArgumentException("Catalog must contain at least one palette.", nameof(palettes));
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (ThemePalette palette in array)
        {
            if (!seen.Add(palette.Id.Value))
            {
                throw new ArgumentException(
                    $"Duplicate palette id '{palette.Id.Value}' in catalog.",
                    nameof(palettes));
            }
        }

        Palettes = array;
    }

    /// <summary>The palette the shell starts with when no preference is stored.</summary>
    public ThemePalette Default => Palettes[0];

    /// <summary>Returns the palette whose id matches <paramref name="id"/>, or the default when the id is unknown.</summary>
    public ThemePalette Resolve(ThemePaletteId id)
    {
        foreach (ThemePalette palette in Palettes)
        {
            if (palette.Id == id)
            {
                return palette;
            }
        }

        return Default;
    }

    /// <summary>Returns the palette whose id matches the raw string, or the default when the string is null, empty or unknown.</summary>
    public ThemePalette Resolve(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Default;
        }

        return Resolve(new ThemePaletteId(id.Trim()));
    }

    /// <summary>The default catalog the options panel displays.</summary>
    public static ThemePaletteCatalog Shipped { get; } = new(
    [
        ClassicDarkPalette.Create(),
        PaperLightPalette.Create(),
        DeepBluePalette.Create(),
        ForestPalette.Create(),
        SunsetPalette.Create(),
        InkPalette.Create(),
    ]);
}

/// <summary>The classic dark navy palette the shell shipped with.</summary>
internal static class ClassicDarkPalette
{
    public static ThemePalette Create() => new(
        id: ThemePaletteId.ClassicDark,
        displayName: "Clásico Oscuro",
        canvas: ThemeColor.FromHex("#0B1724"),
        navigation: ThemeColor.FromHex("#102C40"),
        surface: ThemeColor.FromHex("#163348"),
        surfaceRaised: ThemeColor.FromHex("#1C4057"),
        border: ThemeColor.FromHex("#31576B"),
        accent: ThemeColor.FromHex("#E3B341"),
        positive: ThemeColor.FromHex("#58C69A"),
        warning: ThemeColor.FromHex("#F09558"),
        negative: ThemeColor.FromHex("#EB6B73"),
        textPrimary: ThemeColor.FromHex("#F1F6F9"),
        textSecondary: ThemeColor.FromHex("#A8C0CD"),
        focus: ThemeColor.FromHex("#70C7E8"));
}

/// <summary>A cream-paper palette that reads like a printed page.</summary>
internal static class PaperLightPalette
{
    public static ThemePalette Create() => new(
        id: new ThemePaletteId("paper-light"),
        displayName: "Blanco Papel",
        canvas: ThemeColor.FromHex("#F4EFE6"),
        navigation: ThemeColor.FromHex("#E8DFD0"),
        surface: ThemeColor.FromHex("#FFFCF4"),
        surfaceRaised: ThemeColor.FromHex("#F8F4EC"),
        border: ThemeColor.FromHex("#B8A88F"),
        accent: ThemeColor.FromHex("#8B5A1F"),
        positive: ThemeColor.FromHex("#2D7A4F"),
        warning: ThemeColor.FromHex("#B86A1F"),
        negative: ThemeColor.FromHex("#A02838"),
        textPrimary: ThemeColor.FromHex("#1A1612"),
        textSecondary: ThemeColor.FromHex("#5A4E3F"),
        focus: ThemeColor.FromHex("#2E6BA8"));
}

/// <summary>A deep, saturated blue palette that leans into the maritime look of the genre.</summary>
internal static class DeepBluePalette
{
    public static ThemePalette Create() => new(
        id: new ThemePaletteId("deep-blue"),
        displayName: "Azul Profundo",
        canvas: ThemeColor.FromHex("#060F1F"),
        navigation: ThemeColor.FromHex("#0B1F3A"),
        surface: ThemeColor.FromHex("#143357"),
        surfaceRaised: ThemeColor.FromHex("#1E4470"),
        border: ThemeColor.FromHex("#2E5A8A"),
        accent: ThemeColor.FromHex("#5BB8E8"),
        positive: ThemeColor.FromHex("#5FE0A8"),
        warning: ThemeColor.FromHex("#FFB05C"),
        negative: ThemeColor.FromHex("#FF7080"),
        textPrimary: ThemeColor.FromHex("#F0F4F8"),
        textSecondary: ThemeColor.FromHex("#9FB8D4"),
        focus: ThemeColor.FromHex("#FFCB52"));
}

/// <summary>A forest-green palette that pushes the positive colour forward and warms the accent.</summary>
internal static class ForestPalette
{
    public static ThemePalette Create() => new(
        id: new ThemePaletteId("forest"),
        displayName: "Bosque",
        canvas: ThemeColor.FromHex("#0F1A12"),
        navigation: ThemeColor.FromHex("#1A2E1F"),
        surface: ThemeColor.FromHex("#243F2A"),
        surfaceRaised: ThemeColor.FromHex("#2F5436"),
        border: ThemeColor.FromHex("#4A6D4F"),
        accent: ThemeColor.FromHex("#D4A050"),
        positive: ThemeColor.FromHex("#7AC862"),
        warning: ThemeColor.FromHex("#E89B4A"),
        negative: ThemeColor.FromHex("#D8504E"),
        textPrimary: ThemeColor.FromHex("#F0E8D8"),
        textSecondary: ThemeColor.FromHex("#B5C8A8"),
        focus: ThemeColor.FromHex("#88C4E0"));
}

/// <summary>A warm sunset palette that turns the navy ground into plum and the accent into orange.</summary>
internal static class SunsetPalette
{
    public static ThemePalette Create() => new(
        id: new ThemePaletteId("sunset"),
        displayName: "Atardecer",
        canvas: ThemeColor.FromHex("#1F0F1A"),
        navigation: ThemeColor.FromHex("#2F1A2A"),
        surface: ThemeColor.FromHex("#4A2840"),
        surfaceRaised: ThemeColor.FromHex("#5C3450"),
        border: ThemeColor.FromHex("#8A4A6E"),
        accent: ThemeColor.FromHex("#FF8C4A"),
        positive: ThemeColor.FromHex("#6BD89B"),
        warning: ThemeColor.FromHex("#FFB05C"),
        negative: ThemeColor.FromHex("#FF6E80"),
        textPrimary: ThemeColor.FromHex("#FFF0E8"),
        textSecondary: ThemeColor.FromHex("#E8B8C8"),
        focus: ThemeColor.FromHex("#FFCB52"));
}

/// <summary>A monochrome palette that strips the chrome down to black and white.</summary>
internal static class InkPalette
{
    public static ThemePalette Create() => new(
        id: new ThemePaletteId("ink"),
        displayName: "Tinta",
        canvas: ThemeColor.FromHex("#0A0A0A"),
        navigation: ThemeColor.FromHex("#161616"),
        surface: ThemeColor.FromHex("#1F1F1F"),
        surfaceRaised: ThemeColor.FromHex("#2A2A2A"),
        border: ThemeColor.FromHex("#404040"),
        accent: ThemeColor.FromHex("#C0C0C0"),
        positive: ThemeColor.FromHex("#6BD89B"),
        warning: ThemeColor.FromHex("#FFB05C"),
        negative: ThemeColor.FromHex("#FF6E80"),
        textPrimary: ThemeColor.FromHex("#F5F5F5"),
        textSecondary: ThemeColor.FromHex("#B0B0B0"),
        focus: ThemeColor.FromHex("#FFFFFF"));
}
