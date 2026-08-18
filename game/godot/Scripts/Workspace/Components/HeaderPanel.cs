using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Workspace.Components;

/// <summary>
/// A title plate that opens every section of the workspace. The shell's design
/// system calls this the "title plate": a bordered panel with an eyebrow, a
/// title, an optional summary line and an optional trailing action button.
/// </summary>
/// <remarks>
/// The plate replaces the loose <c>Label</c> + <c>HSeparator</c> pair every
/// screen used to build. It is palette-aware (f00043) and exposes every
/// caption through <c>tr()</c> for f00043's locale switch. The action button
/// is built lazily so screens that don't need it can pass <c>null</c>.
/// </remarks>
public sealed partial class HeaderPanel : PanelContainer
{
    private readonly Label _eyebrow = new() { Name = "Eyebrow" };
    private readonly Label _title = new() { Name = "Title" };
    private readonly Label _summary = new() { Name = "Summary" };
    private readonly HBoxContainer _actions = new() { Name = "HeaderActions" };
    private readonly VBoxContainer _footer = new() { Name = "HeaderFooter" };
    private Button? _actionButton;

    public string ActionName { get; private set; } = "Action";

    /// <remarks>
    /// The children are built here rather than in <c>_Ready</c>: screens
    /// configure a header before adding it to the tree, and a panel that only
    /// exists after <c>_Ready</c> dropped every text it was handed, which is
    /// how the workspace ended up with blank header bands.
    /// </remarks>
    public HeaderPanel()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.TitlePlate());

        var column = new VBoxContainer { Name = "HeaderColumn" };
        column.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        AddChild(column);

        var band = new HBoxContainer { Name = "HeaderBand" };
        band.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        column.AddChild(band);

        var left = new VBoxContainer { Name = "HeaderLeft" };
        left.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        left.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        band.AddChild(left);

        _eyebrow.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        _eyebrow.AddThemeFontSizeOverride("font_size", 12);
        left.AddChild(_eyebrow);

        _title.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        _title.AddThemeFontSizeOverride("font_size", 22);
        left.AddChild(_title);

        _summary.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        _summary.AddThemeFontSizeOverride("font_size", 13);
        _summary.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        left.AddChild(_summary);

        _actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _actions.Alignment = BoxContainer.AlignmentMode.End;
        _actions.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        band.AddChild(_actions);

        _footer.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        column.AddChild(_footer);
    }

    /// <summary>Wires the three text fields. Pass empty strings to hide them.</summary>
    public void SetTexts(string eyebrow, string title, string summary)
    {
        _eyebrow.Visible = !string.IsNullOrEmpty(eyebrow);
        _eyebrow.Text = eyebrow;
        _title.Text = title;
        _summary.Visible = !string.IsNullOrEmpty(summary);
        _summary.Text = summary;
    }

    /// <summary>Wires the three text fields and formats the summary with the given args.</summary>
    public void SetTexts(string eyebrow, string title, string summaryKey, string[] summaryArgs)
    {
        _eyebrow.Visible = !string.IsNullOrEmpty(eyebrow);
        _eyebrow.Text = eyebrow;
        _title.Text = title;
        _summary.Visible = !string.IsNullOrEmpty(summaryKey);
        string translated = summaryKey;
        _summary.Text = summaryArgs is { Length: > 0 }
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, translated, summaryArgs)
            : translated;
    }

    /// <summary>
    /// Creates a single trailing action button. Pass <c>null</c> text to remove a
    /// previously configured action. The action's <c>Pressed</c> event is wired by
    /// the caller; this method just lays out the button.
    /// </summary>
    public void SetAction(string? text, string? tooltip, Action? onPressed, bool enabled = true)
    {
        if (_actionButton is not null)
        {
            _actions.RemoveChild(_actionButton);
            _actionButton.QueueFree();
            _actionButton = null;
        }
        if (text is null)
        {
            return;
        }
        _actionButton = new Button
        {
            Name = ActionName,
            Text = text,
            TooltipText = tooltip ?? string.Empty,
            Disabled = !enabled,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        if (onPressed is not null)
        {
            _actionButton.Pressed += onPressed;
        }
        _actions.AddChild(_actionButton);
    }

    /// <summary>The single action button, when one is configured. Useful for the smoke-boot to press.</summary>
    public Button? ActionButton => _actionButton;

    /// <summary>The summary label, exposed so screens can update it after a refresh.</summary>
    public Label Summary => _summary ?? throw new InvalidOperationException("HeaderPanel is not in the tree yet.");

    /// <summary>The footer VBox; a screen can append hints or trailing actions here.</summary>
    public VBoxContainer Footer => _footer ?? throw new InvalidOperationException("HeaderPanel is not in the tree yet.");
}
