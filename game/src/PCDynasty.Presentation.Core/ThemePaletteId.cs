namespace PCDynasty.Presentation.Core;

/// <summary>
/// A stable identifier for a theme palette. The catalog stores palettes keyed
/// by these strings; the preferences file persists a raw string so future
/// palettes can be added without breaking older saves.
/// </summary>
public readonly record struct ThemePaletteId(string Value)
{
    /// <summary>The default palette the shell ships with.</summary>
    public static readonly ThemePaletteId ClassicDark = new("classic-dark");

    /// <summary>The classic dark palette. This is the fallback when an unknown id is requested.</summary>
    public static ThemePaletteId Default => ClassicDark;

    public override string ToString() => Value;

    public static implicit operator string(ThemePaletteId id) => id.Value;
}
