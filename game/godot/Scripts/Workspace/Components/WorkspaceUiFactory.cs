using System.Globalization;
using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Workspace.Components;

internal static class WorkspaceUiFactory
{
    internal static CultureInfo InvariantCulture { get; } = CultureInfo.InvariantCulture;

    internal static PanelContainer CreatePanel(string title, IEnumerable<string> lines, Color titleColor)
    {
        var panel = CreateSurfacePanel();
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);

        var titleLabel = new Label { Text = title };
        titleLabel.AddThemeColorOverride("font_color", titleColor);
        titleLabel.AddThemeFontSizeOverride("font_size", 14);
        column.AddChild(titleLabel);

        foreach (string line in lines)
        {
            var item = new Label { Text = $"• {line}" };
            item.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
            item.AddThemeFontSizeOverride("font_size", 14);
            column.AddChild(item);
        }

        panel.AddChild(column);
        return panel;
    }

    internal static PanelContainer CreateSurfacePanel()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.Surface,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space12,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space12,
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    internal static void AddField(GridContainer grid, string label, string value)
    {
        var field = new VBoxContainer();
        field.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);

        var labelNode = new Label { Text = label };
        labelNode.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        labelNode.AddThemeFontSizeOverride("font_size", 11);

        var valueNode = new Label { Text = value };
        valueNode.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        valueNode.AddThemeFontSizeOverride("font_size", 16);

        field.AddChild(labelNode);
        field.AddChild(valueNode);
        grid.AddChild(field);
    }

    internal static void AddColumnHeader(
        HBoxContainer row,
        string text,
        int width,
        HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var node = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = alignment,
        };
        node.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        node.AddThemeFontSizeOverride("font_size", 11);
        row.AddChild(node);
    }

    internal static void AddColumnValue(
        HBoxContainer row,
        string text,
        int width,
        Color color,
        HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var node = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = alignment,
        };
        node.AddThemeColorOverride("font_color", color);
        node.AddThemeFontSizeOverride("font_size", 14);
        row.AddChild(node);
    }

    internal static Label CreateText(string text, Color color)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    internal static string Initials(string displayName)
    {
        string[] parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        return parts.Length == 1
            ? parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant()
            : $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    internal static string PlayerAge(int birthYear) => (2026 - birthYear).ToString(InvariantCulture);

    internal static string PlayerCountry(string playerExternalId)
    {
        if (playerExternalId.Contains("aurora", StringComparison.OrdinalIgnoreCase)) return "NOR";
        if (playerExternalId.Contains("cinder", StringComparison.OrdinalIgnoreCase)) return "VEL";
        if (playerExternalId.Contains("forge", StringComparison.OrdinalIgnoreCase)) return "VEL";
        if (playerExternalId.Contains("maris", StringComparison.OrdinalIgnoreCase)) return "SOL";
        if (playerExternalId.Contains("northglass", StringComparison.OrdinalIgnoreCase)) return "NOR";
        return "—";
    }

    internal static Color ValueColor(int value) => value switch
    {
        >= 80 => ManagerDesignTokens.Positive,
        >= 65 => ManagerDesignTokens.Accent,
        >= 50 => ManagerDesignTokens.TextPrimary,
        _ => ManagerDesignTokens.TextSecondary,
    };

    internal static Color DifferenceColor(int difference) => difference switch
    {
        > 0 => ManagerDesignTokens.Positive,
        < 0 => ManagerDesignTokens.Negative,
        _ => ManagerDesignTokens.TextSecondary,
    };

    internal static void Clear(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
