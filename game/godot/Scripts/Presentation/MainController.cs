using System.Globalization;
using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Presentation;

/// <summary>
/// Owns the presentation root. Listens to the sidebar buttons and rebuilds
/// the central panel whenever the user picks a different section.
///
/// The visual style targets the dense, information-rich layout of classic
/// football managers (PCF6-style): navy canvas, amber accents, tabular
/// metric rows instead of free-floating cards, no decorative chrome.
/// </summary>
/// <remarks>
/// This controller is intentionally thin: it never reaches into the domain
/// directly. All domain access goes through <see cref="WorldService"/> via
/// the engine-neutral projections.
/// </remarks>
public partial class MainController : Control
{
    [Export] public NodePath ContentRootPath { get; set; } = new();
    [Export] public NodePath StatusLabelPath { get; set; } = new();
    [Export] public NodePath OverviewButtonPath { get; set; } = new();
    [Export] public NodePath SquadButtonPath { get; set; } = new();
    [Export] public NodePath CompetitionsButtonPath { get; set; } = new();
    [Export] public NodePath HistoryButtonPath { get; set; } = new();
    [Export] public NodePath WorldButtonPath { get; set; } = new();

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private WorldService? _service;
    private string _selectedSection = "resumen";
    private string? _selectedClubId;

    public override void _Ready()
    {
        _service = WorldService.Instance;
        if (_service is null)
        {
            GD.PushError("[MainController] WorldService was not initialized before _Ready.");
            return;
        }

        BindSidebar();
        RenderCurrent();
    }

    private void BindSidebar()
    {
        Wire(OverviewButtonPath, "resumen");
        Wire(SquadButtonPath, "plantilla");
        Wire(CompetitionsButtonPath, "competiciones");
        Wire(HistoryButtonPath, "historial");
        Wire(WorldButtonPath, "mundial");
    }

    private void Wire(NodePath path, string sectionId)
    {
        var button = GetNodeOrNull<Button>(path);
        if (button is not null)
        {
            string captured = sectionId;
            button.Pressed += () => SelectSection(captured);
        }
    }

    private void SelectSection(string sectionId)
    {
        _selectedSection = sectionId;
        RenderCurrent();
    }

    private void RenderCurrent()
    {
        var root = GetNodeOrNull<Control>(ContentRootPath);
        var status = GetNodeOrNull<Label>(StatusLabelPath);
        if (root is null)
        {
            return;
        }

        ClearChildren(root);

        var clubs = _service?.ListClubs() ?? Array.Empty<ClubRow>();
        _selectedClubId ??= clubs.Count > 0 ? clubs[0].ExternalId : null;

        if (_selectedClubId is null)
        {
            root.AddChild(MakeText("No hay clubs cargados.", ManagerDesignTokens.TextSecondary));
            return;
        }

        var club = clubs.FirstOrDefault(c => c.ExternalId == _selectedClubId);
        if (club is null)
        {
            status?.SetText("● Club no encontrado");
            return;
        }

        if (_selectedSection == "club_picker")
        {
            RenderClubPicker(root, clubs);
            status?.SetText("● Selección de club");
            return;
        }

        switch (_selectedSection)
        {
            case "plantilla":
                RenderSquad(root, club);
                status?.SetText($"● {club.DisplayName} · plantilla");
                break;
            case "competiciones":
                RenderCompetitions(root, club);
                status?.SetText($"● {club.DisplayName} · competiciones");
                break;
            case "historial":
                RenderHistory(root);
                status?.SetText($"● Historial de partidos");
                break;
            case "mundial":
                RenderWorldOverview(root);
                status?.SetText($"● Mundial de competiciones");
                break;
            default:
                RenderClubOverview(root, club);
                status?.SetText($"● {club.DisplayName}");
                break;
        }
    }

    // ----- Resumen --------------------------------------------------------

    private void RenderClubOverview(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);

        layout.AddChild(BuildClubHeader(club));

