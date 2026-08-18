using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Components;

public enum MetricTone
{
    Neutral,
    Positive,
    Warning,
    Negative,
}

public sealed class MetricCardView
{
    public MetricCardView(
        string label,
        string value,
        string context,
        string accessibleDescription,
        MetricTone tone = MetricTone.Neutral)
    {
        if (!Enum.IsDefined(tone))
        {
            throw new ArgumentOutOfRangeException(nameof(tone));
        }

        Label = Required(label, nameof(label));
        Value = Required(value, nameof(value));
        Context = context?.Trim() ?? throw new ArgumentNullException(nameof(context));
        AccessibleDescription = Required(
            accessibleDescription,
            nameof(accessibleDescription));
        Tone = tone;
    }

    public string Label { get; }
    public string Value { get; }
    public string Context { get; }
    public string AccessibleDescription { get; }
    public MetricTone Tone { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public partial class MetricCard : PanelContainer
{
    private readonly Label _context = new();
    private readonly Label _label = new();
    private readonly Label _value = new();
    private bool _initialized;

    public override void _Ready()
    {
        EnsureContent();
    }

    public void Bind(MetricCardView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        EnsureContent();
        _label.Text = view.Label;
        _value.Text = view.Value;
        _context.Text = view.Context;
        _context.Visible = view.Context.Length > 0;
        _value.AddThemeColorOverride("font_color", ToneColor(view.Tone));
        TooltipText = view.AccessibleDescription;
    }

    private void EnsureContent()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        FocusMode = FocusModeEnum.All;
        CustomMinimumSize = new Vector2(180, 112);
        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        _value.AddThemeFontSizeOverride("font_size", 24);
        _context.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        content.AddChild(_label);
        content.AddChild(_value);
        content.AddChild(_context);
        AddChild(content);
    }

    private static Color ToneColor(MetricTone tone) => tone switch
    {
        MetricTone.Neutral => ManagerDesignTokens.TextPrimary,
        MetricTone.Positive => ManagerDesignTokens.Positive,
        MetricTone.Warning => ManagerDesignTokens.Warning,
        MetricTone.Negative => ManagerDesignTokens.Negative,
        _ => throw new ArgumentOutOfRangeException(nameof(tone)),
    };
}
