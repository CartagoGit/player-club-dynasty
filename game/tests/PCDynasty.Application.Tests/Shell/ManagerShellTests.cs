using PCDynasty.Application.Shell;
using Xunit;

namespace PCDynasty.Application.Tests.Shell;

public sealed class ManagerShellTests
{
    [Fact]
    public void NewShellStartsAtMainMenuAndReflectsSaveAvailability()
    {
        var emptyShell = new ManagerShell();
        var resumableShell = new ManagerShell("club-aurora-quay");

        Assert.Equal(ShellRoute.MainMenu, emptyShell.State.Route);
        Assert.False(emptyShell.State.CanContinue);
        Assert.True(resumableShell.State.CanContinue);
    }

    [Fact]
    public void NewCareerRequiresAClubBeforeEnteringTheWorkspace()
    {
        var shell = new ManagerShell();

        Assert.True(shell.BeginNewCareer().Accepted);

        ShellTransitionResult rejected = shell.ConfirmCareer();

        Assert.False(rejected.Accepted);
        Assert.Equal(ShellTransitionFailure.ClubRequired, rejected.Failure);
        Assert.Equal(ShellRoute.CareerSetup, shell.State.Route);
    }

    [Fact]
    public void SelectedClubEntersTheManagerWorkspace()
    {
        var shell = new ManagerShell();
        shell.BeginNewCareer();

        Assert.True(shell.SelectClub("club-aurora-quay").Accepted);
        ShellTransitionResult result = shell.ConfirmCareer();

        Assert.True(result.Accepted);
        Assert.Equal(ShellRoute.ManagerWorkspace, result.State.Route);
        Assert.Equal("club-aurora-quay", result.State.SelectedClubId);
    }

    [Fact]
    public void ContinueWithoutASaveIsRejectedWithoutChangingRoute()
    {
        var shell = new ManagerShell();

        ShellTransitionResult result = shell.ContinueCareer();

        Assert.False(result.Accepted);
        Assert.Equal(ShellTransitionFailure.SaveUnavailable, result.Failure);
        Assert.Equal(ShellRoute.MainMenu, shell.State.Route);
    }

    [Fact]
    public void ContinueRestoresTheSavedClub()
    {
        var shell = new ManagerShell("club-cinder-valley");

        ShellTransitionResult result = shell.ContinueCareer();

        Assert.True(result.Accepted);
        Assert.Equal(ShellRoute.ManagerWorkspace, result.State.Route);
        Assert.Equal("club-cinder-valley", result.State.SelectedClubId);
    }

    [Fact]
    public void ClubSelectionAndConfirmationRejectTheMainMenuRouteWithoutMutation()
    {
        var shell = new ManagerShell("club:saved");
        ManagerShellState before = shell.State;

        ShellTransitionResult selection = shell.SelectClub("club:other");
        ShellTransitionResult confirmation = shell.ConfirmCareer();

        AssertInvalidRoute(selection, before);
        AssertInvalidRoute(confirmation, before);
        Assert.Same(before, shell.State);
    }

    [Fact]
    public void MainMenuOnlyTransitionsRejectCareerSetupWithoutMutation()
    {
        var shell = new ManagerShell("club:saved");
        shell.BeginNewCareer();
        ManagerShellState before = shell.State;

        ShellTransitionResult[] rejected =
        [
            shell.BeginNewCareer(),
            shell.ContinueCareer(),
            shell.OpenSettings(),
            shell.OpenDataEditor(),
        ];

        Assert.All(rejected, result => AssertInvalidRoute(result, before));
        Assert.Same(before, shell.State);
    }

    [Theory]
    [InlineData(ShellRoute.Settings)]
    [InlineData(ShellRoute.DataEditor)]
    public void SecondaryMainMenuRoutesCanReturnHome(ShellRoute route)
    {
        var shell = new ManagerShell();

        ShellTransitionResult opened = route == ShellRoute.Settings
            ? shell.OpenSettings()
            : shell.OpenDataEditor();

        Assert.True(opened.Accepted);
        Assert.Equal(route, opened.State.Route);
        Assert.True(shell.ReturnToMainMenu().Accepted);
        Assert.Equal(ShellRoute.MainMenu, shell.State.Route);
    }

    private static void AssertInvalidRoute(
        ShellTransitionResult result,
        ManagerShellState expectedState)
    {
        Assert.False(result.Accepted);
        Assert.Equal(ShellTransitionFailure.InvalidRoute, result.Failure);
        Assert.Same(expectedState, result.State);
    }
}
