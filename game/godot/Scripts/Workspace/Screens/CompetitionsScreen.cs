using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;
using PCDynasty.Presentation.Workspace.Persistence;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class CompetitionsScreen
{
    private readonly IManagerWorld _world;
    private readonly CareerHistoryPersistence _history;
    private readonly Action _refresh;
    private readonly Action<string> _setStatus;

    internal CompetitionsScreen(
        IManagerWorld world,
        CareerHistoryPersistence history,
        Action refresh,
        Action<string> setStatus)
    {
        _world = world;
        _history = history;
        _refresh = refresh;
        _setStatus = setStatus;
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

        long seed = CreateSeed();
        MatchResult result = MatchSimulator.Simulate(club, rival, seed);
        _history.Save(new HistoryEntry(
            CompetitionDisplayName: competition.DisplayName,
            HomeDisplayName: club.DisplayName,
            AwayDisplayName: rival.DisplayName,
            HomeClubId: club.ExternalId,
            AwayClubId: rival.ExternalId,
            HomeGoals: result.HomeGoals,
            AwayGoals: result.AwayGoals,
            Seed: result.Seed,
            PlayedAtUtc: DateTime.UtcNow));
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
        MatchResult replay = MatchSimulator.Simulate(previous.HomeClub, previous.AwayClub, previous.Seed);
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

        SeasonResult season = SeasonSimulator.Simulate(
            competition.ExternalId,
            competition.DisplayName,
            participants,
            CreateSeed());
        DateTime playedAtUtc = DateTime.UtcNow;
        foreach (SeasonFixture fixture in season.Fixtures)
        {
            _history.Save(new HistoryEntry(
                CompetitionDisplayName: competition.DisplayName,
                HomeDisplayName: fixture.HomeDisplayName,
                AwayDisplayName: fixture.AwayDisplayName,
                HomeClubId: fixture.HomeClubId,
                AwayClubId: fixture.AwayClubId,
                HomeGoals: fixture.HomeGoals,
                AwayGoals: fixture.AwayGoals,
                Seed: 0,
                PlayedAtUtc: playedAtUtc));
        }

        StandingRow? managedStanding = season.Standings.FirstOrDefault(standing => standing.ClubExternalId == club.ExternalId);
        if (managedStanding is not null)
        {
            _history.Save(new SeasonHistoryEntry(
                CompetitionDisplayName: competition.DisplayName,
                ManagedClubId: club.ExternalId,
                ManagedClubDisplayName: club.DisplayName,
                ManagedPosition: SeasonReportView.PositionOf(season, club.ExternalId),
                ManagedPoints: managedStanding.Points,
                ManagedGoalsFor: managedStanding.GoalsFor,
                ManagedGoalsAgainst: managedStanding.GoalsAgainst,
                TotalClubs: season.Standings.Length,
                TotalFixtures: season.Fixtures.Length,
                PlayedAtUtc: playedAtUtc,
                FinalStandings: season.Standings));
        }

        WorkspaceUiFactory.Clear(root);
        root.AddChild(SeasonReportView.Build(club, season));
        if (managedStanding is not null)
        {
            _setStatus($"● {competition.DisplayName} · puesto {SeasonReportView.PositionOf(season, club.ExternalId)} · {managedStanding.Points} pts");
        }
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

    private static long CreateSeed() => unchecked((long)(DateTime.UtcNow.Ticks & 0x7FFFFFFFFFFFFFFF));
}
