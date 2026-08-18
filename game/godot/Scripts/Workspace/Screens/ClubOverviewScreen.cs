using Godot;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class ClubOverviewScreen
{
    private readonly IManagerWorld _world;
    private readonly CareerSession _careerSession;
    private readonly Action _openClubPicker;

    internal ClubOverviewScreen(
        IManagerWorld world,
        CareerSession careerSession,
        Action openClubPicker)
    {
        _world = world;
        _careerSession = careerSession;
        _openClubPicker = openClubPicker;
    }

    internal void Render(Control root, ClubRow club)
    {
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);
        layout.AddChild(BuildHeader(club));

        var identity = new GridContainer { Columns = 4 };
        identity.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space16);
        identity.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space8);
        identity.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        WorkspaceUiFactory.AddField(identity, "NOMBRE CORTO", club.DisplayName);
        WorkspaceUiFactory.AddField(identity, "CIUDAD", club.City);
        WorkspaceUiFactory.AddField(identity, "PAÍS", club.CountryShortName);
        WorkspaceUiFactory.AddField(identity, "FUNDADO", club.FoundedYear);
        WorkspaceUiFactory.AddField(identity, "PRESTIGIO", club.Prestige.ToString(WorkspaceUiFactory.InvariantCulture));
        WorkspaceUiFactory.AddField(identity, "AFICIÓN", club.Fanbase.ToString(WorkspaceUiFactory.InvariantCulture));
        WorkspaceUiFactory.AddField(identity, "PLANTILLA", club.SquadSize.ToString(WorkspaceUiFactory.InvariantCulture));
        WorkspaceUiFactory.AddField(identity, "ETAPA", "Pretemporada");
        layout.AddChild(identity);

        layout.AddChild(WorkspaceUiFactory.CreatePanel(
            "ATENCIÓN",
            ["Sin alertas activas — temporada en pretemporada."],
            ManagerDesignTokens.TextSecondary));
        layout.AddChild(BuildCareerPanel(club));
        layout.AddChild(BuildUpcomingFixturesPanel(club));
        layout.AddChild(WorkspaceUiFactory.CreatePanel(
            "SIGUIENTES DECISIONES",
            ["Avanzar pretemporada", "Revisar plantilla", "Configurar alineación"],
            ManagerDesignTokens.Accent));

        root.AddChild(layout);
    }

    private HBoxContainer BuildHeader(ClubRow club)
    {
        var band = new HBoxContainer();
        band.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);

        var crest = new PanelContainer { CustomMinimumSize = new Vector2(72, 72) };
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
            Text = WorkspaceUiFactory.Initials(club.DisplayName),
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
        var subLabel = new Label { Text = $"{club.City} · {club.CountryShortName} · fundado {club.FoundedYear}" };
        subLabel.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        subLabel.AddThemeFontSizeOverride("font_size", 14);
        meta.AddChild(subLabel);
        band.AddChild(meta);

        band.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        var phase = new Label { Text = "PRETEMPORADA" };
        phase.AddThemeColorOverride("font_color", ManagerDesignTokens.Accent);
        phase.AddThemeFontSizeOverride("font_size", 16);
        band.AddChild(phase);

        var picker = new Button { Text = "Cambiar club" };
        picker.AddThemeFontSizeOverride("font_size", 12);
        picker.Pressed += _openClubPicker;
        band.AddChild(picker);
        return band;
    }

    private PanelContainer BuildCareerPanel(ClubRow club)
    {
        IReadOnlyList<SeasonHistoryEntry> seasons = _careerSession.SeasonsFor(club.ExternalId);
        var lines = new List<string>();
        if (seasons.Count == 0)
        {
            lines.Add("Sin temporadas jugadas todavía — Competiciones → Jugar temporada completa.");
        }
        else
        {
            SeasonHistoryEntry latest = seasons[^1];
            string championBadge = latest.ManagedPosition == 1 ? " ★ CAMPEÓN" : string.Empty;
            lines.Add($"Última: {latest.CompetitionDisplayName} · puesto {latest.ManagedPosition}/{latest.TotalClubs}{championBadge}");
            lines.Add($"Puntos: {latest.ManagedPoints} · {latest.ManagedGoalsFor}GF / {latest.ManagedGoalsAgainst}GC");
            int wins = seasons.Count(entry => entry.ManagedPosition == 1);
            int podiums = seasons.Count(entry => entry.ManagedPosition <= 3);
            lines.Add($"Histórico: {seasons.Count} temp · {wins} títulos · {podiums} podios · {seasons.Sum(entry => entry.ManagedPoints)} pts totales");
        }

        return WorkspaceUiFactory.CreatePanel("CARRERA", lines, ManagerDesignTokens.Accent);
    }

    private PanelContainer BuildUpcomingFixturesPanel(ClubRow club)
    {
        var played = new HashSet<string>(StringComparer.Ordinal);
        foreach (HistoryEntry entry in _careerSession.MatchSnapshot())
        {
            if (string.Equals(entry.HomeClubId, club.ExternalId, StringComparison.Ordinal)) played.Add(entry.AwayClubId);
            else if (string.Equals(entry.AwayClubId, club.ExternalId, StringComparison.Ordinal)) played.Add(entry.HomeClubId);
        }

        ClubRow[] upcoming = _world.ListClubs()
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
            lines.AddRange(upcoming.Take(6).Select(rival => $"· {rival.DisplayName} · {rival.CountryShortName}"));
            if (upcoming.Length > 6) lines.Add($"· y {upcoming.Length - 6} más...");
        }

        return WorkspaceUiFactory.CreatePanel("PRÓXIMOS RIVALES", lines, ManagerDesignTokens.Accent);
    }
}
