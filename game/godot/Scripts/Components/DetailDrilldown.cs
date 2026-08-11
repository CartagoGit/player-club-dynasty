using System.Collections.Immutable;
using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Components;

public sealed record DetailFieldView(string Label, string Value);

public sealed class DetailDrilldownView
{
    public DetailDrilldownView(
        string title,
        string summary,
        IEnumerable<DetailFieldView> fields)
    {
        Title = Required(title, nameof(title));
        Summary = summary?.Trim() ?? throw new ArgumentNullException(nameof(summary));
        ArgumentNullException.ThrowIfNull(fields);
        Fields = fields.Select(field =>
        {
            ArgumentNullException.ThrowIfNull(field);
            _ = Required(field.Label, nameof(fields));
            ArgumentNullException.ThrowIfNull(field.Value);
            return field;
        }).ToImmutableArray();
    }

    public string Title { get; }
    public string Summary { get; }
    public ImmutableArray<DetailFieldView> Fields { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public partial class DetailDrilldown : PanelContainer
{
    [Signal]
    public delegate void CloseRequestedEventHandler();

    private readonly VBoxContainer _fields = new();
    private readonly Label _summary = new();
    private readonly Label _title = new();
    private bool _initialized;

    public override void _Ready()
    {
        EnsureContent();
        Visible = false;
    }

    public void ShowDetail(DetailDrilldownView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        EnsureContent();
        _title.Text = view.Title;
        _summary.Text = view.Summary;
        _summary.Visible = view.Summary.Length > 0;
        ClearFields();
        foreach (DetailFieldView field in view.Fields)
        {
            _fields.AddChild(CreateField(field));
        }

        TooltipText = view.Title;
        Visible = true;
        GrabFocus();
    }

    public void Close()
    {
        Visible = false;
        EmitSignal(SignalName.CloseRequested);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void EnsureContent()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        FocusMode = FocusModeEnum.All;
        CustomMinimumSize = new Vector2(360, 0);
        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new HBoxContainer();
        _title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _title.AddThemeFontSizeOverride("font_size", 24);
        var closeButton = new Button
        {
            Text = "Cerrar",
            TooltipText = "Cerrar panel de detalle",
            FocusMode = FocusModeEnum.All,
            CustomMinimumSize = new Vector2(
                ManagerDesignTokens.MinimumPointerTarget,
                ManagerDesignTokens.MinimumPointerTarget),
        };
        closeButton.Pressed += Close;
        header.AddChild(_title);
        header.AddChild(closeButton);
        _summary.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        root.AddChild(header);
        root.AddChild(_summary);
        root.AddChild(_fields);
        AddChild(root);
    }

    private static HBoxContainer CreateField(DetailFieldView field)
    {
        var row = new HBoxContainer();
        var label = new Label
        {
            Text = field.Label,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        row.AddChild(label);
        row.AddChild(new Label { Text = field.Value });
        return row;
    }

    private void ClearFields()
    {
        foreach (Node child in _fields.GetChildren())
        {
            _fields.RemoveChild(child);
            child.QueueFree();
        }
    }
}
