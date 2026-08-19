namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// Strings the career itself owns, outside any one screen: today, the
/// question asked before a career is left.
/// </summary>
/// <remarks>
/// Leaving the desk used to be one unguarded button on the command bar,
/// so a mis-click ended the session with no way back. The classic desk
/// asked, and it offered the two honest answers separately — write the
/// career down and leave, or leave it as it stands — instead of a single
/// "are you sure" that hides which one you are choosing.
/// </remarks>
public sealed record CareerSection(CareerExitSection Exit);

public sealed record CareerExitSection(
    string Title,
    string Message,
    string SaveAndLeave,
    string LeaveWithoutSaving,
    string Stay,
    string SavedFormat);
