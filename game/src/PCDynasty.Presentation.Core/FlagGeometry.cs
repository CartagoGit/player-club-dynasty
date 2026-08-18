namespace PCDynasty.Presentation.Core;

/// <summary>
/// A graphical description of a country flag rendered procedurally by
/// <c>FlagIcon</c>. The geometry is a list of axis-aligned rectangles and
/// diagonal lines in a 0..1 normalised coordinate space, so the icon can
/// scale to any size without losing crispness.
/// </summary>
/// <remarks>
/// The shape is a list of draw operations rather than a fixed bitmap so the
/// icons scale with the rest of the UI and stay sharp on high-density
/// displays. The geometry is purely declarative: the Godot <c>Control</c>
/// reads the operations and renders them with a single <c>DrawRect</c> or
/// <c>DrawLine</c> per operation.
/// </remarks>
public sealed class FlagGeometry
{
    /// <summary>Three palette colours the flag uses (the third may be unused for two-colour flags).</summary>
    public ThemeColor Primary { get; }

    public ThemeColor Secondary { get; }

    public ThemeColor Tertiary { get; }

    /// <summary>The draw operations, in back-to-front order. Coordinates are in 0..1 space.</summary>
    public IReadOnlyList<FlagOperation> Operations { get; }

    public FlagGeometry(ThemeColor primary, ThemeColor secondary, ThemeColor tertiary, IEnumerable<FlagOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Primary = primary;
        Secondary = secondary;
        Tertiary = tertiary;
        Operations = operations.ToArray();
    }
}

/// <summary>One draw operation: either a solid rectangle or a line with a thickness.</summary>
public readonly record struct FlagOperation(FlagOperationKind Kind, ThemeColor Color, float X, float Y, float Width, float Height, float EndX, float EndY, float Thickness)
{
    public static FlagOperation Rect(ThemeColor color, float x, float y, float width, float height) =>
        new(FlagOperationKind.Rect, color, x, y, width, height, 0f, 0f, 0f);

    public static FlagOperation Line(ThemeColor color, float fromX, float fromY, float toX, float toY, float thickness) =>
        new(FlagOperationKind.Line, color, fromX, fromY, 0f, 0f, toX, toY, thickness);
}

public enum FlagOperationKind
{
    Rect = 0,
    Line = 1,
}
