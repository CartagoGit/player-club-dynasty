namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the main menu: the title plate, the eyebrow tag and the
/// six menu actions. The actions live in a sub-section so the
/// translation file can group them under <c>menu.actions</c> rather
/// than cluttering the root of the section.
/// </summary>
public sealed record MenuSection(HeaderSection Header, MenuActionsSection Actions);

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
