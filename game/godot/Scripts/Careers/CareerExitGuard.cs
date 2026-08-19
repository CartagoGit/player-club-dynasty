using Godot;
using PCDynasty.Contracts.Careers;
using PCDynasty.Persistence.Sqlite;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Careers;

/// <summary>
/// Stands between "main menu" and actually leaving the career.
/// </summary>
/// <remarks>
/// Leaving the desk used to be a single unguarded button on the command
/// bar, so one mis-click ended the session with nothing asked. This guard
/// puts the question in front of it and offers the two honest answers
/// separately — write the career down and leave, or leave it as it stands —
/// instead of one "are you sure" that hides which is which.
///
/// It owns the profile write as well as the dialog because the two belong
/// together: the only difference between the answers is whether the profile
/// is stamped before the workspace closes.
/// </remarks>
internal sealed class CareerExitGuard
{
    private readonly Node _host;
    private readonly SqliteCareerProfileStore _profiles;
    private readonly TimeProvider _timeProvider;

    internal CareerExitGuard(Node host, SqliteCareerProfileStore profiles, TimeProvider timeProvider)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Ask, then run <paramref name="leave"/> only if the player chose to.
    /// The dialog is added to the host, so it covers the whole shell and
    /// nothing behind it can be clicked while the question stands.
    /// </summary>
    internal void Ask(CareerExitSection copy, string clubExternalId, Action leave)
    {
        ArgumentNullException.ThrowIfNull(copy);
        ArgumentNullException.ThrowIfNull(leave);

        var dialog = new ConfirmDialog(copy.Title, copy.Message, ShellArea.Tracking);
        dialog.AddCommand(
            "SaveAndLeave",
            copy.SaveAndLeave,
            () =>
            {
                Save(clubExternalId);
                leave();
            },
            ClassicChrome.Intent.Advance);
        dialog.AddCommand(
            "LeaveWithoutSaving",
            copy.LeaveWithoutSaving,
            leave,
            ClassicChrome.Intent.Destructive);
        dialog.AddCommand("Stay", copy.Stay, () => { }, ClassicChrome.Intent.Neutral, isCancel: true);
        _host.AddChild(dialog);
    }

    /// <summary>
    /// Stamp the profile so "Continue" reopens where the player stopped.
    /// Squad, market, finances and history are already written through to
    /// the career database as they change; what the profile adds is the
    /// moment the player last held the career.
    /// </summary>
    internal void Save(string clubExternalId)
    {
        if (string.IsNullOrWhiteSpace(clubExternalId))
        {
            return;
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        bool known = _profiles.TryLoad(out CareerProfile? existing);
        DateTimeOffset startedAt = known ? existing!.StartedAtUtc : now;
        string clubName = known ? existing!.ClubDisplayName : clubExternalId;
        _profiles.Save(new CareerProfile(clubExternalId, clubName, startedAt, now));
    }
}
