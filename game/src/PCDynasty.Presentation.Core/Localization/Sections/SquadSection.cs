namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the squad screen: the header band, the empty-state
/// hint, the trailing action button, and the column labels. The two
/// summary strings carry positional placeholders the screen fills
/// with the club name and the player count.
/// </summary>
public sealed record SquadSection(
    HeaderSection Header,
    string SummaryOwnedFormat,
    string SummaryBrowseFormat,
    string ActionOpenMarket,
    string ActionOpenMarketTooltip,
    string Empty,
    SquadColumnsSection Columns,
    SquadActionsSection Actions);

/// <summary>Column headers for the player list, in display order.</summary>
public sealed record SquadColumnsSection(
    string Player,
    string Nation,
    string Age,
    string Tec,
    string For,
    string Fis,
    string Men,
    string Val,
    string Action);

/// <summary>The per-row action button labels.</summary>
public sealed record SquadActionsSection(string Inspect, string InspectTooltip);
