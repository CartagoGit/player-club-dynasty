namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings for the settings menu: the header, the name of each area the
/// player can walk into (language, theme, display, data), the toggles inside
/// the display area, and the two action buttons the panel exposes
/// (<c>Save and close</c> and <c>Revert</c>).
/// </summary>
public sealed record OptionsSection(
    HeaderSection Header,
    string Fullscreen,
    string ReducedMotion,
    string DenseTables,
    string ThemeHeader,
    string ThemeHint,
    string LanguageHeader,
    string LanguageHint,
    string DisplayHeader,
    string DataHeader,
    OptionsActionsSection Actions,
    string Hint,
    OptionsRejectionMessage RejectionMessage);

/// <summary>The two action buttons on the options panel.</summary>
public sealed record OptionsActionsSection(string SaveAndClose, string Revert);

/// <summary>
/// A short, always-English message the options panel can show when the
/// player tries to apply a value the runtime rejected (e.g. an unknown
/// palette). The copy is intentionally English because the panel
/// surfaces it as a system notification, not as a translatable UI
/// string.
/// </summary>
public sealed record OptionsRejectionMessage(string Title, string Body);
