namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the transfer market: the header band, the three notice
/// states (idle, accepted, rejected), the seven column headers, the
/// sign action and the rejection reason labels. The <c>Rejected</c>
/// notice carries a positional placeholder the screen fills with the
/// matched reason key, and the rejection reason block is a
/// sub-section so the translator can group every translation under
/// <c>market.reasons</c>.
///
/// <c>TruncatedFormat</c> is the line under a capped list. The market
/// is the whole transfer-listed population of the world, and drawing a
/// row per candidate is what made the screen slow; the screen shows the
/// strongest and says so rather than pretending the list is complete.
/// </summary>
public sealed record MarketSection(
    HeaderSection Header,
    string SummaryFormat,
    string NoticeIdle,
    string NoticeAcceptedFormat,
    string NoticeRejectedFormat,
    string Empty,
    string TruncatedFormat,
    MarketColumnsSection Columns,
    MarketActionsSection Actions,
    MarketReasonsSection Reasons);

public sealed record MarketColumnsSection(
    string Player,
    string SellingClub,
    string Age,
    string Tec,
    string Val,
    string Price,
    string Action);

public sealed record MarketActionsSection(string Sign, string SignTooltipFormat);

public sealed record MarketReasonsSection(
    string Budget,
    string Price,
    string Unavailable,
    string Stale,
    string Finance,
    string Generic);
