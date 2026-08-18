using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Workspace.Components;

/// <summary>
/// A bordered row that follows the shell's design language: zebra background
/// on odd indices, hover surface, gold edge when selected. Every colour
/// resolves through <see cref="ManagerDesignTokens"/> so the f00043 palette
/// switch re-skins rows for free.
/// </summary>
/// <remarks>
/// The row is a <see cref="PanelContainer"/> wrapping an <c>HBoxContainer</c>
/// of cells. Screens add cells with <see cref="AddCell"/>; the row owns
/// the hover/select state machine and the background, the cells own their
/// width and alignment. The action area on the right is built by the row
/// itself so every screen ends up with a consistent trailing-button zone.
/// </remarks>
public sealed partial class ZebraRow : PanelContainer
{
    private static readonly Color OddRowBackground = ManagerDesignTokens.Canvas;

    private static StyleBoxFlat BaseStyle(Color background) => new()
    {
        BgColor = background,
        BorderColor = ManagerDesignTokens.Border,
        BorderWidthLeft = 1,
        BorderWidthTop = 0,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 0,
        CornerRadiusTopRight = 0,
        CornerRadiusBottomRight = 0,
        CornerRadiusBottomLeft = 0,
        ContentMarginLeft = ManagerDesignTokens.Space8,
        ContentMarginTop = ManagerDesignTokens.Space4,
        ContentMarginRight = ManagerDesignTokens.Space8,
        ContentMarginBottom = ManagerDesignTokens.Space4,
    };

    private static StyleBoxFlat HoverStyle() => new()
    {
        BgColor = ManagerDesignTokens.SurfaceRaised,
        BorderColor = ManagerDesignTokens.Accent,
        BorderWidthLeft = 2,
        BorderWidthTop = 0,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 0,
        CornerRadiusTopRight = 0,
        CornerRadiusBottomRight = 0,
        CornerRadiusBottomLeft = 0,
        ContentMarginLeft = ManagerDesignTokens.Space8 - 1,
        ContentMarginTop = ManagerDesignTokens.Space4,
        ContentMarginRight = ManagerDesignTokens.Space8,
        ContentMarginBottom = ManagerDesignTokens.Space4,
    };

    private static StyleBoxFlat SelectedStyle() => new()
    {
        BgColor = ManagerDesignTokens.SurfaceRaised,
        BorderColor = ManagerDesignTokens.Accent,
        BorderWidthLeft = 4,
        BorderWidthTop = 0,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 0,
        CornerRadiusTopRight = 0,
        CornerRadiusBottomRight = 0,
        CornerRadiusBottomLeft = 0,
        ContentMarginLeft = ManagerDesignTokens.Space8,
        ContentMarginTop = ManagerDesignTokens.Space4,
        ContentMarginRight = ManagerDesignTokens.Space8,
        ContentMarginBottom = ManagerDesignTokens.Space4,
    };

    private readonly HBoxContainer _cells = new() { Name = "Cells" };
    private readonly HBoxContainer _actions = new() { Name = "Actions" };
    private bool _isHovered;
    private bool _isSelected;
    private int _index;

    /// <summary>Emitted when the user clicks the row body. The action is invoked with the row's index.</summary>
    public event Action<int>? Activated;

    public ZebraRow(int index)
    {
        _index = index;
        _cells.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        _actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _actions.Alignment = BoxContainer.AlignmentMode.End;
        MouseEntered += () => { _isHovered = true; RefreshStyle(); };
        MouseExited += () => { _isHovered = false; RefreshStyle(); };
        GuiInput += OnGuiInput;

        var split = new HBoxContainer { Name = "Split" };
        split.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        split.AddChild(_cells);
        _cells.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        split.AddChild(_actions);
        AddChild(split);
        RefreshStyle();
    }

    public int Index
    {
        get => _index;
        set
        {
            _index = value;
            RefreshStyle();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            RefreshStyle();
        }
    }

    public HBoxContainer Cells => _cells;
    public HBoxContainer Actions => _actions;

    /// <summary>Appends a left-aligned text cell with the given width and colour.</summary>
    public Label AddCell(string text, int width, Color color, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var label = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = alignment,
        };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.VerticalAlignment = VerticalAlignment.Center;
        _cells.AddChild(label);
        return label;
    }

    /// <summary>Appends a trailing action button. The action is invoked with the row index.</summary>
    public Button AddAction(string text, string? tooltip, Action<int>? onPressed, bool enabled = true)
    {
        var button = new Button
        {
            Text = text,
            TooltipText = tooltip ?? string.Empty,
            Disabled = !enabled,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        if (onPressed is not null)
        {
            int capturedIndex = _index;
            button.Pressed += () => onPressed(capturedIndex);
        }
        _actions.AddChild(button);
        return button;
    }

    /// <summary>Replaces the cell at the given slot's text. Used by partial refreshes.</summary>
    public void SetCellText(int slot, string text)
    {
        if (slot < 0 || slot >= _cells.GetChildCount())
        {
            return;
        }
        if (_cells.GetChild(slot) is Label label)
        {
            label.Text = text;
        }
    }

    private void RefreshStyle()
    {
        StyleBoxFlat style = _isSelected
            ? SelectedStyle()
            : _isHovered
                ? HoverStyle()
                : BaseStyle(BackgroundForIndex(_index));
        AddThemeStyleboxOverride("panel", style);
    }

    /// <summary>Even rows use the surface; odd rows use the canvas as a faint zebra striping.</summary>
    private static Color BackgroundForIndex(int index) =>
        (index & 1) == 0 ? ManagerDesignTokens.Surface : OddRowBackground;

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            Activated?.Invoke(_index);
            AcceptEvent();
        }
    }
}
