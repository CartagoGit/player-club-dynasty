using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Settings as a menu of sections: the player picks an area first and only
/// then sees its controls.
/// </summary>
/// <remarks>
/// The classic managers never dumped every preference onto one surface; they
/// offered a short, chunky list of areas and opened one at a time, with a
/// single way back. Stacking language, theme, display and data controls into a
/// single panel is what made this screen unreadable.
///
/// The menu owns navigation and chrome only. It never knows what a section
/// contains: callers register a title and a control, which keeps language,
/// theme, display and data independent of each other and of this file.
/// </remarks>
public sealed partial class OptionsMenu : VBoxContainer
{
    private readonly Label _title = new() { Name = "OptionsTitle" };
    private readonly VBoxContainer _entries = new() { Name = "OptionsEntries" };
    private readonly VBoxContainer _content = new() { Name = "OptionsSectionContent" };
    private readonly Button _back = new() { Name = "Back" };
    private readonly List<(string Title, Control Content, Button Entry)> _sections = [];

    private string _rootTitle = "OPCIONES";
    private string _backLabel = "VOLVER";

    /// <summary>Raised when the player leaves the settings menu entirely.</summary>
    public event Action? Closed;

    public OptionsMenu()
    {
        Name = "OptionsMenu";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        _title.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        _title.AddThemeFontSizeOverride("font_size", 22);
        AddChild(_title);
        AddChild(new HSeparator());

        _entries.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        AddChild(_entries);

        _content.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        _content.Visible = false;
        AddChild(_content);

        var spacer = new Control { Name = "OptionsSpacer", SizeFlagsVertical = SizeFlags.ExpandFill };
        AddChild(spacer);

        _back.CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget);
        _back.Alignment = HorizontalAlignment.Left;
        _back.Pressed += GoBack;
        AddChild(_back);

        ShowSectionList();
    }

    /// <summary>Sets the labels the shell owns, so this file carries no copy.</summary>
    public void SetChrome(string rootTitle, string backLabel)
    {
        _rootTitle = string.IsNullOrWhiteSpace(rootTitle) ? _rootTitle : rootTitle;
        _backLabel = string.IsNullOrWhiteSpace(backLabel) ? _backLabel : backLabel;
        ShowSectionList();
    }

    /// <summary>
    /// Registers one area. The control is reparented into the menu and stays
    /// hidden until the player walks into its section.
    /// </summary>
    public void AddSection(string title, Control content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        var entry = new Button
        {
            Name = $"Section{_sections.Count + 1}",
            Text = $"  {title.ToUpperInvariant()}",
            Alignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            TooltipText = title,
        };
        entry.AddThemeStyleboxOverride("normal", ClassicChrome.RailEntry(active: false));
        int index = _sections.Count;
        entry.Pressed += () => OpenSection(index);
        _entries.AddChild(entry);

        content.Visible = false;
        content.GetParent()?.RemoveChild(content);
        _content.AddChild(content);
        _sections.Add((title, content, entry));
    }

    /// <summary>Walks back to the section list, or out of settings from there.</summary>
    public void GoBack()
    {
        if (_content.Visible)
        {
            ShowSectionList();
            return;
        }

        Closed?.Invoke();
    }

    private void OpenSection(int index)
    {
        if (index < 0 || index >= _sections.Count)
        {
            return;
        }

        for (int position = 0; position < _sections.Count; position++)
        {
            _sections[position].Content.Visible = position == index;
        }

        _title.Text = $"{_rootTitle} · {_sections[index].Title.ToUpperInvariant()}";
        _entries.Visible = false;
        _content.Visible = true;
        _back.Text = $"  {_backLabel}";
    }

    private void ShowSectionList()
    {
        foreach ((_, Control content, _) in _sections)
        {
            content.Visible = false;
        }

        _title.Text = _rootTitle;
        _entries.Visible = true;
        _content.Visible = false;
        _back.Text = $"  {_backLabel}";
    }
}
