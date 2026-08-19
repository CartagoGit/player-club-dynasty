using Godot;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Settings as one configuration screen: every group of preferences is a
/// captioned plate and all of them are on screen at once.
/// </summary>
/// <remarks>
/// The classic configuration screen never buried a preference behind
/// navigation. It grouped them into rotulated plates — match, resolution,
/// sound, timing — laid them out on a single surface, put the screen's name on
/// a vertical rail and gave the player one way out. Everything was one glance
/// and one click away, and nothing scrolled.
///
/// The board owns navigation and chrome only. It never knows what a group
/// contains: callers register a title and a control, which keeps language,
/// look, display and data independent of each other and of this file. Groups
/// are addressed by a stable node name, never by index, so a test or a
/// shortcut survives a reordering.
/// </remarks>
public sealed partial class OptionsBoard : VBoxContainer, IPaletteAware
{
    private const int GroupMinimumWidth = 420;

    private readonly EdgeRail _rail = new("OPCIONES", ShellArea.Settings);
    private readonly GridContainer _groups = new() { Name = "GroupsFlow", Columns = 2 };
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

        var scroll = new ScrollContainer
        {
            Name = "BoardScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        body.AddChild(scroll);

        var margin = new MarginContainer { Name = "BoardMargin", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        foreach (string side in new[] { "margin_left", "margin_right", "margin_top", "margin_bottom" })
        {
            margin.AddThemeConstantOverride(side, ManagerDesignTokens.Space16);
        }
        scroll.AddChild(margin);

        _groups.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space16);
        _groups.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space16);
        _groups.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        margin.AddChild(_groups);
        // One column when two would not fit: the grammar forbids a plate that
        // leaves its frame far more strongly than it asks for two columns.
        margin.Resized += () => Reflow(margin.Size.X);

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
    /// Registers one group of preferences. The control is reparented into the
    /// group's plate and stays visible: this screen shows everything at once.
    /// </summary>
    /// <param name="name">
    /// Stable node name for the group. The title is translated, so anything
    /// that needs to address a group — a test, a shortcut — uses this instead.
    /// </param>
    public void AddSection(string title, Control content, string name = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        var plate = new CaptionPlate(title, ShellArea.Settings)
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Section{_plates.Count + 1}" : name,
            CustomMinimumSize = new Vector2(GroupMinimumWidth, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin,
        };

        content.Visible = true;
        content.GetParent()?.RemoveChild(content);
        plate.Add(content);

        _groups.AddChild(plate);
        _plates.Add(plate);
    }

    /// <summary>Leaves settings. Kept as a method so a shortcut can call it.</summary>
    public void GoBack() => Closed?.Invoke();

    private void Reflow(float availableWidth)
    {
        int columns = availableWidth >= (GroupMinimumWidth * 2) + ManagerDesignTokens.Space16 ? 2 : 1;
        if (_groups.Columns != columns)
        {
            _groups.Columns = columns;
        }
    }

    /// <inheritdoc />
    public void OnPaletteChanged()
    {
        // The rail, the plates and the bar are palette-aware themselves; the
        // applier reaches them on its own walk. Nothing extra is needed here,
        // but the board still declares the contract so a future derived colour
        // has an obvious home.
    }
}
