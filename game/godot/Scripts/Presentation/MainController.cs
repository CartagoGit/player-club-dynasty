using Godot;
using PCDynasty.Presentation.Clubs;
using PCDynasty.Presentation.Components;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Presentation;

/// <summary>
/// Owns the presentation root. Listens to the sidebar's
/// <see cref="SidebarNavigator.DestinationRequested"/> signal and rebuilds
/// the central panel whenever the user picks a different section.
/// </summary>
/// <remarks>
/// This controller is intentionally thin: it never reaches into the domain
/// directly. All domain access goes through <see cref="WorldService"/> via
/// the engine-neutral projections.
/// </remarks>
public partial class MainController : Control
{
    [Export] public NodePath ContentRootPath { get; set; } = new();
    [Export] public NodePath SidebarPath { get; set; } = new();
    [Export] public NodePath StatusLabelPath { get; set; } = new();
    [Export] public NodePath OverviewButtonPath { get; set; } = new();
    [Export] public NodePath SquadButtonPath { get; set; } = new();
    [Export] public NodePath CompetitionsButtonPath { get; set; } = new();

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
        // Use the existing hardcoded buttons in the .tscn so we don't
        // touch the scene file. The SidebarNavigator.Bind path is reserved
        // for a future refactor that ships a fully data-driven sidebar.
        var overview = GetNodeOrNull<Button>(OverviewButtonPath);
        var squad = GetNodeOrNull<Button>(SquadButtonPath);
        var competitions = GetNodeOrNull<Button>(CompetitionsButtonPath);

        if (overview is not null)
        {
            overview.Pressed += () => SelectSection("resumen");
        }
        if (squad is not null)
        {
            squad.Pressed += () => SelectSection("plantilla");
        }
        if (competitions is not null)
        {
            competitions.Pressed += () => SelectSection("competiciones");
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
            root.AddChild(new Label
            {
                Text = "No hay clubs cargados.",
                HorizontalAlignment = HorizontalAlignment.Center,
            });
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
                status?.SetText($"● {club.DisplayName} — plantilla");
                break;
            case "competiciones":
                RenderCompetitionsStub(root, club);
                status?.SetText($"● {club.DisplayName} — competiciones");
                break;
            case "resumen":
            default:
                RenderClubOverview(root, club);
                status?.SetText($"● {club.DisplayName}");
                break;
        }
    }

    private static void RenderClubOverview(Control root, ClubRow club)
    {
        var view = ClubOverviewProjector.FromClubRow(club);

        var wrapper = new VBoxContainer();
        var header = new Label { Text = view.DisplayName };
        header.AddThemeFontSizeOverride("font_size", 32);
        var context = new Label { Text = view.Context };
        context.AddThemeColorOverride("font_color", new Color(0.659f, 0.753f, 0.804f));
        wrapper.AddChild(header);
        wrapper.AddChild(context);

        var grid = new GridContainer { Columns = 3 };
        foreach (var metric in view.Metrics)
        {
            var card = new MetricCard();
            card.Bind(metric);
            grid.AddChild(card);
        }
        wrapper.AddChild(grid);
        root.AddChild(wrapper);
    }

    private void RenderSquad(Control root, ClubRow club)
    {
        var players = _service?.ListPlayers(club.ExternalId) ?? Array.Empty<PlayerRow>();
        var wrapper = new VBoxContainer();
        var header = new Label { Text = $"{club.DisplayName} · plantilla ({players.Count})" };
        header.AddThemeFontSizeOverride("font_size", 24);
        wrapper.AddChild(header);

        var list = new VBoxContainer();
        foreach (var player in players)
        {
            var row = new Label
            {
                Text = $"{player.DisplayName} · {player.BirthYear} · TEC {player.Technique} / FOR {player.Form} / FIS {player.Physical} / MEN {player.Mental} · VAL {player.Overall}",
            };
            list.AddChild(row);
        }
        var scroll = new ScrollContainer();
        scroll.AddChild(list);
        wrapper.AddChild(scroll);
        root.AddChild(wrapper);
    }

    private static void RenderCompetitionsStub(Control root, ClubRow club)
    {
        var wrapper = new VBoxContainer();
        var header = new Label { Text = $"{club.DisplayName} · competiciones" };
        header.AddThemeFontSizeOverride("font_size", 24);
        wrapper.AddChild(header);
        var note = new Label
        {
            Text = "Calendario, clasificación y resultados llegan en la fase 2 (game loop).",
        };
        note.AddThemeColorOverride("font_color", new Color(0.612f, 0.722f, 0.792f));
        wrapper.AddChild(note);
        root.AddChild(wrapper);
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