using Godot;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// The main menu as a board of destinations rather than a column of
/// session commands.
/// </summary>
/// <remarks>
/// The reference's menu was a 3×3 grid of illustrated destinations over a
/// rail of small commands, and every destination the disc knew about was on
/// it — including the expansions that were not installed, greyed, in their
/// place. That is the shape this rebuilds, in our own grammar.
///
/// Two rules make it a board and not a list. Every destination is visible at
/// once: nothing hides behind a submenu. And a destination that is not built
/// yet stays on the board carrying "on the way", because the menu is the map
/// of the game and a map with gaps is more honest than one that lies about
/// its size.
///
/// The tiles keep the node names the rest of the shell already binds to
/// (<c>NewCareer</c>, <c>ContinueCareer</c>, …), so wiring a destination is
/// still a lookup by name.
/// </remarks>
public sealed partial class DestinationBoard : VBoxContainer
{
    private const int Columns = 3;

    private readonly GridContainer _grid = new() { Name = "Grid" };
    private readonly HBoxContainer _rail = new() { Name = "Rail" };
    private readonly Dictionary<string, DestinationTile> _tiles = [];

    public DestinationBoard()
    {
        Name = "Board";
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        _grid.Columns = Columns;
        _grid.SizeFlagsVertical = SizeFlags.ExpandFill;
        _grid.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space8);
        _grid.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space8);
        AddChild(_grid);

        _rail.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        AddChild(_rail);
    }

    /// <summary>Look a destination up by the node name the shell binds to.</summary>
    public DestinationTile? Tile(string name) =>
        _tiles.TryGetValue(name, out DestinationTile? tile) ? tile : null;

    /// <summary>
    /// Fills the board. `built` names the destinations that actually work
    /// today; everything else is drawn greyed with the "on the way" hint, so
    /// the board never has to be edited to reveal a mode — only this set does.
    /// </summary>
    public void Build(MenuBoardSection copy, MenuActionsSection actions, IReadOnlySet<string> built)
    {
        ArgumentNullException.ThrowIfNull(copy);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(built);

        foreach (Node child in _grid.GetChildren())
        {
            child.QueueFree();
        }

        _tiles.Clear();

        // Order matters: the three career rungs first, because that is the
        // question the player is actually answering — how much of the game do
        // I want to run — then the ways back into an existing one, then the
        // parts of the world that need no career at all.
        Add("NewCareer", ShellArea.Technical, copy.Manager, built, copy.ComingSoon);
        Add("CoachCareer", ShellArea.Technical, copy.Coach, built, copy.ComingSoon);
        Add("ProManagerCareer", ShellArea.Technical, copy.ProManager, built, copy.ComingSoon);
        Add("ContinueCareer", ShellArea.Tracking, copy.Resume, built, copy.ComingSoon);
        Add("LoadCareer", ShellArea.Tracking, copy.Load, built, copy.ComingSoon);
        Add("Archive", ShellArea.Tracking, copy.Archive, built, copy.ComingSoon);
        Add("FriendlyMatch", ShellArea.World, copy.Friendly, built, copy.ComingSoon);
        Add("DataEditor", ShellArea.World, copy.Database, built, copy.ComingSoon);
        Add("WorldBrowser", ShellArea.World, copy.World, built, copy.ComingSoon);

        BuildRail(actions);
    }

    /// <summary>Marks a destination available or not after the board is built.</summary>
    public void SetAvailable(string name, bool available, MenuDestinationSection copy, string comingSoon)
    {
        ArgumentNullException.ThrowIfNull(copy);
        Tile(name)?.Show(copy.Title, Hint(copy, available, comingSoon), available);
    }

    private void Add(
        string name,
        ShellArea area,
        MenuDestinationSection copy,
        IReadOnlySet<string> built,
        string comingSoon)
    {
        bool available = built.Contains(name);
        var tile = new DestinationTile(name, area, _tiles.Count + 1);
        tile.Show(copy.Title, Hint(copy, available, comingSoon), available);
        _grid.AddChild(tile);
        _tiles[name] = tile;
    }

    /// <summary>
    /// An unavailable destination says what it will be **and** that it is not
    /// here yet. Naming only the absence would leave the board unreadable;
    /// naming only the promise would be a lie.
    /// </summary>
    private static string Hint(MenuDestinationSection copy, bool available, string comingSoon) =>
        available ? copy.Hint : $"{copy.Hint} · {comingSoon.ToUpperInvariant()}";

    private void BuildRail(MenuActionsSection actions)
    {
        foreach (Node child in _rail.GetChildren())
        {
            child.QueueFree();
        }

        // The rail carries small commands, not destinations: they take their
        // own width and leave the grid above as the thing being looked at.
        Button options = ClassicChrome.CommandButton("Options", actions.Options);
        options.CustomMinimumSize = new Vector2(160, ManagerDesignTokens.MinimumPointerTarget);
        _rail.AddChild(options);

        var spacer = new Control { Name = "RailSpacer", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _rail.AddChild(spacer);

        // Exit sits apart from everything else, at the far edge — the way the
        // reference separated SALIR from the small commands beside it.
        Button exit = ClassicChrome.CommandButton("Exit", actions.Exit, ClassicChrome.Intent.Destructive);
        exit.CustomMinimumSize = new Vector2(160, ManagerDesignTokens.MinimumPointerTarget);
        _rail.AddChild(exit);
    }

    /// <summary>The two small commands under the grid, by node name.</summary>
    public Button? Command(string name) => _rail.GetNodeOrNull<Button>(name);
}
