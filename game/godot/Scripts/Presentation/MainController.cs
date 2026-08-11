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
            default:
                RenderClubOverview(root, club);
                status?.SetText($"● {club.DisplayName}");
                break;
        }
    }

    // ----- Resumen --------------------------------------------------------

    private static void RenderClubOverview(Control root, ClubRow club)
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

        layout.AddChild(BuildPanel(
            "SIGUIENTES DECISIONES",
            ["Avanzar pretemporada", "Revisar plantilla", "Configurar alineación"],
            ManagerDesignTokens.Accent));

        root.AddChild(layout);
    }

    private static HBoxContainer BuildClubHeader(ClubRow club)
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

        return band;
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

        var root = GetNodeOrNull<Control>(ContentRootPath);
        var status = GetNodeOrNull<Label>(StatusLabelPath);
        if (root is null)
        {
            return;
        }

        ClearChildren(root);
        root.AddChild(BuildMatchReport(club, result));

        string winner = result.HomeGoals > result.AwayGoals
            ? club.ShortName
            : result.AwayGoals > result.HomeGoals
                ? rival.ShortName
                : "Empate";
        status?.SetText($"● {club.ShortName} {result.HomeGoals}-{result.AwayGoals} {rival.ShortName} · {winner}");
    }

    private VBoxContainer BuildMatchReport(ClubRow club, MatchResult result)
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

        var meta = new Label
        {
            Text = $"Jornada simulada · seed {result.Seed} · {result.Events.Count} intentos · {result.Events.Count(e => e.IsGoal)} goles",
        };
        meta.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        meta.AddThemeFontSizeOverride("font_size", 12);
        layout.AddChild(meta);

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

        // Back button.
        var backButton = new Button
        {
            Text = "  ← Volver a competiciones",
            Alignment = HorizontalAlignment.Left,
        };
        backButton.Pressed += () => RenderCurrent();
        layout.AddChild(backButton);

        return layout;
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
}