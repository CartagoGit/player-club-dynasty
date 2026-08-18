namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the club picker: the header band, the "your club" badge
/// that decorates the row of the managed club, the empty state, the
/// seven column headers, and the choose action.
/// </summary>
public sealed record PickerSection(
    HeaderSection Header,
    string SummaryFormat,
    string OwnedBadge,
    string Empty,
    PickerColumnsSection Columns,
    PickerActionsSection Actions);

public sealed record PickerColumnsSection(
    string Club,
    string Country,
    string Founded,
    string Prestige,
    string Fanbase,
    string Squad,
    string Action);

public sealed record PickerActionsSection(string Choose, string ChooseTooltip);