        var identity = new GridContainer { Columns = 4 };
        identity.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space16);
        identity.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space8);
        identity.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddField(identity, "NOMBRE CORTO", club.DisplayName);
        AddField(identity, "CIUDAD", club.City);
        AddField(identity, "PAÍS", club.CountryShortName);
        AddField(identity, "FUNDADO", club.FoundedYear);
        AddField(identity, "PRESTIGIO", club.Prestige.ToString(Inv));
        AddField(identity, "AFICIÓN", club.Fanbase.ToString(Inv));
        AddField(identity, "PLANTILLA", club.SquadSize.ToString(Inv));
        AddField(identity, "ETAPA", "Pretemporada");
        layout.AddChild(identity);

        layout.AddChild(BuildPanel(
            "ATENCIÓN",
            ["Sin alertas activas — temporada en pretemporada."],
            ManagerDesignTokens.TextSecondary));

        layout.AddChild(BuildUpcomingFixturesPanel(club));

        layout.AddChild(BuildPanel(
            "SIGUIENTES DECISIONES",
            ["Avanzar pretemporada", "Revisar plantilla", "Configurar alineación"],
            ManagerDesignTokens.Accent));

        root.AddChild(layout);
    }

    private static PanelContainer BuildUpcomingFixturesPanel(ClubRow club)
    {
        var played = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in MatchHistory.Instance.Snapshot())
        {
            if (string.Equals(entry.HomeClubId, club.ExternalId, StringComparison.Ordinal))
            {
                played.Add(entry.AwayClubId);
            }
            else if (string.Equals(entry.AwayClubId, club.ExternalId, StringComparison.Ordinal))
            {
                played.Add(entry.HomeClubId);
            }
        }

        var clubs = WorldService.Instance?.ListClubs() ?? Array.Empty<ClubRow>();
        var upcoming = clubs
            .Where(other => !string.Equals(other.ExternalId, club.ExternalId, StringComparison.Ordinal))
            .Where(other => !played.Contains(other.ExternalId))
            .OrderBy(other => other.DisplayName, StringComparer.Ordinal)
            .ToArray();

        var lines = new List<string>();
        if (upcoming.Length == 0)
        {
            lines.Add("Has jugado contra todos los clubes — completa la pretemporada.");
        }
        else
        {
            lines.Add($"{upcoming.Length} rivales pendientes:");
            foreach (var rival in upcoming.Take(6))
            {
                lines.Add($"· {rival.DisplayName} · {rival.CountryShortName}");
            }
            if (upcoming.Length > 6)
            {
                lines.Add($"· y {upcoming.Length - 6} más...");
            }
        }

        return BuildPanel(
            "PRÓXIMOS RIVALES",
            lines.ToArray(),
            ManagerDesignTokens.Accent);
    }

    private HBoxContainer BuildClubHeader(ClubRow club)
    {
        var band = new HBoxContainer();
        band.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);

        var crest = new PanelContainer();
        crest.CustomMinimumSize = new Vector2(72, 72);
        var crestStyle = new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.Accent,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = 8,
            ContentMarginTop = 8,
            ContentMarginRight = 8,
            ContentMarginBottom = 8,
        };
        crest.AddThemeStyleboxOverride("panel", crestStyle);
        var initials = new Label
        {
            Text = Initials(club.DisplayName),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        initials.AddThemeColorOverride("font_color", ManagerDesignTokens.Canvas);
        initials.AddThemeFontSizeOverride("font_size", 28);
        crest.AddChild(initials);
        band.AddChild(crest);

        var meta = new VBoxContainer();
        meta.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var nameLabel = new Label { Text = club.DisplayName };
        nameLabel.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        nameLabel.AddThemeFontSizeOverride("font_size", 32);
        meta.AddChild(nameLabel);
        var subLabel = new Label
        {
            Text = $"{club.City} · {club.CountryShortName} · fundado {club.FoundedYear}",
        };
        subLabel.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        subLabel.AddThemeFontSizeOverride("font_size", 14);
        meta.AddChild(subLabel);
        band.AddChild(meta);

        var spacer = new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        band.AddChild(spacer);

        var phase = new Label { Text = "PRETEMPORADA" };
        phase.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        phase.AddThemeFontSizeOverride("font_size", 16);
        band.AddChild(phase);

        var pickBtn = new Button { Text = "Cambiar club" };
        pickBtn.AddThemeFontSizeOverride("font_size", 12);
        pickBtn.Pressed += OpenClubPicker;
        band.AddChild(pickBtn);

        return band;
    }

    private void OpenClubPicker()
    {
        _selectedSection = "club_picker";
        RenderCurrent();
    }

    // ----- Plantilla ------------------------------------------------------

    private static void RenderSquad(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var header = new Label
        {
            Text = $"{club.DisplayName.ToUpperInvariant()} · PLANTILLA · {club.SquadSize} JUGADORES",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddColumnHeader(headerRow, "JUGADOR", 280);
        AddColumnHeader(headerRow, "NAC.", 80);
        AddColumnHeader(headerRow, "EDAD", 60);
        AddColumnHeader(headerRow, "TEC", 60, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "FOR", 60, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "FIS", 60, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "MEN", 60, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "VAL", 80, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        var players = WorldService.Instance?.ListPlayers(club.ExternalId) ?? Array.Empty<PlayerRow>();
        if (players.Count == 0)
        {
            var empty = new Label { Text = "(sin jugadores inscritos)" };
            empty.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            layout.AddChild(empty);
        }
        else
        {
            foreach (var player in players)
            {
                layout.AddChild(BuildSquadRow(player));
            }
        }

        root.AddChild(layout);
    }

    private static HBoxContainer BuildSquadRow(PlayerRow player)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        AddColumnValue(row, player.DisplayName, 280, ManagerDesignTokens.TextPrimary);
        AddColumnValue(row, ExtractCountry(player.ExternalId), 80, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, Age(player.BirthYear), 60, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, player.Technique.ToString(Inv), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        AddColumnValue(row, player.Form.ToString(Inv), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        AddColumnValue(row, player.Physical.ToString(Inv), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        AddColumnValue(row, player.Mental.ToString(Inv), 60, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);

        var val = player.Overall;
        AddColumnValue(row, val.ToString(Inv), 80, ValueColor(val), HorizontalAlignment.Right);

        return row;
    }

    // ----- Competiciones --------------------------------------------------

    private void RenderCompetitions(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var header = new Label
        {
            Text = $"{club.DisplayName.ToUpperInvariant()} · COMPETICIONES DE LA TEMPORADA",
        };
        header.AddThemeFontSizeOverride("font_size", 18);
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        layout.AddChild(header);

        var competitions = WorldService.Instance?.ListCompetitionsForClub(club.ExternalId) ?? Array.Empty<CompetitionRow>();
        if (competitions.Count == 0)
        {
            var empty = new Label { Text = "(sin competiciones inscritas)" };
            empty.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            layout.AddChild(empty);
        }
        else
        {
            foreach (var competition in competitions)
            {
                layout.AddChild(BuildCompetitionRow(club, competition));
            }
        }

        root.AddChild(layout);
    }

    private PanelContainer BuildCompetitionRow(ClubRow club, CompetitionRow competition)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.Surface,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space12,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space12,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);

        var title = new Label { Text = competition.DisplayName };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 16);
        col.AddChild(title);

        var meta = new Label
        {
            Text = $"{competition.Format} · {competition.Frequency} · {competition.Rivals.Count} rivales",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        col.AddChild(meta);

        var rivals = new Label
        {
            Text = "Rivales: " + string.Join(", ", competition.Rivals),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        rivals.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        rivals.AddThemeFontSizeOverride("font_size", 13);
        col.AddChild(rivals);

        var playButton = new Button
        {
            Text = $"  Jugar partido vs primer rival",
            Alignment = HorizontalAlignment.Left,
        };
        playButton.Pressed += () => PlayMatch(club, competition);
        col.AddChild(playButton);

        var seasonButton = new Button
        {
            Text = $"  Jugar temporada completa ({competition.Rivals.Count} partidos)",
            Alignment = HorizontalAlignment.Left,
        };
        seasonButton.Pressed += () => PlaySeason(club, competition);
        col.AddChild(seasonButton);

        panel.AddChild(col);
        return panel;
    }

    private void PlayMatch(ClubRow club, CompetitionRow competition)
    {
        if (competition.Rivals.Count == 0 || _service is null)
        {
            return;
        }

        // Resolve the rival's full ClubRow. Fall back to a deterministic
        // search by short-name prefix if the cross-reference lookup fails.
        ClubRow? rival = competition.Rivals
            .Select(rivalName => _service!.ListClubs().FirstOrDefault(c =>
                string.Equals(c.ShortName, rivalName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.DisplayName, rivalName, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(match => match is not null);

        if (rival is null)
        {
            return;
        }

        long seed = unchecked((long)(DateTime.UtcNow.Ticks & 0x7FFFFFFFFFFFFFFF));
        var result = MatchSimulator.Simulate(club, rival, seed);

        MatchHistory.Instance.Append(new HistoryEntry(
            CompetitionDisplayName: competition.DisplayName,
            HomeDisplayName: club.DisplayName,
            AwayDisplayName: rival.DisplayName,
            HomeClubId: club.ExternalId,
            AwayClubId: rival.ExternalId,
            HomeGoals: result.HomeGoals,
            AwayGoals: result.AwayGoals,
            Seed: result.Seed,
            PlayedAtUtc: DateTime.UtcNow));

        var root = GetNodeOrNull<Control>(ContentRootPath);
        var status = GetNodeOrNull<Label>(StatusLabelPath);
        if (root is null)
        {
            return;
        }

        ClearChildren(root);
        root.AddChild(BuildMatchReport(club, result, competition.DisplayName));

        string winner = result.HomeGoals > result.AwayGoals
            ? club.ShortName
            : result.AwayGoals > result.HomeGoals
                ? rival.ShortName
                : "Empate";
        status?.SetText($"● {club.ShortName} {result.HomeGoals}-{result.AwayGoals} {rival.ShortName} · {winner}");
    }

    private VBoxContainer BuildMatchReport(ClubRow club, MatchResult result, string competitionDisplayName)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        // Scoreboard
        var scoreboard = new HBoxContainer();
        scoreboard.AddThemeConstantOverride("separation", ManagerDesignTokens.Space24);
        scoreboard.Alignment = BoxContainer.AlignmentMode.Center;

        var homeName = new Label { Text = result.HomeClub.ShortName.ToUpperInvariant() };
        homeName.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        homeName.AddThemeFontSizeOverride("font_size", 22);
        scoreboard.AddChild(homeName);

        var score = new Label { Text = $"{result.HomeGoals} - {result.AwayGoals}" };
        score.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        score.AddThemeFontSizeOverride("font_size", 32);
        scoreboard.AddChild(score);

        var awayName = new Label { Text = result.AwayClub.ShortName.ToUpperInvariant() };
        awayName.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        awayName.AddThemeFontSizeOverride("font_size", 22);
        scoreboard.AddChild(awayName);

        layout.AddChild(scoreboard);

        var stats = MatchStats.Compute(result);

        var meta = new Label
        {
            Text = $"{competitionDisplayName} · seed {result.Seed} · {result.Events.Count} intentos · {result.Events.Count(e => e.IsGoal)} goles",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(meta);

        // Stats panel: shots, saves, misses, cards, possession.
        layout.AddChild(BuildStatsTable(stats));

        // Event timeline (only goals).
        var goalsHeader = new Label { Text = "GOLES" };
        goalsHeader.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        goalsHeader.AddThemeFontSizeOverride("font_size", 14);
        layout.AddChild(goalsHeader);

        var goalEvents = result.Events.Where(e => e.IsGoal).OrderBy(e => e.Minute).ToArray();
        if (goalEvents.Length == 0)
        {
            var noneLabel = new Label { Text = "Sin goles — portería a cero" };
            noneLabel.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            layout.AddChild(noneLabel);
        }
        else
        {
            foreach (var evt in goalEvents)
            {
                var goalRow = new Label
                {
                    Text = $"  Min {evt.Minute,3}' · {evt.ActorShortName}",
                };
                goalRow.AddThemeColorOverride("font_color", ManagerDesignTokens.Positive);
                goalRow.AddThemeFontSizeOverride("font_size", 14);
                layout.AddChild(goalRow);
            }
        }

        // Replay + back buttons.
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);

        var replayButton = new Button
        {
            Text = "  ↻ Repetir (misma seed)",
            Alignment = HorizontalAlignment.Left,
        };
        replayButton.Pressed += () => ReplayMatch(result, competitionDisplayName);
        actions.AddChild(replayButton);

        var backButton = new Button
        {
            Text = "  ← Volver a competiciones",
            Alignment = HorizontalAlignment.Left,
        };
        backButton.Pressed += () => RenderCurrent();
        actions.AddChild(backButton);

        layout.AddChild(actions);

        return layout;
    }

    private void ReplayMatch(MatchResult previous, string competitionDisplayName)
    {
        // Deterministic replay: same clubs, same seed => same result.
        var replay = MatchSimulator.Simulate(previous.HomeClub, previous.AwayClub, previous.Seed);

        var root = GetNodeOrNull<Control>(ContentRootPath);
        var status = GetNodeOrNull<Label>(StatusLabelPath);
        if (root is null)
        {
            return;
        }

        ClearChildren(root);
        root.AddChild(BuildMatchReport(previous.HomeClub, replay, competitionDisplayName));

        bool sameScore = replay.HomeGoals == previous.HomeGoals && replay.AwayGoals == previous.AwayGoals;
        status?.SetText(sameScore
            ? $"● Repetición · {replay.HomeGoals}-{replay.AwayGoals} (idéntica, seed {previous.Seed})"
            : $"● Repetición · {replay.HomeGoals}-{replay.AwayGoals} (difiere — seed no determinista)");
    }

    // ----- Club picker -----------------------------------------------------

    private void RenderClubPicker(Control root, IReadOnlyList<ClubRow> clubs)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var header = new Label
        {
            Text = $"SELECCIONA UN CLUB · {clubs.Count} DISPONIBLES",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddColumnHeader(headerRow, "CLUB", 260);
        AddColumnHeader(headerRow, "PAÍS", 100);
        AddColumnHeader(headerRow, "FUND.", 60);
        AddColumnHeader(headerRow, "PREST.", 70, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "AFIC.", 70, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "PLANT.", 70, HorizontalAlignment.Right);
        AddColumnHeader(headerRow, "ACCIÓN", 110, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        foreach (var club in clubs)
        {
            layout.AddChild(BuildClubPickerRow(club));
        }

        root.AddChild(layout);
    }

    private HBoxContainer BuildClubPickerRow(ClubRow club)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        AddColumnValue(row, club.DisplayName, 260, ManagerDesignTokens.TextPrimary);
        AddColumnValue(row, club.CountryShortName, 100, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, club.FoundedYear, 60, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, club.Prestige.ToString(Inv), 70, ValueColor(club.Prestige), HorizontalAlignment.Right);
        AddColumnValue(row, club.Fanbase.ToString(Inv), 70, ValueColor(club.Fanbase), HorizontalAlignment.Right);
        AddColumnValue(row, club.SquadSize.ToString(Inv), 70, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);

        var pickBtn = new Button { Text = "Elegir" };
        pickBtn.AddThemeFontSizeOverride("font_size", 12);
        string capturedId = club.ExternalId;
        pickBtn.Pressed += () => PickClub(capturedId);
        var cell = new HBoxContainer();
        cell.CustomMinimumSize = new Vector2(110, 0);
        cell.AddChild(pickBtn);
        cell.SizeFlagsHorizontal = 0;
        row.AddChild(cell);

        return row;
    }

    private void PickClub(string clubExternalId)
    {
        _selectedClubId = clubExternalId;
        _selectedSection = "resumen";
        RenderCurrent();
    }

    // ----- Stats -----------------------------------------------------------

    private static PanelContainer BuildStatsTable(MatchStatsRow stats)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.Surface,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space12,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space12,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);

        var title = new Label { Text = "ESTADÍSTICAS" };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        title.AddThemeFontSizeOverride("font_size", 14);
        col.AddChild(title);

        col.AddChild(BuildStatRow("Tiros", stats.HomeShots, stats.AwayShots));
        col.AddChild(BuildStatRow("Paradas", stats.HomeSaves, stats.AwaySaves));
        col.AddChild(BuildStatRow("Fuera", stats.HomeMisses, stats.AwayMisses));
        col.AddChild(BuildStatRow("Tarjetas", stats.HomeCards, stats.AwayCards));
        col.AddChild(BuildStatRow("Posesión", $"{stats.HomePossession}%", $"{stats.AwayPossession}%"));

        panel.AddChild(col);
        return panel;
    }

    private static HBoxContainer BuildStatRow(string label, int home, int away) =>
        BuildStatRow(label, home.ToString(Inv), away.ToString(Inv));

    private static HBoxContainer BuildStatRow(string label, string home, string away)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddColumnValue(row, label, 140, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, home, 80, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        AddColumnValue(row, "-", 20, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Center);
        AddColumnValue(row, away, 80, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Left);
        return row;
    }

    // ----- Historial ------------------------------------------------------

    private static void RenderHistory(Control root)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var entries = MatchHistory.Instance.Snapshot();
        var header = new Label
        {
            Text = $"HISTORIAL DE PARTIDOS · {entries.Count} REGISTROS",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        if (entries.Count == 0)
        {
            var empty = new Label { Text = "Sin partidos jugados todavía — ve a Competiciones." };
            empty.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            layout.AddChild(empty);
        }
        else
        {
            var headerRow = new HBoxContainer();
            headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
            AddColumnHeader(headerRow, "CUÁNDO", 130);
            AddColumnHeader(headerRow, "COMPETICIÓN", 200);
            AddColumnHeader(headerRow, "LOCAL", 200);
            AddColumnHeader(headerRow, "", 30);
            AddColumnHeader(headerRow, "VISITANTE", 200);
            AddColumnHeader(headerRow, "SEED", 110, HorizontalAlignment.Right);
            layout.AddChild(headerRow);
            layout.AddChild(new HSeparator());

            foreach (var entry in entries.Reverse())
            {
                layout.AddChild(BuildHistoryRow(entry));
            }
        }

        root.AddChild(layout);
    }

    private static HBoxContainer BuildHistoryRow(HistoryEntry entry)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        AddColumnValue(row, entry.PlayedAtUtc.ToLocalTime().ToString("HH:mm:ss", Inv), 130, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, entry.CompetitionDisplayName, 200, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, entry.HomeDisplayName, 200, ManagerDesignTokens.TextPrimary);
        AddColumnValue(row, $"{entry.HomeGoals}-{entry.AwayGoals}", 30, ManagerDesignTokens.Accent, HorizontalAlignment.Center);
        AddColumnValue(row, entry.AwayDisplayName, 200, ManagerDesignTokens.TextPrimary);
        AddColumnValue(row, entry.Seed.ToString(Inv), 110, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);

        return row;
    }

    // ----- helpers --------------------------------------------------------

    private static PanelContainer BuildPanel(string title, string[] lines, Color titleColor)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = ManagerDesignTokens.Surface,
            BorderColor = ManagerDesignTokens.Border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = ManagerDesignTokens.CornerRadius,
            CornerRadiusTopRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomRight = ManagerDesignTokens.CornerRadius,
            CornerRadiusBottomLeft = ManagerDesignTokens.CornerRadius,
            ContentMarginLeft = ManagerDesignTokens.Space16,
            ContentMarginTop = ManagerDesignTokens.Space12,
            ContentMarginRight = ManagerDesignTokens.Space16,
            ContentMarginBottom = ManagerDesignTokens.Space12,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var titleLabel = new Label { Text = title };
        titleLabel.AddThemeColorOverride("font_color", titleColor);
        titleLabel.AddThemeFontSizeOverride("font_size", 14);
        col.AddChild(titleLabel);
        foreach (string line in lines)
        {
            var item = new Label { Text = $"• {line}" };
            item.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
            item.AddThemeFontSizeOverride("font_size", 14);
            col.AddChild(item);
        }
        panel.AddChild(col);
        return panel;
    }

    private static void AddField(GridContainer grid, string label, string value)
    {
        var field = new VBoxContainer();
        field.AddThemeConstantOverride("separation", ManagerDesignTokens.Space2);
        var labelNode = new Label { Text = label };
        labelNode.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        labelNode.AddThemeFontSizeOverride("font_size", 11);
        var valueNode = new Label { Text = value };
        valueNode.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        valueNode.AddThemeFontSizeOverride("font_size", 16);
        field.AddChild(labelNode);
        field.AddChild(valueNode);
        grid.AddChild(field);
    }

    private static void AddColumnHeader(
        HBoxContainer row,
        string text,
        int width,
        HorizontalAlignment align = HorizontalAlignment.Left)
    {
        var node = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = align,
        };
        node.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        node.AddThemeFontSizeOverride("font_size", 11);
        row.AddChild(node);
    }

    private static void AddColumnValue(
        HBoxContainer row,
        string text,
        int width,
        Color color,
        HorizontalAlignment align = HorizontalAlignment.Left)
    {
        var node = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 0),
            HorizontalAlignment = align,
        };
        node.AddThemeColorOverride("font_color", color);
        node.AddThemeFontSizeOverride("font_size", 14);
        row.AddChild(node);
    }

    private static Label MakeText(string text, Color color)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private static string Initials(string displayName)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0][..System.Math.Min(2, parts[0].Length)].ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private static string Age(int birthYear)
    {
        int currentYear = 2026;
        int age = currentYear - birthYear;
        return $"{age}";
    }

    private static string ExtractCountry(string playerExternalId)
    {
        if (playerExternalId.Contains("aurora", StringComparison.OrdinalIgnoreCase)) return "NOR";
        if (playerExternalId.Contains("cinder", StringComparison.OrdinalIgnoreCase)) return "VEL";
        if (playerExternalId.Contains("forge", StringComparison.OrdinalIgnoreCase)) return "VEL";
        if (playerExternalId.Contains("maris", StringComparison.OrdinalIgnoreCase)) return "SOL";
        if (playerExternalId.Contains("northglass", StringComparison.OrdinalIgnoreCase)) return "NOR";
        return "—";
    }

    private static Color ValueColor(int value)
    {
        return value switch
        {
            >= 80 => ManagerDesignTokens.Positive,
            >= 65 => ManagerDesignTokens.Accent,
            >= 50 => ManagerDesignTokens.TextPrimary,
            _ => ManagerDesignTokens.TextSecondary,
        };
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }

    // ----- Mundial --------------------------------------------------------

    private static void RenderWorldOverview(Control root)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var competitions = WorldService.Instance?.ListAllCompetitions() ?? Array.Empty<CompetitionRow>();

        var header = new Label
        {
            Text = $"MUNDIAL DE COMPETICIONES · {competitions.Count} LIGAS Y COPAS",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        // Aggregated overview: count clubs that play in at least one competition.
        var participatingClubIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var competition in competitions)
        {
            foreach (var rival in competition.Rivals)
            {
                participatingClubIds.Add(rival);
            }
        }

        var summary = new Label
        {
            Text = $"{participatingClubIds.Count} clubes inscritos en al menos una competición",
        };
        summary.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        summary.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(summary);

        layout.AddChild(new HSeparator());

        // Table header.
        var headerRow = new HBoxContainer();
        headerRow.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddColumnHeader(headerRow, "COMPETICIÓN", 260);
        AddColumnHeader(headerRow, "FORMATO", 120);
        AddColumnHeader(headerRow, "FRECUENCIA", 120);
        AddColumnHeader(headerRow, "RIVALES", 80, HorizontalAlignment.Right);
        layout.AddChild(headerRow);
        layout.AddChild(new HSeparator());

        foreach (var competition in competitions)
        {
            layout.AddChild(BuildWorldCompetitionRow(competition));
        }

        root.AddChild(layout);
    }

    private static HBoxContainer BuildWorldCompetitionRow(CompetitionRow competition)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        AddColumnValue(row, competition.DisplayName, 260, ManagerDesignTokens.TextPrimary);
        AddColumnValue(row, competition.Format, 120, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, competition.Frequency, 120, ManagerDesignTokens.TextSecondary);
        AddColumnValue(row, competition.Rivals.Count.ToString(Inv), 80, ManagerDesignTokens.Accent, HorizontalAlignment.Right);

        return row;
    }

    // ----- Temporada completa --------------------------------------------

    private void PlaySeason(ClubRow club, CompetitionRow competition)
    {
        if (_service is null || competition.Rivals.Count == 0)
        {
            return;
        }

        var clubs = _service.ListClubs();
        var participants = new List<ClubRow> { club };
        foreach (var rivalName in competition.Rivals)
        {
            var match = clubs.FirstOrDefault(c =>
                string.Equals(c.ShortName, rivalName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.DisplayName, rivalName, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                participants.Add(match);
            }
        }

        if (participants.Count < 2)
        {
            return;
        }

        long seed = unchecked((long)(DateTime.UtcNow.Ticks & 0x7FFFFFFFFFFFFFFF));
        var season = SeasonSimulator.Simulate(
            competitionExternalId: competition.ExternalId,
            competitionDisplayName: competition.DisplayName,
            participants: participants,
            seed: seed);

        // Log every fixture into the match history so the user can browse
        // every goal from every played season via the Historial tab.
        foreach (var fixture in season.Fixtures)
        {
            MatchHistory.Instance.Append(new HistoryEntry(
                CompetitionDisplayName: competition.DisplayName,
                HomeDisplayName: fixture.HomeDisplayName,
                AwayDisplayName: fixture.AwayDisplayName,
                HomeClubId: fixture.HomeClubId,
                AwayClubId: fixture.AwayClubId,
                HomeGoals: fixture.HomeGoals,
                AwayGoals: fixture.AwayGoals,
                Seed: 0,
                PlayedAtUtc: DateTime.UtcNow));
        }

        var myStanding = season.Standings.FirstOrDefault(s => s.ClubExternalId == club.ExternalId);
        if (myStanding is not null)
        {
            SeasonHistory.Instance.Append(new SeasonHistoryEntry(
                CompetitionDisplayName: competition.DisplayName,
                ManagedClubId: club.ExternalId,
                ManagedClubDisplayName: club.DisplayName,
                ManagedPosition: PositionOf(season, club.ExternalId),
                ManagedPoints: myStanding.Points,
                ManagedGoalsFor: myStanding.GoalsFor,
                ManagedGoalsAgainst: myStanding.GoalsAgainst,
                TotalClubs: season.Standings.Length,
                TotalFixtures: season.Fixtures.Length,
                PlayedAtUtc: DateTime.UtcNow,
                FinalStandings: season.Standings));
        }

        var root = GetNodeOrNull<Control>(ContentRootPath);
        var status = GetNodeOrNull<Label>(StatusLabelPath);
        if (root is null)
        {
            return;
        }

        ClearChildren(root);
        root.AddChild(BuildSeasonReport(club, season));

        if (myStanding is not null)
        {
            status?.SetText($"● {competition.DisplayName} · puesto {PositionOf(season, club.ExternalId)} · {myStanding.Points} pts");
        }
    }

    private static VBoxContainer BuildSeasonReport(ClubRow club, SeasonResult season)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        var header = new Label
        {
            Text = $"{season.CompetitionDisplayName.ToUpperInvariant()} · CLASIFICACIÓN FINAL",
        };
        header.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        header.AddThemeFontSizeOverride("font_size", 18);
        layout.AddChild(header);

        var meta = new Label
        {
            Text = $"{season.Standings.Length} clubes · {season.Fixtures.Length} partidos · seed {season.Standings.Length}",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(meta);

        layout.AddChild(new HSeparator());

        // Standings table.
        var standingsHeader = new HBoxContainer();
        standingsHeader.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        AddColumnHeader(standingsHeader, "#", 30);
        AddColumnHeader(standingsHeader, "CLUB", 260);
        AddColumnHeader(standingsHeader, "PTS", 60, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "PJ", 50, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "G", 50, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "E", 50, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "P", 50, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "GF", 60, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "GC", 60, HorizontalAlignment.Right);
        AddColumnHeader(standingsHeader, "DG", 60, HorizontalAlignment.Right);
        layout.AddChild(standingsHeader);
        layout.AddChild(new HSeparator());

        for (int i = 0; i < season.Standings.Length; i++)
        {
            layout.AddChild(BuildStandingRow(i + 1, season.Standings[i], club.ExternalId));
        }

        // Fixture results.
        layout.AddChild(new HSeparator());
        var fixturesTitle = new Label { Text = "RESULTADOS" };
        fixturesTitle.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        fixturesTitle.AddThemeFontSizeOverride("font_size", 14);
        layout.AddChild(fixturesTitle);

        foreach (var fixture in season.Fixtures)
        {
            layout.AddChild(BuildFixtureRow(fixture, club.ExternalId));
        }

        return layout;
    }

    private static HBoxContainer BuildStandingRow(int position, StandingRow row, string managedClubId)
    {
        var line = new HBoxContainer();
        line.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        var isManaged = string.Equals(row.ClubExternalId, managedClubId, StringComparison.Ordinal);

        AddColumnValue(line, position.ToString(Inv), 30, isManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextSecondary);
        AddColumnValue(line, row.DisplayName, 260, isManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextPrimary);
        AddColumnValue(line, row.Points.ToString(Inv), 60, isManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        AddColumnValue(line, row.Played.ToString(Inv), 50, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.Wins.ToString(Inv), 50, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.Draws.ToString(Inv), 50, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.Losses.ToString(Inv), 50, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.GoalsFor.ToString(Inv), 60, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.GoalsAgainst.ToString(Inv), 60, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        AddColumnValue(line, row.GoalDifference.ToString("+#;-#;0", Inv), 60, DiffColor(row.GoalDifference), HorizontalAlignment.Right);

        return line;
    }

    private static HBoxContainer BuildFixtureRow(SeasonFixture fixture, string managedClubId)
    {
        var line = new HBoxContainer();
        line.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);

        bool homeManaged = string.Equals(fixture.HomeClubId, managedClubId, StringComparison.Ordinal);
        bool awayManaged = string.Equals(fixture.AwayClubId, managedClubId, StringComparison.Ordinal);
        var homeColor = homeManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextPrimary;
        var awayColor = awayManaged ? ManagerDesignTokens.Accent : ManagerDesignTokens.TextPrimary;

        AddColumnValue(line, fixture.HomeDisplayName, 260, homeColor);
        AddColumnValue(line, fixture.HomeGoals.ToString(Inv), 40, homeColor, HorizontalAlignment.Right);
        AddColumnValue(line, "-", 20, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Center);
        AddColumnValue(line, fixture.AwayGoals.ToString(Inv), 40, awayColor, HorizontalAlignment.Left);
        AddColumnValue(line, fixture.AwayDisplayName, 260, awayColor);

        return line;
    }

    private static int PositionOf(SeasonResult season, string clubExternalId)
    {
        for (int i = 0; i < season.Standings.Length; i++)
        {
            if (string.Equals(season.Standings[i].ClubExternalId, clubExternalId, StringComparison.Ordinal))
            {
                return i + 1;
            }
        }

        return 0;
    }

    private static Color DiffColor(int diff)
    {
        return diff switch
        {
            > 0 => ManagerDesignTokens.Positive,
            < 0 => ManagerDesignTokens.Negative,
            _ => ManagerDesignTokens.TextSecondary,
        };
    }
}