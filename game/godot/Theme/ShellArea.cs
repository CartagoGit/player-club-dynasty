namespace PCDynasty.Presentation.Theme;

/// <summary>
/// The functional areas the shell colour-codes. The classic desk gave each
/// group of destinations its own hue and repeated that hue inside every screen
/// the group opened, so the player always knew where they were without reading
/// the title.
/// </summary>
/// <remarks>
/// The enum carries no colour of its own: <see cref="ManagerDesignTokens.AreaColor"/>
/// derives one from the active palette, so a palette switch re-colours the areas
/// and no screen ever names a literal.
/// </remarks>
public enum ShellArea
{
    /// <summary>Neutral chrome: no area, plain surface.</summary>
    None = 0,

    /// <summary>Results, standings, calendar — what happened and what is next.</summary>
    Tracking = 1,

    /// <summary>Squad, lineup, tactics — the sporting side.</summary>
    Technical = 2,

    /// <summary>Transfers, staff, roster — who comes and who goes.</summary>
    Market = 3,

    /// <summary>Cash, decisions, stadium — the money.</summary>
    Finance = 4,

    /// <summary>Competitions and clubs beyond our own.</summary>
    World = 5,

    /// <summary>Preferences and data sources.</summary>
    Settings = 6,
}
