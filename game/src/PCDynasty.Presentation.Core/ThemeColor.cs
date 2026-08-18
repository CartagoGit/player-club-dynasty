namespace PCDynasty.Presentation.Core;

/// <summary>
/// A sRGB colour used by the shell's design tokens. The values are stored as
/// 0–1 floats so the same model can drive both StyleBoxFlat overrides and the
/// <c>Theme</c> overrides that the Godot render tree consumes.
/// </summary>
/// <remarks>
/// The struct is deliberately tiny and value-typed: a palette is an immutable
/// collection of these, so two palettes with the same components compare equal
/// and the catalog can deduplicate by structural equality.
/// </remarks>
public readonly record struct ThemeColor(float R, float G, float B, float A = 1f)
{
    /// <summary>Parses a hex string in the form <c>#RRGGBB</c> or <c>#RRGGBBAA</c>.</summary>
    public static ThemeColor FromHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);
        string normalised = hex.Trim().TrimStart('#');
        if (normalised.Length is not (6 or 8))
        {
            throw new FormatException($"Theme colour must be 6 or 8 hex digits; got '{hex}'.");
        }

        float r = ParseByte(normalised.AsSpan(0, 2)) / 255f;
        float g = ParseByte(normalised.AsSpan(2, 2)) / 255f;
        float b = ParseByte(normalised.AsSpan(4, 2)) / 255f;
        float a = normalised.Length == 8 ? ParseByte(normalised.AsSpan(6, 2)) / 255f : 1f;
        return new ThemeColor(r, g, b, a);
    }

    /// <summary>Relative luminance used to verify min-text contrast against backgrounds.</summary>
    public float RelativeLuminance()
    {
        float Channel(float c) => c <= 0.03928f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * Channel(R) + 0.7152f * Channel(G) + 0.0722f * Channel(B);
    }

    /// <summary>WCAG contrast ratio between this colour and <paramref name="other"/>.</summary>
    public float ContrastRatio(ThemeColor other)
    {
        float l1 = RelativeLuminance();
        float l2 = other.RelativeLuminance();
        float lighter = MathF.Max(l1, l2);
        float darker = MathF.Min(l1, l2);
        return (lighter + 0.05f) / (darker + 0.05f);
    }

    private static int ParseByte(ReadOnlySpan<char> slice) =>
        int.Parse(slice, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
}
