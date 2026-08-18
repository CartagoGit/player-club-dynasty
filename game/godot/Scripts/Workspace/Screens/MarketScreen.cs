using Godot;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class MarketScreen
{
    private readonly ICareerTransferMarketService _market;
    private readonly ManagedClubContext _managed;
    private readonly CareerCalendar _calendar;
    private readonly Action _refresh;
    private string _notice = "Selecciona un objetivo para presentar una oferta por su precio solicitado.";
    private bool _lastOfferAccepted;

    internal MarketScreen(
        ICareerTransferMarketService market,
        ManagedClubContext managed,
        CareerCalendar calendar,
        Action refresh)
    {
        _market = market ?? throw new ArgumentNullException(nameof(market));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
    }

    internal void Render(Control root, ClubRow club)
    {
        var market = new VBoxContainer { Name = "TransferMarket" };
        market.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        market.AddChild(Header(club));

        var notice = new Label { Name = "Notice", Text = _notice };
        notice.AddThemeColorOverride(
            "font_color",
            _lastOfferAccepted ? ManagerDesignTokens.Positive : ManagerDesignTokens.TextSecondary);
        notice.AddThemeFontSizeOverride("font_size", 13);
        market.AddChild(notice);
        market.AddChild(ColumnHeaders());
        market.AddChild(new HSeparator());

        var rows = new VBoxContainer { Name = "CandidateRows" };
        rows.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        IReadOnlyList<TransferCandidate> candidates = _market.ListCandidates(_managed);
        if (candidates.Count == 0)
        {
            rows.AddChild(WorkspaceUiFactory.CreateText(
                "No quedan jugadores disponibles en esta ventana.",
                ManagerDesignTokens.TextSecondary));
        }
        else
        {
            foreach (TransferCandidate candidate in candidates)
            {
                rows.AddChild(CandidateRow(candidate));
            }
        }

        market.AddChild(rows);
        root.AddChild(market);
    }

    private static VBoxContainer Header(ClubRow club)
    {
        var header = new VBoxContainer { Name = "Header" };
        header.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var title = new Label { Text = "MERCADO · SECRETARÍA TÉCNICA" };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        title.AddThemeFontSizeOverride("font_size", 22);
        header.AddChild(title);
        var subtitle = new Label
        {
            Name = "ClubScope",
            Text = $"{club.DisplayName} · operaciones inmediatas · importes en EUR",
        };
        subtitle.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        subtitle.AddThemeFontSizeOverride("font_size", 13);
        header.AddChild(subtitle);
        return header;
    }

    private static HBoxContainer ColumnHeaders()
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        WorkspaceUiFactory.AddColumnHeader(row, "JUGADOR", 210);
        WorkspaceUiFactory.AddColumnHeader(row, "CLUB", 190);
        WorkspaceUiFactory.AddColumnHeader(row, "EDAD", 48, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "TEC", 48, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "VAL", 48, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "PRECIO", 120, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnHeader(row, "ACCIÓN", 110, HorizontalAlignment.Right);
        return row;
    }

    private HBoxContainer CandidateRow(TransferCandidate candidate)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        WorkspaceUiFactory.AddColumnValue(row, candidate.PlayerName, 210, ManagerDesignTokens.TextPrimary);
        WorkspaceUiFactory.AddColumnValue(row, candidate.SellerClubName, 190, ManagerDesignTokens.TextSecondary);
        WorkspaceUiFactory.AddColumnValue(row, WorkspaceUiFactory.PlayerAge(candidate.BirthYear, _calendar), 48, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, candidate.Technique.ToString(WorkspaceUiFactory.InvariantCulture), 48, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, candidate.Overall.ToString(WorkspaceUiFactory.InvariantCulture), 48, WorkspaceUiFactory.ValueColor(candidate.Overall), HorizontalAlignment.Right);
        WorkspaceUiFactory.AddColumnValue(row, WorkspaceUiFactory.FormatMoney(candidate.AskingPrice, candidate.Currency), 120, ManagerDesignTokens.Accent, HorizontalAlignment.Right);
        var action = new Button
        {
            Name = "Sign",
            Text = "FICHAR",
            CustomMinimumSize = new Vector2(110, ManagerDesignTokens.MinimumPointerTarget),
            TooltipText = $"Presentar {WorkspaceUiFactory.FormatMoney(candidate.AskingPrice, candidate.Currency)} por {candidate.PlayerName}",
        };
        action.Pressed += () => Sign(candidate);
        row.AddChild(action);
        return row;
    }

    private void Sign(TransferCandidate candidate)
    {
        TransferOfferResult result = _market.SubmitOffer(
            _managed,
            candidate.PlayerExternalId,
            candidate.AskingPrice);
        _lastOfferAccepted = result.Accepted;
        _notice = result.Accepted
            ? $"FICHAJE CERRADO · {candidate.PlayerName} · presupuesto restante {WorkspaceUiFactory.FormatMoney(result.RemainingTransferBudget, candidate.Currency)}"
            : $"OFERTA RECHAZADA · {RejectionText(result.Reason)}";
        _refresh();
    }

    private static string RejectionText(string reason) => reason switch
    {
        "budget-exceeded" => "caja o presupuesto insuficiente",
        "below-asking-price" => "la oferta no alcanza el precio solicitado",
        "player-unavailable" => "el jugador ya no está disponible",
        "stale-or-duplicate" => "la operación cambió; revisa el mercado",
        "finance-not-initialized" => "las finanzas de la carrera no están disponibles",
        _ => reason,
    };

}
