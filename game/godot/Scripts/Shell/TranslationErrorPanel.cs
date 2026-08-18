using Godot;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Renders a <see cref="TranslationErrorPanelShape"/> as a Godot
/// <see cref="PanelContainer"/>. The panel is intentionally narrow
/// (max width 640 px) and centred; the diagnostic list lives inside a
/// <see cref="ScrollContainer"/> with a fixed pixel cap so a file
/// with hundreds of missing keys does not overflow the viewport.
/// </summary>
/// <remarks>
/// The renderer is a thin pass-through over the shape: every string
/// the player sees comes from <see cref="TranslationErrorPanelShapeBuilder"/>,
/// which is unit-tested. The Godot side is reviewed manually and
/// exercised by the smoke boot; the C# tests pin the shape contract.
/// </remarks>
public sealed partial class TranslationErrorPanel : PanelContainer
{
    /// <summary>Fired when the primary button (the only button in panels A / B / C) is pressed.</summary>
    public event Action? PrimaryPressed;

    private readonly Button _primary;

    /// <summary>Builds a panel from a shape and wires the primary button to <see cref="PrimaryPressed"/>.</summary>
    public TranslationErrorPanel(TranslationErrorPanelShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        Name = $"TranslationErrorPanel_{shape.Kind}";
        CustomMinimumSize = new Vector2(640, 0);
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);

        var root = new VBoxContainer { Name = "Root" };
        root.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        AddChild(root);

        var title = new Label
        {
            Name = "Title",
            Text = shape.Title,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 18);
        root.AddChild(title);

        var body = new Label
        {
            Name = "Body",
            Text = shape.Body,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        body.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        root.AddChild(body);

        if (!string.IsNullOrEmpty(shape.FilePath))
        {
            var path = new Label
            {
                Name = "Path",
                Text = $"Path: {shape.FilePath}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            path.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            root.AddChild(path);
        }

        AppendDiagnosticList(root, "MissingKeys", shape.MissingKeys, shape.MaximumListHeight);
        AppendDiagnosticList(root, "ExtraKeys", shape.ExtraKeys, shape.MaximumListHeight);
        AppendDiagnosticList(root, "InvalidKeys", shape.InvalidKeys, shape.MaximumListHeight);

        _primary = new Button
        {
            Name = "PrimaryButton",
            Text = shape.PrimaryButtonLabel,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        _primary.Pressed += () => PrimaryPressed?.Invoke();
        root.AddChild(_primary);
    }

    private static void AppendDiagnosticList(
        VBoxContainer parent,
        string listName,
        System.Collections.Immutable.ImmutableArray<TranslationErrorPanelItem> items,
        int maxHeight)
    {
        if (items.IsDefaultOrEmpty || items.Length == 0)
        {
            return;
        }

        // The scroll container is always present (even when the list
        // fits), so the test contract "scroll container is present in
        // the tree" holds regardless of how many keys the file is
        // missing.
        var scroll = new ScrollContainer { Name = $"{listName}Scroll" };
        if (maxHeight > 0)
        {
            scroll.CustomMinimumSize = new Vector2(0, maxHeight);
            scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        }

        var list = new VBoxContainer { Name = listName };
        list.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        scroll.AddChild(list);

        foreach (TranslationErrorPanelItem item in items)
        {
            string line = $"• {item.Path} — {item.Message}";
            var label = new Label
            {
                Name = $"{listName}Item",
                Text = line,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
            list.AddChild(label);
        }

        parent.AddChild(scroll);
    }
}
