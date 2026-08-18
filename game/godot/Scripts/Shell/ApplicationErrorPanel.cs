using System.Collections.Immutable;
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
/// Layout:
///
/// <list type="bullet">
///   <item><see cref="Control"/> fills the viewport with a dim overlay so the rest of the
///         UI is visibly disabled while the panel is open.</item>
///   <item>A <see cref="CenterContainer"/> centres the dialog both horizontally and vertically,
///         regardless of how the panel was added to the scene tree.</item>
///   <item>The dialog is a <see cref="PanelContainer"/> with a top status strip
///         (severity badge + title), the body, an optional code/path block, an
///         optional scrollable details list (only when there are items), and
///         a button bar with 'Copy details' + a primary action.</item>
///   <item>Paths are translated from <c>res://...</c> to absolute filesystem
///         paths via <see cref="ResPath.Globalize"/> so the player can
///         open the file in their file browser.</item>
///   <item>'Copy details' writes a plain-text report (severity, title, body,
///         path, code, items) to the system clipboard so the player can
///         paste it into a bug ticket.</item>
/// </list>
/// </remarks>
public sealed partial class ApplicationErrorPanel : Control
{
    private readonly ApplicationError _error;
    private readonly IClipboardWriter _clipboard;
    private readonly string _reportText;
    private readonly Godot.Button _primary;
    private readonly Godot.Button? _secondary;

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
        MouseFilter = MouseFilterEnum.Stop;

        BuildOverlay();
        BuildDialog(out _primary, out _secondary);
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

    private void BuildDialog(out Godot.Button primary, out Godot.Button? secondary)
    {
        // CenterContainer is the only Godot node that actually
        // centres a single child reliably: it computes the dialog's
        // size from the child's minimum size and places it in the
        // middle of the viewport. LayoutPreset.Center alone on a
        // child of a Control collapses to a 0x0 point, so we wrap.
        var center = new CenterContainer
        {
            Name = "DialogCenter",
            MouseFilter = MouseFilterEnum.Stop,
        };
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var dialog = new PanelContainer
        {
            Name = "Dialog",
            CustomMinimumSize = new Vector2(640, 0),
            MouseFilter = MouseFilterEnum.Stop,
        };

        Color accent = _error.Severity == ApplicationErrorSeverity.Fatal
            ? ManagerDesignTokens.Accent
            : ManagerDesignTokens.TextSecondary;

        var root = new VBoxContainer { Name = "Root" };
        root.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        dialog.AddChild(root);

        BuildHeader(root, accent);
        BuildBody(root);
        BuildContextPanel(root);
        if (_error.Items.Length > 0)
        {
            BuildDetailsScroll(root, _error.Items);
        }
        (primary, secondary) = BuildActionBar(root);

        center.AddChild(dialog);
    }

