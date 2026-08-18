using Godot;
using PCDynasty.Application.Matches;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Matches;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class TacticsScreen
{
    private const string ScenePath = "res://Scenes/Matches/LineupSelection.tscn";
    private readonly ICareerLineupService _lineups;
    private readonly IManagerWorld _world;
    private readonly ManagedClubContext _managed;

    internal TacticsScreen(
        IManagerWorld world,
        ICareerLineupService lineups,
        ManagedClubContext managed)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _lineups = lineups ?? throw new ArgumentNullException(nameof(lineups));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
    }

    /// <summary>
    /// A match plan is a command, so it is always authored for the managed
    /// club even when the user is browsing a rival.
    /// </summary>
    internal void Render(Control root, ClubRow browsedClub)
    {
        ClubRow club = _managed.Owns(browsedClub.ExternalId)
            ? browsedClub
            : _world.FindClub(_managed.ClubExternalId) ?? browsedClub;
        PackedScene scene = GD.Load<PackedScene>(ScenePath)
            ?? throw new InvalidOperationException($"Could not load tactics scene '{ScenePath}'.");
        LineupSelectionController controller = scene.Instantiate<LineupSelectionController>();
        controller.Name = "LineupSelection";
        controller.CustomMinimumSize = new Vector2(0, 560);
        controller.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        controller.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        root.AddChild(controller);

        var competitions = _world.ListCompetitionsForClub(club.ExternalId);
        string contestId = competitions.Count > 0
            ? competitions[0].ExternalId
            : $"friendly:{club.ExternalId}";
        LineupCandidateView[] candidates = _world.ListPlayers(club.ExternalId)
            .OrderByDescending(player => player.Overall)
            .ThenBy(player => player.DisplayName, StringComparer.Ordinal)
            .Select(player => new LineupCandidateView(
                player.ExternalId,
                $"{player.DisplayName} · MED {player.Overall}",
                "starter"))
            .ToArray();
        var view = new LineupSelectionView(
            contestId,
            club.ExternalId,
            $"{club.DisplayName.ToUpperInvariant()} · PLAN DE PARTIDO",
            candidates);
        _lineups.TryGetLatest(club.ExternalId, out LineupPlan? savedPlan);
        controller.Bind(view, command => _lineups.Submit(_managed, command), savedPlan);
    }
}
