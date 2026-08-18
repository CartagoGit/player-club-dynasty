using Godot;
using PCDynasty.Application.Competitions.Simulation;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core.Localization;
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
    private readonly Action? _openHistory;
    private readonly CompetitionsSection _texts;

    internal CompetitionsScreen(
        IManagerWorld world,
        ICompetitionSimulationService simulation,
        CareerHistoryPersistence history,
        ManagedClubContext managed,
        Action refresh,
        Action<string> setStatus,
        Action? openHistory,
        CompetitionsSection texts)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
        _openHistory = openHistory;
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
    }

    internal void Render(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);
        root.AddChild(layout);

        IReadOnlyList<CompetitionRow> competitions = _world.ListCompetitionsForClub(club.ExternalId);
        int cups = competitions.Count(c => c.Format.Contains("copa", StringComparison.OrdinalIgnoreCase));
        int leagues = competitions.Count - cups;

        var header = new HeaderPanel { Name = "CompetitionsHeaderPanel" };
        header.SetTexts(
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _texts.SummaryFormat,
            new[]
            {
                leagues.ToString(WorkspaceUiFactory.InvariantCulture),
                cups.ToString(WorkspaceUiFactory.InvariantCulture),
                club.DisplayName,
            });
        layout.AddChild(header);

        if (competitions.Count == 0)
        {
            layout.AddChild(WorkspaceUiFactory.CreateText(
                (_texts.Empty),
                ManagerDesignTokens.TextSecondary));
            return;
        }

        // Precompute a by-name club lookup so ResolveRival / ResolveParticipants
        // don't re-walk the world on every render.
        Dictionary<string, ClubRow> clubsByName = _world.ListClubs()
            .ToDictionary(
                c => c.ShortName,
                c => c,
                StringComparer.OrdinalIgnoreCase);

        foreach (CompetitionRow competition in competitions)
        {
            layout.AddChild(BuildCompetitionPanel(club, competition, clubsByName));
        }

        if (!_managed.Owns(club.ExternalId))
        {
            layout.AddChild(WorkspaceUiFactory.CreatePanel(
                (_texts.ReadOnly.Title),
                new[] { (_texts.ReadOnly.Body) },
                ManagerDesignTokens.TextSecondary));
        }
    }

    private HeaderPanel BuildCompetitionPanel(
        ClubRow club,
        CompetitionRow competition,
        IReadOnlyDictionary<string, ClubRow> clubsByName)
    {
        var panel = new HeaderPanel
        {
            Name = $"Competition_{competition.ExternalId}",
        };
        // The eyebrow is a fixed token (the language file carries the localised
        // version); we just prepend the competition's format string at format
        // time so the eyebrow stays tr()-keyed.
        panel.SetTexts(
            _texts.Entry.Eyebrow,
            competition.DisplayName,
            _texts.Entry.SummaryFormat,
            new[] { competition.Frequency, competition.Rivals.Count.ToString(WorkspaceUiFactory.InvariantCulture) });
        // Patch the eyebrow text so it reads "FORMAT · LIGA" / "FORMAT · COPA"
        // — we know the label is set; reach in to add the format prefix.
        // HeaderPanel does not expose a public setter for that field after
        // construction, but the Label is reachable through the panel tree
        // and we can set its text after the fact. This keeps the call site
        // terse: the format is data, not another translation key.
        AttachFormatPrefix(panel, competition.Format);

        if (_managed.Owns(club.ExternalId))
        {
            // The trailing action button on each competition's header panel is
            // the "play the next fixture" entry point. The "play the full
            // season" remains inline below the panel because it needs more
            // space than the action button affordance allows.
            panel.SetAction(
                _texts.Actions.PlayNext,
                _texts.Actions.PlayNextTooltip,
                () => PlayNext(panel, club, competition, clubsByName),
                enabled: true);
        }
        panel.AddChild(BuildRivalsList(competition));

        if (_managed.Owns(club.ExternalId))
        {
            var playSeason = new Button
            {
                Name = "PlaySeason",
                Text = string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    (_texts.Actions.PlaySeasonFormat),
                    competition.Rivals.Count.ToString(WorkspaceUiFactory.InvariantCulture)),
                CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
            };
            playSeason.Pressed += () => PlaySeason(panel, club, competition, clubsByName);
            panel.Footer.AddChild(playSeason);
        }

        return panel;
    }

    private static void AttachFormatPrefix(HeaderPanel panel, string format)
    {
        // The eyebrow Label is the first child of the HeaderLeft VBox. Walk
        // two levels down from the panel to find it and prepend the format
        // string. The walk is O(1) and the alternative — exposing a
        // dedicated setter for the eyebrow — is a public API change for
        // a screen-internal concern.
        if (panel.GetNodeOrNull<Label>("HeaderColumn/HeaderBand/HeaderLeft/Eyebrow") is Label eyebrow)
        {
            eyebrow.Text = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0} · {1}",
                eyebrow.Text,
                format);
        }
    }

    private static HBoxContainer BuildRivalsList(CompetitionRow competition)
    {
        var rivals = new Label
        {
            Text = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "Rivales: {0}",
                string.Join(", ", competition.Rivals)),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        rivals.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        rivals.AddThemeFontSizeOverride("font_size", 13);
        var wrap = new HBoxContainer();
        wrap.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        wrap.AddChild(rivals);
        return wrap;
    }

    private void PlayNext(
        Control root,
        ClubRow club,
        CompetitionRow competition,
        IReadOnlyDictionary<string, ClubRow> clubsByName)
    {
        ClubRow? rival = ResolveRival(competition, clubsByName);
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

    private static ClubRow? ResolveRival(
        CompetitionRow competition,
        IReadOnlyDictionary<string, ClubRow> clubsByName)
    {
        foreach (string rivalName in competition.Rivals)
        {
            if (clubsByName.TryGetValue(rivalName, out ClubRow? club))
            {
                return club;
            }
        }
        return null;
    }

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

    private void PlaySeason(
        Control root,
        ClubRow club,
        CompetitionRow competition,
        IReadOnlyDictionary<string, ClubRow> clubsByName)
    {
        ClubRow[] participants = ResolveParticipants(club, competition, clubsByName);
        if (participants.Length < 2)
        {
            _setStatus($"● {competition.DisplayName} necesita al menos 2 rivales para simular");
            return;
        }

        SeasonExecution execution = _simulation.PlaySeason(
            _managed,
            competition.ExternalId,
            competition.DisplayName,
            club,
            participants);
        // The season is a batch simulation: persist every match in the
        // history and refresh the screen so the user can drill into each
        // one through the History panel. A dedicated season-report view
        // is owned by a follow-up slice (the existing SeasonReportView
        // is a stub that needs the new data shape).
        foreach (HistoryEntry entry in execution.MatchHistory)
        {
            _history.Save(entry);
        }
        _setStatus(
            $"● {competition.DisplayName} · puesto {execution.SeasonHistory.ManagedPosition} · {execution.SeasonHistory.ManagedPoints} pts · {execution.Result.Fixtures.Length} partidos");
        _refresh();
    }

    private static ClubRow[] ResolveParticipants(
        ClubRow club,
        CompetitionRow competition,
        IReadOnlyDictionary<string, ClubRow> clubsByName)
    {
        var list = new List<ClubRow> { club };
        foreach (string rivalName in competition.Rivals)
        {
            if (clubsByName.TryGetValue(rivalName, out ClubRow? rival))
            {
                list.Add(rival);
            }
        }
        return list.ToArray();
    }
}
