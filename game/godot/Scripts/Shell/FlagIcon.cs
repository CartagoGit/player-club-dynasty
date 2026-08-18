using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A Godot <see cref="Control"/> that renders a <see cref="FlagGeometry"/>
/// procedurally. The icon owns the geometry it was built with and is
/// re-rendered whenever its size changes so the flag stays sharp at any
/// scaling factor.
/// </summary>
/// <remarks>
/// No image asset ships with the icon: every rectangle and line is drawn
/// through the Godot <c>DrawRect</c> / <c>DrawLine</c> API. The geometry is
/// a 0..1 normalised space, so the icon is resolution-independent.
/// </remarks>
[Tool]
public sealed partial class FlagIcon : Control
{
    private FlagGeometry _geometry;

    public FlagIcon()
    {
        _geometry = new FlagGeometry(
            ThemeColor.FromHex("#888888"),
            ThemeColor.FromHex("#888888"),
            ThemeColor.FromHex("#888888"),
            Array.Empty<FlagOperation>());
        CustomMinimumSize = new Vector2(36, 24);
        QueueRedraw();
    }

    /// <summary>Replaces the geometry and asks the icon to redraw.</summary>
    public void SetGeometry(FlagGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        _geometry = geometry;
        QueueRedraw();
    }

    public override void _Draw()
    {
        Rect2 area = new(Vector2.Zero, Size);
        foreach (FlagOperation op in _geometry.Operations)
        {
            switch (op.Kind)
            {
                case FlagOperationKind.Rect:
                    DrawRect(
                        new Rect2(
                            area.Position + new Vector2(op.X * Size.X, op.Y * Size.Y),
                            new Vector2(op.Width * Size.X, op.Height * Size.Y)),
                        ManagerDesignTokens.ToGodot(op.Color),
                        filled: true);
                    break;
                case FlagOperationKind.Line:
                    DrawLine(
                        area.Position + new Vector2(op.X * Size.X, op.Y * Size.Y),
                        area.Position + new Vector2(op.EndX * Size.X, op.EndY * Size.Y),
                        ManagerDesignTokens.ToGodot(op.Color),
                        width: Mathf.Max(op.Width * Size.X, op.Thickness * Size.Y),
                        antialiased: true);
                    break;
            }
        }

        // Hairline border so the flag stays readable on similar-coloured backgrounds.
        DrawRect(
            area,
            ManagerDesignTokens.ToGodot(ManagerDesignTokens.ActivePalette.Border),
            filled: false,
            width: 1f);
    }
}
