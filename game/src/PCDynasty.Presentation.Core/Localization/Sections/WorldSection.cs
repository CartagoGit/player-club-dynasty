namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the world screen: the header band, the empty state, the
/// five column headers, and the per-row action button.
/// </summary>
public sealed record WorldSection(
    HeaderSection Header,
    string SummaryFormat,
    string Empty,
    WorldColumnsSection Columns,
    WorldActionsSection Actions);

public sealed record WorldColumnsSection(
    string Name,
    string Format,
    string Frequency,
    string Rivals,
    string Action);

public sealed record WorldActionsSection(string Inspect, string InspectTooltip);
