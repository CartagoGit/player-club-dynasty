using System.Globalization;
using Godot;

namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The compact calendar block that sits in the middle of the context band.
/// </summary>
/// <remarks>
/// A manager answers "what day is it" a hundred times a session, and the
/// classic desk never made you leave a screen to find out: the date lived in
/// the middle of the top band, as a small torn-calendar plate with the weekday,
/// the day, the month and the year stacked. It is a permanent anchor, so it
/// never moves and never scrolls away.
/// </remarks>
public sealed partial class DatePlate : PanelContainer, IPaletteAware
{
    private readonly Label _weekday = new() { Name = "Weekday" };
    private readonly Label _day = new() { Name = "Day" };
    private readonly Label _month = new() { Name = "Month" };

    public DatePlate()
    {
        Name = "DatePlate";
        CustomMinimumSize = new Vector2(118, 0);

        var column = new VBoxContainer { Name = "DateColumn" };
        column.AddThemeConstantOverride("separation", 0);
        AddChild(column);

        _weekday.HorizontalAlignment = HorizontalAlignment.Center;
        _weekday.AddThemeFontSizeOverride("font_size", 11);
        column.AddChild(_weekday);

        _day.HorizontalAlignment = HorizontalAlignment.Center;
        _day.AddThemeFontSizeOverride("font_size", 20);
        column.AddChild(_day);

        _month.HorizontalAlignment = HorizontalAlignment.Center;
        _month.AddThemeFontSizeOverride("font_size", 11);
        column.AddChild(_month);

        Repaint();
    }

    /// <summary>Shows a date. The culture decides the weekday and month names.</summary>
    public void Show(DateOnly date, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        _weekday.Text = date.ToString("dddd", culture).ToUpperInvariant();
        _day.Text = date.ToString("dd", culture);
        _month.Text = date.ToString("MMM yyyy", culture).ToUpperInvariant();
    }

    /// <inheritdoc />
    public void OnPaletteChanged() => Repaint();

    private void Repaint()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.RaisedPlate(ManagerDesignTokens.Space4));
        _weekday.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        _day.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        _month.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
    }
}
