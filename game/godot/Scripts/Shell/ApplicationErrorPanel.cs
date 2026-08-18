using System.Collections.Immutable;
using System.Text;
using Godot;
using PCDynasty.Presentation.Core.Diagnostics;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// One modal dialog for every kind of error the game can show:
/// translation failures, content-load failures, unexpected
/// exceptions. The renderer is a thin pass-through over an
/// <see cref="ApplicationError"/>; the value is unit-tested and the
/// Godot side only wires the controls.
/// </summary>
/// <remarks>
/// The panel sits inside a dim overlay that fills the whole
/// viewport, with the dialog centred both horizontally and
/// vertically. A ScrollContainer with a fixed pixel cap holds the
/// details list so a file with hundreds of missing keys (or a
/// deep stack trace) does not overflow the viewport. A 'Copy
/// details' button writes a plain-text report to the system
/// clipboard so the player can paste it into a bug ticket.
/// </remarks>
public sealed partial class ApplicationErrorPanel : Control
{
    private readonly ApplicationError _error;
    private readonly IClipboardWriter _clipboard;
    private readonly string _reportText;

    /// <summary>Fired when the player taps the primary button.</summary>
    public event Action? PrimaryPressed;

    /// <summary>Fired when the player taps the secondary button (typically 'Copy details').</summary>
    public event Action? SecondaryPressed;

    /// <summary>Builds the panel from an <see cref="ApplicationError"/> shape.</summary>
    public ApplicationErrorPanel(ApplicationError error, IClipboardWriter? clipboard = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        _error = error;
        _clipboard = clipboard ?? new GodotClipboardWriter();
        _reportText = ApplicationErrorReport.Format(error);

        Name = $"ApplicationErrorPanel_{error.Severity}";
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        BuildOverlay();
        BuildDialog();
    }

    /// <summary>Returns the plain-text report the 'Copy details' button writes to the clipboard.</summary>
    public string ReportText => _reportText;

    private void BuildOverlay()
    {
        // Dim background covering the whole viewport. The dialog sits
        // on top of it, centred, so the rest of the UI is visibly
        // disabled while the panel is open.
        var overlay = new ColorRect
        {
            Name = "Overlay",
            Color = new Color(0f, 0f, 0f, 0.65f),
        };
        overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.MouseFilter = MouseFilterEnum.Stop;
        AddChild(overlay);
    }

    private void BuildDialog()
    {
        Color accent = _error.Severity == ApplicationErrorSeverity.Fatal
            ? ManagerDesignTokens.Accent
            : ManagerDesignTokens.TextSecondary;

        var dialog = new PanelContainer
        {
            Name = "Dialog",
            CustomMinimumSize = new Vector2(640, 0),
        };
        dialog.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);

        var root = new VBoxContainer { Name = "Root" };
        root.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        dialog.AddChild(root);

        // Severity badge + title in one row so the player sees both at a glance.
        var header = new HBoxContainer { Name = "Header" };
        header.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        var badge = new Label
        {
            Name = "SeverityBadge",
            Text = _error.Severity.ToString().ToUpperInvariant(),
        };
        badge.AddThemeColorOverride("font_color", accent);
        badge.AddThemeFontSizeOverride("font_size", 12);
        header.AddChild(badge);

        var title = new Label
        {
            Name = "Title",
            Text = _error.Title,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        title.AddThemeColorOverride("font_color", accent);
        title.AddThemeFontSizeOverride("font_size", 20);
        header.AddChild(title);
        root.AddChild(header);

        var body = new Label
        {
            Name = "Body",
            Text = _error.Body,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        body.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        body.AddThemeFontSizeOverride("font_size", 13);
        root.AddChild(body);

        AppendContextRow(root, accent);

        if (_error.Items.Length > 0)
        {
            AppendScrollableDetails(root, _error.Items);
        }

        var actions = new HBoxContainer { Name = "Actions" };
        actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        actions.Alignment = BoxContainer.AlignmentMode.End;

        if (_error.SecondaryButtonLabel is not null)
        {
            var secondary = new Button
            {
                Name = "SecondaryButton",
                Text = _error.SecondaryButtonLabel,
                CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            };
            secondary.Pressed += OnSecondaryPressed;
            actions.AddChild(secondary);
        }

        var primary = new Button
        {
            Name = "PrimaryButton",
            Text = _error.PrimaryButtonLabel,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        primary.Pressed += () => PrimaryPressed?.Invoke();
        actions.AddChild(primary);
        root.AddChild(actions);

        AddChild(dialog);
    }

    private void AppendContextRow(VBoxContainer root, Color accent)
    {
        bool hasPath = !string.IsNullOrEmpty(_error.FilePath);
        bool hasCode = !string.IsNullOrEmpty(_error.ErrorCode);
        if (!hasPath && !hasCode)
        {
            return;
        }

        var row = new HBoxContainer { Name = "ContextRow" };
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);

        if (hasCode)
        {
            var code = new Label
            {
                Name = "Code",
                Text = $"[{_error.ErrorCode}]",
            };
            code.AddThemeColorOverride("font_color", accent);
            row.AddChild(code);
        }

        if (hasPath)
        {
            var path = new Label
            {
                Name = "Path",
                Text = _error.FilePath!,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            path.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            row.AddChild(path);
        }

        root.AddChild(row);
    }

    private static void AppendScrollableDetails(
        VBoxContainer parent,
        ImmutableArray<ApplicationErrorItem> items)
    {
        var scroll = new ScrollContainer
        {
            Name = "DetailsScroll",
            CustomMinimumSize = new Vector2(0, ApplicationErrorBuilder.DefaultMaxItemsHeight),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };

        var list = new VBoxContainer { Name = "DetailsList" };
        list.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        scroll.AddChild(list);

        foreach (ApplicationErrorItem item in items)
        {
            string line = $"• <b>{item.Label}</b>: {item.Detail}";
            var rich = new RichTextLabel
            {
                Name = $"Detail_{item.Label}",
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
            };
            rich.AppendText(line);
            list.AddChild(rich);
        }

        parent.AddChild(scroll);
    }

    private void OnSecondaryPressed()
    {
        _clipboard.WriteText(_reportText);
        SecondaryPressed?.Invoke();
    }
}

/// <summary>Abstraction over the system clipboard so tests can pin the call.</summary>
public interface IClipboardWriter
{
    /// <summary>Writes <paramref name="text"/> to the system clipboard.</summary>
    void WriteText(string text);
}

/// <summary>Godot-backed clipboard writer (uses <see cref="DisplayServer.ClipboardSet"/>).</summary>
public sealed class GodotClipboardWriter : IClipboardWriter
{
    /// <inheritdoc/>
    public void WriteText(string text) => DisplayServer.ClipboardSet(text);
}
