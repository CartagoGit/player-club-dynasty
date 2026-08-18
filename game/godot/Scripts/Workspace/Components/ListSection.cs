using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Workspace.Components;

/// <summary>
/// A bordered plate that wraps a list of zebra rows. The plate is the workhorse
/// of the classic manager UX: a titled, dense table where every row carries its
/// own hover and selected state, and the rows are clickable.
/// </summary>
/// <remarks>
/// The section owns:
///   - the column-header row (built with <see cref="AddColumnHeader"/>),
///   - the scrollable body of <see cref="ZebraRow"/>s,
///   - the empty-state hint when there are no rows,
///   - the optional trailing footer (action area, hint, etc.).
/// The section never builds cells; each <see cref="ZebraRow"/> exposes
/// <see cref="ZebraRow.Cells"/> so the caller decides the cell layout. The
/// section is palette-aware (f00043) and token-driven; no hardcoded colour.
/// </remarks>
public sealed partial class ListSection : PanelContainer
{
    private readonly VBoxContainer _column = new() { Name = "Column" };
    private readonly HBoxContainer _headers = new() { Name = "Headers" };
    private readonly ScrollContainer _scroll = new() { Name = "Scroll" };
    private readonly VBoxContainer _rows = new() { Name = "Rows" };
    private readonly Label _empty = new() { Name = "Empty" };
    private readonly VBoxContainer _footer = new() { Name = "Footer" };

    private int _selectedIndex = -1;

    /// <summary>Emitted with the row's index when a row is clicked.</summary>
    public event Action<int>? RowActivated;

    public ListSection()
    {
        AddThemeStyleboxOverride("panel", ClassicChrome.Plate(ManagerDesignTokens.Space8));
        _column.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        AddChild(_column);

        _headers.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        _headers.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        _column.AddChild(_headers);
        _column.AddChild(new HSeparator());

        _scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _rows.AddThemeConstantOverride("separation", 0);
        _scroll.AddChild(_rows);
        _column.AddChild(_scroll);

        _empty.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        _empty.HorizontalAlignment = HorizontalAlignment.Center;
        _empty.Visible = false;
        _column.AddChild(_empty);

        _footer.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _footer.Visible = false;
        _column.AddChild(_footer);
    }

    /// <summary>Appends a column header to the header row. Widths are in pixels and are inherited by every row.</summary>
    public Label AddColumnHeader(string text, int width, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var label = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = alignment,
        };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        label.AddThemeFontSizeOverride("font_size", 11);
        _headers.AddChild(label);
        return label;
    }

    /// <summary>Appends a row, already built by the caller. The section keeps a reference for selection.</summary>
    public void AddRow(ZebraRow row)
    {
        row.Index = _rows.GetChildCount();
        if (row.Index == _selectedIndex)
        {
            row.IsSelected = true;
        }
        row.Activated += OnRowActivated;
        _rows.AddChild(row);
    }

    /// <summary>Replaces the row at the given index. Used by partial refreshes (e.g. a sign that mutates one row).</summary>
    public void ReplaceRow(int index, ZebraRow row)
    {
        if (index < 0 || index >= _rows.GetChildCount())
        {
            return;
        }
        Node old = _rows.GetChild(index);
        _rows.RemoveChild(old);
        old.QueueFree();
        AddRow(row);
    }

    /// <summary>Returns the row at the given index, or <c>null</c> when the section is empty.</summary>
    public ZebraRow? GetRow(int index)
    {
        if (index < 0 || index >= _rows.GetChildCount())
        {
            return null;
        }
        return _rows.GetChild(index) as ZebraRow;
    }

    /// <summary>Number of rows currently rendered. Empty state and the footer are not counted.</summary>
    public int RowCount => _rows.GetChildCount();

    /// <summary>Index of the currently selected row, or -1 if no row is selected.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value)
            {
                return;
            }
            ZebraRow? previous = GetRow(_selectedIndex);
            if (previous is not null)
            {
                previous.IsSelected = false;
            }
            _selectedIndex = value;
            ZebraRow? current = GetRow(_selectedIndex);
            if (current is not null)
            {
                current.IsSelected = true;
            }
        }
    }

    /// <summary>Sets the text shown when there are no rows.</summary>
    public void SetEmptyText(string text)
    {
        _empty.Text = text;
    }

    /// <summary>Visibility of the empty-state label, derived from the row count.</summary>
    public void RefreshEmptyState()
    {
        _empty.Visible = _rows.GetChildCount() == 0;
        _scroll.Visible = _rows.GetChildCount() > 0;
    }

    /// <summary>Removes every row. Used when the underlying data changes shape.</summary>
    public void ClearRows()
    {
        foreach (Node child in _rows.GetChildren())
        {
            _rows.RemoveChild(child);
            child.QueueFree();
        }
        _selectedIndex = -1;
        RefreshEmptyState();
    }

    /// <summary>The footer container; a screen can append hints or trailing actions here.</summary>
    public VBoxContainer Footer
    {
        get
        {
            _footer.Visible = true;
            return _footer;
        }
    }

    private void OnRowActivated(int index) => RowActivated?.Invoke(index);
}
