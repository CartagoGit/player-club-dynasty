using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Workspace.Components;

/// <summary>
/// The band above every screen: whose club this is, what day it is, and what
/// the club can spend.
/// </summary>
/// <remarks>
/// A classic manager never makes you leave the screen to answer those three
/// questions; audit a00001 recorded their absence as a gap in the workspace.
/// The header renders a projection and owns no rules.
/// </remarks>
public sealed partial class WorkspaceHeader : PanelContainer
{
    private Label? _club;
    private Label? _date;
    private Label? _money;
    private Control? _crest;
    private HBoxContainer? _row;

    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.HeaderBand());
        _row = new HBoxContainer { Name = "HeaderRow" };
        _row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddChild(_row);

        _club = Caption("Club", 18, ManagerDesignTokens.TextPrimary);
        _row.AddChild(_club);

        var spacer = new Control { Name = "HeaderSpacer", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _row.AddChild(spacer);

        _date = Caption("Date", 14, ManagerDesignTokens.TextSecondary);
        _row.AddChild(_date);
        _row.AddChild(new VSeparator());
        _money = Caption("Money", 14, ManagerDesignTokens.Accent);
        _row.AddChild(_money);
    }

    /// <summary>Shows the managed club, the career date and the club's cash.</summary>
    public void Show(ClubRow club, CareerCalendar calendar, string money)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(calendar);
        if (_row is null || _club is null || _date is null || _money is null)
        {
            return;
        }

        if (_crest is not null)
        {
            _row.RemoveChild(_crest);
            _crest.QueueFree();
        }

        _crest = ClassicChrome.CrestPlaceholder(club.DisplayName);
        _row.AddChild(_crest);
        _row.MoveChild(_crest, 0);

        _club.Text = $"{club.DisplayName.ToUpperInvariant()} · {club.City}";
        _date.Text = calendar.CurrentDate.ToString("dd MMM yyyy", WorkspaceUiFactory.InvariantCulture)
            .ToUpperInvariant();
        _money.Text = money;
    }

    private static Label Caption(string name, int fontSize, Color color)
    {
        var label = new Label { Name = name, VerticalAlignment = VerticalAlignment.Center };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        return label;
    }
}
