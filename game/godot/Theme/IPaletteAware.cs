namespace PCDynasty.Presentation.Theme;

/// <summary>
/// Implemented by a control that builds colours the palette applier cannot
/// infer from a token — a blend, a tint, an area colour — and therefore has to
/// repaint itself when the palette changes.
/// </summary>
/// <remarks>
/// <see cref="PCDynasty.Presentation.Shell.ThemePaletteApplier"/> recolours a
/// <c>StyleBoxFlat</c> by matching its current colour against the palette's
/// twelve tokens. A derived colour matches none of them, so it would silently
/// stay on the previous palette. Rather than teach the applier every blend the
/// shell might invent, the control that produced the colour owns its own
/// repaint: the applier walks the tree once and calls this.
/// </remarks>
public interface IPaletteAware
{
    /// <summary>Rebuilds every colour this control derives from the palette.</summary>
    void OnPaletteChanged();
}
