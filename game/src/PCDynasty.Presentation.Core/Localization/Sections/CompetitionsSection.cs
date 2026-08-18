namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the competitions screen: the header band, the empty
/// state, the read-only notice, the per-competition header template
/// and the two action buttons.
/// </summary>
public sealed record CompetitionsSection(
    HeaderSection Header,
    string SummaryFormat,
    string Empty,
    CompetitionsReadOnlySection ReadOnly,
    CompetitionEntrySection Entry,
    CompetitionActionsSection Actions);

public sealed record CompetitionsReadOnlySection(string Title, string Body);

/// <summary>
/// The strings that decorate every competition entry inside the
/// section. The <c>Eyebrow</c> is the format token the screen fills
/// with the competition format ("LIGA", "COPA", etc.).
/// </summary>
public sealed record CompetitionEntrySection(
    string Eyebrow,
    string SummaryFormat,
    string RivalsPrefix);

public sealed record CompetitionActionsSection(
    string PlayNext,
    string PlayNextTooltip,
    string PlaySeasonFormat);