    private void BuildHeader(VBoxContainer root, Color accent)
    {
        // Severity strip: a coloured bar with the badge on the left
        // and the title on the right. The strip is the visual anchor
        // — when the player opens the panel they see severity first,
        // body second.
        var strip = new PanelContainer
        {
            Name = "HeaderStrip",
        };
        strip.AddThemeStyleboxOverride("panel", SeverityStyleBox(_error.Severity));

        var header = new HBoxContainer { Name = "Header" };
        header.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        var badge = new Label
        {
            Name = "SeverityBadge",
            Text = SeverityGlyph(_error.Severity) + " " + _error.Severity.ToString().ToUpperInvariant(),
        };
        badge.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        badge.AddThemeFontSizeOverride("font_size", 13);
        header.AddChild(badge);

        var title = new Label
        {
            Name = "Title",
            Text = _error.Title,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        title.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        title.AddThemeFontSizeOverride("font_size", 18);
        header.AddChild(title);
        strip.AddChild(header);
        root.AddChild(strip);
    }

    private void BuildBody(VBoxContainer root)
    {
        var body = new Label
        {
            Name = "Body",
            Text = _error.Body,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        body.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        body.AddThemeFontSizeOverride("font_size", 13);
        root.AddChild(body);
    }

    private void BuildContextPanel(VBoxContainer root)
    {
        bool hasCode = !string.IsNullOrEmpty(_error.ErrorCode);
        bool hasFilePath = !string.IsNullOrEmpty(_error.FilePath);
        if (!hasCode && !hasFilePath)
        {
            return;
        }

        var box = new VBoxContainer { Name = "Context" };
        box.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);

        if (hasCode)
        {
            AppendMonospaceRow(box, "Code", _error.ErrorCode!);
        }

        // Show the absolute filesystem path when we have it; fall
        // back to the res:// path only when the platform could not
        // resolve it. Either way the player can paste the path into
        // a bug ticket.
        if (hasFilePath)
        {
            string physical = ResPath.Globalize(_error.FilePath!, GodotResPathResolver.Globalize);
            string display = physical == _error.FilePath
                ? "Cannot resolve physical path"
                : physical;

            AppendMonospaceRow(box, "File", display);
            if (physical != _error.FilePath)
            {
                AppendMonospaceRow(box, "Resource", _error.FilePath!, subdued: true);
            }
        }

        root.AddChild(box);
    }

    private static void BuildDetailsScroll(VBoxContainer root, ImmutableArray<ApplicationErrorItem> items)
    {
        // The scroll container is built only when there are items to
        // show, so the empty-state panel never paints a stray
        // scrollbar.
        var sectionHeader = new Label
        {
            Name = "DetailsHeader",
            Text = $"Details ({items.Length})",
        };
        sectionHeader.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        sectionHeader.AddThemeFontSizeOverride("font_size", 12);
        root.AddChild(sectionHeader);

        var scroll = new ScrollContainer
        {
            Name = "DetailsScroll",
            CustomMinimumSize = new Vector2(0, ApplicationErrorBuilder.DefaultMaxItemsHeight),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };

        var list = new VBoxContainer { Name = "DetailsList" };
        list.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        scroll.AddChild(list);

        foreach (ApplicationErrorItem item in items)
        {
            string line = $"• <b>{EscapeBbcode(item.Label)}</b>: {EscapeBbcode(item.Detail)}";
            var rich = new RichTextLabel
            {
                Name = "Detail",
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            rich.AppendText(line);
            list.AddChild(rich);
        }

        root.AddChild(scroll);
    }

    private (Godot.Button primary, Godot.Button? secondary) BuildActionBar(VBoxContainer root)
    {
        var actions = new HBoxContainer { Name = "Actions" };
        actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        actions.Alignment = BoxContainer.AlignmentMode.End;

        Godot.Button? secondary = null;
        // Always offer Copy details — even on the simple picker
        // rejection the player might want to share what they saw.
        if (!string.IsNullOrEmpty(_reportText))
        {
            secondary = new Godot.Button
            {
                Name = "CopyDetailsButton",
                Text = "Copy details",
                TooltipText = "Copy the severity, title, body, path, code and items to the clipboard.",
                CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            };
            secondary.Pressed += OnSecondaryPressed;
            actions.AddChild(secondary);
        }

        var primary = new Godot.Button
        {
            Name = "PrimaryButton",
            Text = _error.PrimaryButtonLabel,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        primary.Pressed += () => PrimaryPressed?.Invoke();
        actions.AddChild(primary);

        root.AddChild(actions);
        return (primary, secondary);
    }

    private void OnSecondaryPressed()
    {
        _clipboard.WriteText(_reportText);
        SecondaryPressed?.Invoke();
    }

    private static void AppendMonospaceRow(
        VBoxContainer parent,
        string label,
        string value,
        bool subdued = false)
    {
        var row = new HBoxContainer { Name = $"{label}Row" };
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);

        var labelControl = new Label
        {
            Name = $"{label}Label",
            Text = $"{label}:",
            CustomMinimumSize = new Vector2(72, 0),
        };
        labelControl.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        labelControl.AddThemeFontSizeOverride("font_size", 12);
        row.AddChild(labelControl);

        var valueControl = new Label
        {
            Name = $"{label}Value",
            Text = value,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        valueControl.AddThemeColorOverride(
            "font_color",
            subdued ? ManagerDesignTokens.TextSecondary : ManagerDesignTokens.TextPrimary);
        // Monospace is reserved for paths and codes — the kind of
        // text the player might copy verbatim.
        row.AddChild(valueControl);

        parent.AddChild(row);
    }

    private static StyleBoxFlat SeverityStyleBox(ApplicationErrorSeverity severity) =>
        new()
        {
            BgColor = severity switch
            {
                ApplicationErrorSeverity.Fatal => new Color(0.55f, 0.10f, 0.10f),
                _ => new Color(0.45f, 0.35f, 0.05f),
            },
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8,
        };

    private static string SeverityGlyph(ApplicationErrorSeverity severity) =>
        severity switch
        {
            ApplicationErrorSeverity.Fatal => "✕",
            _ => "⚠",
        };

    private static string EscapeBbcode(string value) =>
        value.Replace("[", "\\[").Replace("]", "\\]");
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

/// <summary>
/// Resolver that delegates <c>res://</c> translation to Godot's
/// <c>ProjectSettings.GlobalizePath</c>. Kept as a separate type so
/// the Core project never has to import Godot.
/// </summary>
public static class GodotResPathResolver
{
    /// <summary>Function-typed handle the panel passes to <see cref="ResPath.Globalize"/>.</summary>
    public static string Globalize(string resourcePath) =>
        ProjectSettings.GlobalizePath(resourcePath);
}
