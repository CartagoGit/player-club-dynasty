namespace PCDynasty.Application.Shell;

public enum ShellRoute
{
    MainMenu = 0,
    CareerSetup = 1,
    ManagerWorkspace = 2,
    DataEditor = 3,
    Settings = 4,
}

public enum ShellTransitionFailure
{
    None = 0,
    InvalidRoute = 1,
    ClubRequired = 2,
    SaveUnavailable = 3,
}

public sealed record ManagerShellState(
    ShellRoute Route,
    string? SelectedClubId,
    bool CanContinue);

public sealed record ShellTransitionResult(
    bool Accepted,
    ManagerShellState State,
    ShellTransitionFailure Failure)
{
    internal static ShellTransitionResult Success(ManagerShellState state) =>
        new(true, state, ShellTransitionFailure.None);

    internal static ShellTransitionResult Rejected(
        ManagerShellState state,
        ShellTransitionFailure failure) =>
        new(false, state, failure);
}

public sealed class ManagerShell
{
    private string? _savedClubId;

    public ManagerShell(string? savedClubId = null)
    {
        _savedClubId = NormalizeId(savedClubId);
        State = CreateState(ShellRoute.MainMenu, selectedClubId: null);
    }

    public ManagerShellState State { get; private set; }

    public ShellTransitionResult BeginNewCareer()
    {
        if (State.Route != ShellRoute.MainMenu)
        {
            return Reject(ShellTransitionFailure.InvalidRoute);
        }

        return MoveTo(ShellRoute.CareerSetup, selectedClubId: null);
    }

    public ShellTransitionResult SelectClub(string? clubId)
    {
        if (State.Route != ShellRoute.CareerSetup)
        {
            return Reject(ShellTransitionFailure.InvalidRoute);
        }

        string? normalizedId = NormalizeId(clubId);
        if (normalizedId is null)
        {
            return Reject(ShellTransitionFailure.ClubRequired);
        }

        return MoveTo(ShellRoute.CareerSetup, normalizedId);
    }

    public ShellTransitionResult ConfirmCareer()
    {
        if (State.Route != ShellRoute.CareerSetup)
        {
            return Reject(ShellTransitionFailure.InvalidRoute);
        }

        if (State.SelectedClubId is null)
        {
            return Reject(ShellTransitionFailure.ClubRequired);
        }

        _savedClubId = State.SelectedClubId;
        return MoveTo(ShellRoute.ManagerWorkspace, _savedClubId);
    }

    public ShellTransitionResult ContinueCareer()
    {
        if (State.Route != ShellRoute.MainMenu)
        {
            return Reject(ShellTransitionFailure.InvalidRoute);
        }

        if (_savedClubId is null)
        {
            return Reject(ShellTransitionFailure.SaveUnavailable);
        }

        return MoveTo(ShellRoute.ManagerWorkspace, _savedClubId);
    }

    public ShellTransitionResult OpenSettings() => OpenMainMenuRoute(ShellRoute.Settings);

    public ShellTransitionResult OpenDataEditor() => OpenMainMenuRoute(ShellRoute.DataEditor);

    public ShellTransitionResult ReturnToMainMenu() => MoveTo(ShellRoute.MainMenu, selectedClubId: null);

    private ShellTransitionResult OpenMainMenuRoute(ShellRoute route)
    {
        if (State.Route != ShellRoute.MainMenu)
        {
            return Reject(ShellTransitionFailure.InvalidRoute);
        }

        return MoveTo(route, selectedClubId: null);
    }

    private ShellTransitionResult MoveTo(ShellRoute route, string? selectedClubId)
    {
        State = CreateState(route, selectedClubId);
        return ShellTransitionResult.Success(State);
    }

    private ShellTransitionResult Reject(ShellTransitionFailure failure) =>
        ShellTransitionResult.Rejected(State, failure);

    private ManagerShellState CreateState(ShellRoute route, string? selectedClubId) =>
        new(route, selectedClubId, _savedClubId is not null);

    private static string? NormalizeId(string? id) =>
        string.IsNullOrWhiteSpace(id) ? null : id.Trim();
}
