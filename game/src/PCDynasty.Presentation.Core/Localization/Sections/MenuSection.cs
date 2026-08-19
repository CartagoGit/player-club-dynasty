namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the main menu: the title plate, the eyebrow tag, the six
/// menu actions and the board of destinations. The actions live in a
/// sub-section so the translation file can group them under
/// <c>menu.actions</c> rather than cluttering the root of the section.
/// </summary>
public sealed record MenuSection(
    HeaderSection Header,
    MenuActionsSection Actions,
    MenuBoardSection Board);

/// <summary>
/// The board of destinations: every place the game can take the player,
/// including the ones that are not built yet.
/// </summary>
/// <remarks>
/// A destination that does not exist is still listed, greyed, carrying
/// <see cref="ComingSoon"/> as its hint. The menu is the map of the game,
/// and a map with gaps is more honest than one that lies about its size —
/// the reference did exactly this with its uninstalled expansions.
/// </remarks>
public sealed record MenuBoardSection(
    string ComingSoon,
    MenuDestinationSection Manager,
    MenuDestinationSection Coach,
    MenuDestinationSection ProManager,
    MenuDestinationSection Resume,
    MenuDestinationSection Load,
    MenuDestinationSection Friendly,
    MenuDestinationSection Database,
    MenuDestinationSection Archive,
    MenuDestinationSection World);

/// <summary>One destination: its name, and one line saying what it is.</summary>
public sealed record MenuDestinationSection(string Title, string Hint);

/// <summary>
/// The button labels the main menu exposes to the player.
/// </summary>
public sealed record MenuActionsSection(
    string NewCareer,
    string Continue,
    string Load,
    string DataEditor,
    string Options,
    string Exit);
