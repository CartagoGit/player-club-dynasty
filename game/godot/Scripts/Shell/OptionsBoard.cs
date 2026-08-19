using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Settings as a configuration screen with one tab per kind of option.
/// </summary>
/// <remarks>
/// The classic configuration screen grouped preferences into rotulated plates
/// and put every group on one surface. That works while there are four groups;
/// with language, look, graphics, audio, video and data it stops working, and
/// the shape the original itself used for exactly this problem is the tab strip
/// of its dossier — one strip, every option named, only one open at a time.
///
/// The board owns navigation and chrome only. It never knows what a tab
/// contains: callers register a title and a control, which keeps every area
/// independent of the others and of this file. Tabs are addressed by a stable
/// node name, never by index, so a test or a shortcut survives a reordering.
/// </remarks>
public sealed partial class OptionsBoard : VBoxContainer, IPaletteAware
{
    private readonly EdgeRail _rail = new("OPCIONES", ShellArea.Settings);
    private readonly TabStrip _tabs = new();
    private readonly VBoxContainer _pages = new() { Name = "Pages" };
    private readonly CommandBar _commands = new();
    private readonly List<CaptionPlate> _plates = [];
    private Button? _back;
    private string _backLabel = "VOLVER";

    /// <summary>Raised when the player leaves settings.</summary>
    public event Action? Closed;

    public OptionsBoard()
    {
        Name = "OptionsBoard";
        AddThemeConstantOverride("separation", 0);

        var body = new HBoxContainer { Name = "BoardBody", SizeFlagsVertical = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 0);
        AddChild(body);
        body.AddChild(_rail);

        var column = new VBoxContainer
        {
            Name = "BoardColumn",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        column.AddThemeConstantOverride("separation", 0);
        body.AddChild(column);

        _tabs.Area = ShellArea.Settings;
        _tabs.TabSelected += ShowPage;
        column.AddChild(_tabs);

        var scroll = new ScrollContainer
        {
            Name = "BoardScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        column.AddChild(scroll);

        var margin = new MarginContainer { Name = "BoardMargin", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        foreach (string side in new[] { "margin_left", "margin_right", "margin_top", "margin_bottom" })
        {
            margin.AddThemeConstantOverride(side, ManagerDesignTokens.Space16);
        }
        scroll.AddChild(margin);

        _pages.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);
        _pages.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _pages.SizeFlagsVertical = SizeFlags.ExpandFill;
        margin.AddChild(_pages);

        AddChild(_commands);
        _back = _commands.AddLeading("Back", _backLabel, () => Closed?.Invoke());
    }

    /// <summary>Sets the labels the shell owns, so this file carries no copy.</summary>
    public void SetChrome(string rootTitle, string backLabel)
    {
        if (!string.IsNullOrWhiteSpace(rootTitle))
        {
            _rail.SetText(rootTitle);
        }
        if (!string.IsNullOrWhiteSpace(backLabel))
        {
            _backLabel = backLabel;
            if (_back is not null)
            {
                _back.Text = backLabel.ToUpperInvariant();
            }
        }
    }

    /// <summary>
    /// Registers one kind of option. It becomes a tab, and its control becomes
    /// the plate the tab opens.
    /// </summary>
    /// <param name="name">
    /// Stable node name for the tab and its plate. The title is translated, so
    /// anything that needs to address an area uses this instead.
    /// </param>
    public void AddSection(string title, Control content, string name = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        string stableName = string.IsNullOrWhiteSpace(name) ? $"Section{_plates.Count + 1}" : name;
        // One plate is on screen at a time, so it takes the whole page: a tab
        // whose body floats in the top eighth of the screen reads as a mistake.
        var plate = new CaptionPlate(title, ShellArea.Settings)
        {
            Name = stableName,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Visible = _plates.Count == 0,
            FillHeight = true,
        };

        content.Visible = true;
        content.GetParent()?.RemoveChild(content);
        plate.Add(content);

        _pages.AddChild(plate);
        _plates.Add(plate);
        _tabs.AddTab(title, $"Tab{stableName}");
    }

    /// <summary>Opens the tab whose plate carries the given node name.</summary>
    public void OpenSection(string name)
    {
        for (int index = 0; index < _plates.Count; index++)
        {
            if (_plates[index].Name == name)
            {
                _tabs.Select(index);
                return;
            }
        }
    }

    /// <summary>Leaves settings. Kept as a method so a shortcut can call it.</summary>
    public void GoBack() => Closed?.Invoke();

    /// <inheritdoc />
    public void OnPaletteChanged()
    {
        // The rail, the strip, the plates and the bar are palette-aware
        // themselves; the applier reaches them on its own walk. The contract
        // stays declared so a future derived colour has an obvious home.
    }

    private void ShowPage(int index)
    {
        for (int position = 0; position < _plates.Count; position++)
        {
            _plates[position].Visible = position == index;
        }
    }
}
