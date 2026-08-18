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

    internal TacticsScreen(IManagerWorld world, ICareerLineupService lineups)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _lineups = lineups ?? throw new ArgumentNullException(nameof(lineups));
    }

    internal void Render(Control root, ClubRow club)
    {
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
        controller.Bind(view, command => _lineups.Submit(command), savedPlan);
    }
}
