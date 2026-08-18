namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the history screen: the header band, the empty state,
/// the three filter labels and their option sets, the aggregate panel
/// title and empty state, the seven column headers, and the per-row
/// action button.
/// </summary>
public sealed record HistorySection(
    HeaderSection Header,
    string SummaryFormat,
    string Empty,
    string FilterEmpty,
    HistoryFiltersSection Filters,
    HistoryAggregateSection Aggregate,
    HistoryColumnsSection Columns,
    HistoryActionsSection Actions);

public sealed record HistoryFiltersSection(
    string CompetitionLabel,
    string ClubLabel,
    string OrderLabel,
    string AllOption,
    HistoryOrderOptionsSection OrderOptions);

public sealed record HistoryOrderOptionsSection(string Date, string Competition, string Goals);

public sealed record HistoryAggregateSection(string Title, string Empty);

public sealed record HistoryColumnsSection(
    string When,
    string Competition,
    string Home,
    string Score,
    string Away,
    string Seed,
    string Action);

public sealed record HistoryActionsSection(string View, string ViewTooltip);
