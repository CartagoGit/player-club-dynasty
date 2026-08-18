using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The shipped catalog must expose at least six palettes, the default must
/// be the one the shell already uses, and every palette must satisfy the
/// minimum contrast invariant the constructor enforces.
/// </summary>
public sealed class ThemePaletteCatalogTests
{
    [Fact]
    public void TheShippedCatalogListsAtLeastSixDistinctPalettes()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        Assert.True(catalog.Palettes.Count >= 6);
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (ThemePalette palette in catalog.Palettes)
        {
            Assert.True(ids.Add(palette.Id.Value), $"Duplicate palette id '{palette.Id.Value}' shipped.");
        }
    }

    [Fact]
    public void TheDefaultPaletteIsClassicDark()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        Assert.Equal(ThemePaletteId.ClassicDark, catalog.Default.Id);
    }

    [Fact]
    public void ResolvingAnUnknownIdReturnsTheDefault()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        ThemePalette resolved = catalog.Resolve(new ThemePaletteId("not-a-real-palette"));

        Assert.Equal(catalog.Default.Id, resolved.Id);
    }

    [Fact]
    public void ResolvingAnEmptyOrNullStringReturnsTheDefault()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        Assert.Equal(catalog.Default.Id, catalog.Resolve((string?)null).Id);
        Assert.Equal(catalog.Default.Id, catalog.Resolve(string.Empty).Id);
        Assert.Equal(catalog.Default.Id, catalog.Resolve("   ").Id);
    }

    [Fact]
    public void ResolvingARealIdReturnsThatPalette()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        ThemePalette paper = catalog.Resolve("paper-light");

        Assert.Equal("paper-light", paper.Id.Value);
    }

    [Fact]
    public void EveryShippedPaletteSatisfiesTheContrastInvariant()
    {
        ThemePaletteCatalog catalog = ThemePaletteCatalog.Shipped;

        foreach (ThemePalette palette in catalog.Palettes)
        {
            Assert.True(
                palette.TextPrimary.ContrastRatio(palette.Canvas) >= 4.5f,
                $"Palette '{palette.Id.Value}' primary text fails contrast on canvas.");
            Assert.True(
                palette.Accent.ContrastRatio(palette.Canvas) >= 4.5f,
                $"Palette '{palette.Id.Value}' accent fails contrast on canvas.");
        }
    }

    [Fact]
    public void AContrastingPaletteWithBadTokensIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new ThemePalette(
            id: new ThemePaletteId("bad"),
            displayName: "Bad",
            canvas: ThemeColor.FromHex("#FFFFFF"),
            navigation: ThemeColor.FromHex("#FFFFFF"),
            surface: ThemeColor.FromHex("#FFFFFF"),
            surfaceRaised: ThemeColor.FromHex("#FFFFFF"),
            border: ThemeColor.FromHex("#FFFFFF"),
            accent: ThemeColor.FromHex("#FFFFFF"),
            positive: ThemeColor.FromHex("#FFFFFF"),
            warning: ThemeColor.FromHex("#FFFFFF"),
            negative: ThemeColor.FromHex("#FFFFFF"),
            textPrimary: ThemeColor.FromHex("#FFFFFF"),
            textSecondary: ThemeColor.FromHex("#FFFFFF"),
            focus: ThemeColor.FromHex("#FFFFFF")));
    }

    [Fact]
    public void ConstructingAnEmptyCatalogIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new ThemePaletteCatalog(Array.Empty<ThemePalette>()));
    }

    [Fact]
    public void ConstructingACatalogWithDuplicateIdsIsRejected()
    {
        ThemePalette palette = ThemePaletteCatalog.Shipped.Default;

        Assert.Throws<ArgumentException>(() => new ThemePaletteCatalog([palette, palette]));
    }

    [Fact]
    public void ThemeColorParsesHexTokens()
    {
        ThemeColor opaque = ThemeColor.FromHex("#102C40");
        Assert.Equal(0x10 / 255f, opaque.R, 3);
        Assert.Equal(0x2C / 255f, opaque.G, 3);
        Assert.Equal(0x40 / 255f, opaque.B, 3);
        Assert.Equal(1f, opaque.A);

        ThemeColor withAlpha = ThemeColor.FromHex("#102C4080");
        Assert.Equal(0x80 / 255f, withAlpha.A, 3);
    }

    [Fact]
    public void ThemeColorRejectsMalformedHex()
    {
        Assert.Throws<FormatException>(() => ThemeColor.FromHex("not-hex"));
        Assert.Throws<FormatException>(() => ThemeColor.FromHex("#12345"));
    }
}
