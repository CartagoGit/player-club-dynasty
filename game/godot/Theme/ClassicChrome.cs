using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The bordered plates, bevels and strips every shell surface is built from.
/// </summary>
/// <remarks>
/// The look is the interface grammar of a nineties desktop manager — plates
/// with a lit top edge and a shadowed bottom one, a gold rule under the header,
/// dense rows — expressed entirely with this project's own tokens. Screens ask
/// for a named surface here instead of assembling a StyleBoxFlat each time,
/// which is what let three slightly different panel styles drift apart.
/// </remarks>
public static class ClassicChrome
{
    private static readonly Color BevelLight = new(1f, 1f, 1f, 0.10f);
    private static readonly Color BevelShadow = new(0f, 0f, 0f, 0.35f);

    /// <summary>A framed plate: the default container for any block of content.</summary>
    public static StyleBoxFlat Plate(int padding = ManagerDesignTokens.Space12) =>
        new()
        {
            BgColor = ManagerDesignTokens.Surface,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = padding + ManagerDesignTokens.Space4,
            ContentMarginTop = padding,
            ContentMarginRight = padding + ManagerDesignTokens.Space4,
            ContentMarginBottom = padding,
        };

    /// <summary>The title plate of the main menu: raised, gold-edged, square.</summary>
    public static StyleBoxFlat TitlePlate() =>
        new()
        {
            BgColor = ManagerDesignTokens.Navigation,
            BorderColor = ManagerDesignTokens.Accent,
            BorderWidthLeft = 3,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 2,
            ContentMarginLeft = ManagerDesignTokens.Space24,
            ContentMarginTop = ManagerDesignTokens.Space16,
            ContentMarginRight = ManagerDesignTokens.Space24,
            ContentMarginBottom = ManagerDesignTokens.Space16,
        };

    /// <summary>A recessed well for tables and lists.</summary>
    public static StyleBoxFlat Inset() =>
        new()
        {
            BgColor = ManagerDesignTokens.Canvas,
            BorderColor = BevelShadow,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = ManagerDesignTokens.Space8,
            ContentMarginTop = ManagerDesignTokens.Space4,
            ContentMarginRight = ManagerDesignTokens.Space8,
            ContentMarginBottom = ManagerDesignTokens.Space4,
        };

    /// <summary>The band that carries club, date and money above every screen.</summary>
    public static StyleBoxFlat HeaderBand() =>
        new()
        {
            BgColor = ManagerDesignTokens.Navigation,
            BorderColor = ManagerDesignTokens.Accent,
            BorderWidthBottom = 2,
            BorderWidthTop = 1,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space8,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space8,
        };

    /// <summary>The strip along the bottom that always shows the last message.</summary>
    public static StyleBoxFlat StatusStrip() =>
        new()
        {
            BgColor = ManagerDesignTokens.Navigation,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthTop = 1,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space4,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space4,
        };

    /// <summary>A rail entry; the active one carries the gold edge.</summary>
    public static StyleBoxFlat RailEntry(bool active) =>
        new()
        {
            BgColor = active ? ManagerDesignTokens.SurfaceRaised : ManagerDesignTokens.Navigation,
            BorderColor = active ? ManagerDesignTokens.Accent : BevelLight,
            BorderWidthLeft = active ? 4 : 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = ManagerDesignTokens.Space12,
            ContentMarginTop = ManagerDesignTokens.Space8,
            ContentMarginRight = ManagerDesignTokens.Space12,
            ContentMarginBottom = ManagerDesignTokens.Space8,
        };

    /// <summary>The club crest placeholder: initials on a coloured disc.</summary>
    public static Control CrestPlaceholder(string clubName, int size = 34)
    {
        var crest = new PanelContainer
        {
            Name = "Crest",
            CustomMinimumSize = new Vector2(size, size),
        };
        crest.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.SurfaceRaised,
            BorderColor = ManagerDesignTokens.Accent,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = size / 2,
            CornerRadiusTopRight = size / 2,
            CornerRadiusBottomRight = size / 2,
            CornerRadiusBottomLeft = size / 2,
        });

        var initials = new Label
        {
            Name = "Initials",
            Text = Initials(clubName),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        initials.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        initials.AddThemeFontSizeOverride("font_size", 13);
        crest.AddChild(initials);
        return crest;
    }

    private static string Initials(string clubName)
    {
        string[] words = clubName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length switch
        {
            0 => "··",
            1 => words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant(),
            _ => $"{words[0][0]}{words[^1][0]}".ToUpperInvariant(),
        };
    }
}
