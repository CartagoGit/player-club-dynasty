using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;
using PCDynasty.Presentation.Workspace.Persistence;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class CompetitionsScreen
{
    private readonly IManagerWorld _world;
    private readonly ICompetitionSimulationService _simulation;
    private readonly CareerHistoryPersistence _history;
    private readonly ManagedClubContext _managed;
    private readonly Action _refresh;
    private readonly Action<string> _setStatus;

    internal CompetitionsScreen(
        IManagerWorld world,
        ICompetitionSimulationService simulation,
        CareerHistoryPersistence history,
        ManagedClubContext managed,
        Action refresh,
        Action<string> setStatus)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
    }

    internal void Render(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var header = new Label { Text = $"{club.DisplayName.ToUpperInvariant()} · COMPETICIONES DE LA TEMPORADA" };
        header.AddThemeFontSizeOverride("font_size", 18);
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        layout.AddChild(header);

        IReadOnlyList<CompetitionRow> competitions = _world.ListCompetitionsForClub(club.ExternalId);
        if (competitions.Count == 0)
        {
            layout.AddChild(WorkspaceUiFactory.CreateText("(sin competiciones inscritas)", ManagerDesignTokens.TextSecondary));
        }
        else
        {
            foreach (CompetitionRow competition in competitions)
            {
                layout.AddChild(BuildCompetitionRow(root, club, competition));
            }
        }

        root.AddChild(layout);
    }

    private PanelContainer BuildCompetitionRow(Control root, ClubRow club, CompetitionRow competition)
    {
        PanelContainer panel = WorkspaceUiFactory.CreateSurfacePanel();
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var title = new Label { Text = competition.DisplayName };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 16);
        column.AddChild(title);

        var meta = new Label { Text = $"{competition.Format} · {competition.Frequency} · {competition.Rivals.Count} rivales" };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        column.AddChild(meta);
        var rivals = new Label
        {
            Text = "Rivales: " + string.Join(", ", competition.Rivals),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        rivals.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        rivals.AddThemeFontSizeOverride("font_size", 13);
        column.AddChild(rivals);

        // Playing a fixture writes career facts, so the actions exist only for
        // the managed club; a browsed rival stays a read-only view.
        if (_managed.Owns(club.ExternalId))
        {
            var playButton = new Button { Text = "  Jugar partido vs primer rival", Alignment = HorizontalAlignment.Left };
            playButton.Pressed += () => PlayMatch(root, club, competition);
            column.AddChild(playButton);
            var seasonButton = new Button
            {
                Text = $"  Jugar temporada completa ({competition.Rivals.Count} partidos)",
                Alignment = HorizontalAlignment.Left,
            };
            seasonButton.Pressed += () => PlaySeason(root, club, competition);
            column.AddChild(seasonButton);
        }
        else
        {
            column.AddChild(WorkspaceUiFactory.CreateText(
                "Sólo puedes disputar los partidos de tu club.",
                ManagerDesignTokens.TextSecondary));
        }

        panel.AddChild(column);
        return panel;
    }

    private void PlayMatch(Control root, ClubRow club, CompetitionRow competition)
    {
        ClubRow? rival = ResolveRival(competition);
        if (rival is null)
        {
            return;
        }

        MatchExecution execution = _simulation.PlayMatch(_managed, competition.DisplayName, club, rival);
        MatchResult result = execution.Result;
        _history.Save(execution.History);
        ShowMatch(root, result, competition.DisplayName);

        string winner = result.HomeGoals > result.AwayGoals
            ? club.ShortName
            : result.AwayGoals > result.HomeGoals
                ? rival.ShortName
                : "Empate";
        _setStatus($"● {club.ShortName} {result.HomeGoals}-{result.AwayGoals} {rival.ShortName} · {winner}");
    }

    private ClubRow? ResolveRival(CompetitionRow competition) => competition.Rivals
        .Select(rivalName => _world.ListClubs().FirstOrDefault(club =>
            string.Equals(club.ShortName, rivalName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(club.DisplayName, rivalName, StringComparison.OrdinalIgnoreCase)))
        .FirstOrDefault(club => club is not null);

    private void ShowMatch(Control root, MatchResult result, string competitionDisplayName)
    {
        WorkspaceUiFactory.Clear(root);
        root.AddChild(MatchReportView.Build(
            result,
            competitionDisplayName,
            replay: () => Replay(root, result, competitionDisplayName),
            back: _refresh));
    }

    private void Replay(Control root, MatchResult previous, string competitionDisplayName)
    {
        MatchResult replay = _simulation.Replay(previous);
        ShowMatch(root, replay, competitionDisplayName);
        bool identical = replay.HomeGoals == previous.HomeGoals && replay.AwayGoals == previous.AwayGoals;
        _setStatus(identical
            ? $"● Repetición · {replay.HomeGoals}-{replay.AwayGoals} (idéntica, seed {previous.Seed})"
            : $"● Repetición · {replay.HomeGoals}-{replay.AwayGoals} (difiere — seed no determinista)");
    }

    private void PlaySeason(Control root, ClubRow club, CompetitionRow competition)
    {
        ClubRow[] participants = ResolveParticipants(club, competition);
        if (participants.Length < 2)
        {
            return;
        }

        SeasonExecution execution = _simulation.PlaySeason(
            _managed,
            competition.ExternalId,
            competition.DisplayName,
            club,
            participants);
        foreach (HistoryEntry match in execution.MatchHistory)
        {
            _history.Save(match);
        }

        _history.Save(execution.SeasonHistory);

        SeasonResult season = execution.Result;
        WorkspaceUiFactory.Clear(root);
        root.AddChild(SeasonReportView.Build(club, season));
        _setStatus($"● {competition.DisplayName} · puesto {execution.SeasonHistory.ManagedPosition} · {execution.SeasonHistory.ManagedPoints} pts");
    }

    private ClubRow[] ResolveParticipants(ClubRow managedClub, CompetitionRow competition)
    {
        IReadOnlyList<ClubRow> clubs = _world.ListClubs();
        return competition.Rivals
            .Select(rivalName => clubs.FirstOrDefault(club =>
                string.Equals(club.ShortName, rivalName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(club.DisplayName, rivalName, StringComparison.OrdinalIgnoreCase)))
            .Where(club => club is not null)
            .Prepend(managedClub)
            .DistinctBy(club => club!.ExternalId, StringComparer.Ordinal)
            .Cast<ClubRow>()
            .ToArray();
    }
}
