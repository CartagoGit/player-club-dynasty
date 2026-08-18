using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A small rectangular preview of a <see cref="ThemePalette"/>: the canvas
/// colour with the accent border and a couple of text rows. The swatch is
/// drawn the same way the rest of the shell is — no image assets — and
/// refreshes whenever the palette changes.
/// </summary>
[Tool]
public sealed partial class PaletteSwatch : Control
{
    private ThemePalette _palette;

    public PaletteSwatch()
    {
        _palette = ManagerDesignTokens.ActivePalette;
        CustomMinimumSize = new Vector2(132, 60);
        QueueRedraw();
    }

    public void SetPalette(ThemePalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        _palette = palette;
        QueueRedraw();
    }

    public override void _Draw()
    {
        Rect2 area = new(Vector2.Zero, Size);
        Rect2 canvas = new(area.Position + new Vector2(8, 8), new Vector2(area.Size.X - 16, area.Size.Y - 16));

        // Background canvas.
        DrawRect(canvas, ManagerDesignTokens.ToGodot(_palette.Canvas), filled: true);

        // Accent bar on the left edge to show the highlight colour.
        DrawRect(
            new Rect2(canvas.Position, new Vector2(4, canvas.Size.Y)),
            ManagerDesignTokens.ToGodot(_palette.Accent),
            filled: true);

        // Two "text" lines using the primary and secondary text tokens.
        DrawRect(
            new Rect2(canvas.Position + new Vector2(12, 12), new Vector2(canvas.Size.X - 24, 4)),
            ManagerDesignTokens.ToGodot(_palette.TextPrimary),
            filled: true);
        DrawRect(
            new Rect2(canvas.Position + new Vector2(12, 22), new Vector2(canvas.Size.X - 36, 4)),
            ManagerDesignTokens.ToGodot(_palette.TextSecondary),
            filled: true);

        // Surface row at the bottom.
        DrawRect(
            new Rect2(canvas.Position + new Vector2(0, canvas.Size.Y - 12), new Vector2(canvas.Size.X, 12)),
            ManagerDesignTokens.ToGodot(_palette.Surface),
            filled: true);

        // Hairline border.
        DrawRect(canvas, ManagerDesignTokens.ToGodot(_palette.Border), filled: false, width: 1f);
    }
}
