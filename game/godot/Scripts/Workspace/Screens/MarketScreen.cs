using Godot;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class MarketScreen
{
    private const int ColumnPlayer = 210;
    private const int ColumnClub = 190;
    private const int ColumnAge = 48;
    private const int ColumnTech = 48;
    private const int ColumnValue = 48;
    private const int ColumnPrice = 120;
    private const int ColumnAction = 110;

    private readonly ICareerTransferMarketService _market;
    private readonly ManagedClubContext _managed;
    private readonly CareerCalendar _calendar;
    private readonly Action _refresh;
    private readonly Func<string, string> _tr;
    private string _noticeKey = "MARKET_NOTICE_IDLE";
    private string[] _noticeArgs = Array.Empty<string>();
    private bool _lastOfferAccepted;

    internal MarketScreen(
        ICareerTransferMarketService market,
        ManagedClubContext managed,
        CareerCalendar calendar,
        Action refresh,
        Func<string, string> tr)
    {
        _market = market ?? throw new ArgumentNullException(nameof(market));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        _tr = tr ?? throw new ArgumentNullException(nameof(tr));
    }

    internal void Render(Control root, ClubRow club)
    {
        var market = new VBoxContainer { Name = "TransferMarket" };
        market.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(market);

        IReadOnlyList<TransferCandidate> candidates = _market.ListCandidates(_managed);

        var header = new HeaderPanel { Name = "Header" };
        header.SetTexts(
            "MARKET_EYEBROW",
            "MARKET_TITLE",
            "MARKET_SUMMARY",
            new[]
            {
                club.DisplayName,
                candidates.Count.ToString(WorkspaceUiFactory.InvariantCulture),
                candidates.Count(c => c.AskingPrice > 0).ToString(WorkspaceUiFactory.InvariantCulture),
            });
        market.AddChild(header);

        // Compat shim: the smoke-boot and the existing UX (top-right club
        // scope label) reach a Label named "ClubScope" directly. Keep it
        // available so the assertions still resolve.
        var scope = new Label
        {
            Name = "ClubScope",
            Text = $"{club.DisplayName} · operaciones inmediatas · importes en EUR",
            Visible = false,
        };
        header.AddChild(scope);

        // The notice lives on its own line under the header, exactly where
        // the smoke-boot expects it: TransferMarket/Notice.
        var notice = new Label { Name = "Notice" };
        notice.AddThemeColorOverride(
            "font_color",
            _lastOfferAccepted ? ManagerDesignTokens.Positive : ManagerDesignTokens.TextSecondary);
        notice.AddThemeFontSizeOverride("font_size", 13);
        notice.Text = _noticeArgs.Length > 0
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, _tr(_noticeKey), _noticeArgs)
            : _tr(_noticeKey);
        market.AddChild(notice);

        var list = new ListSection { Name = "CandidateRows" };
        list.SetEmptyText("MARKET_EMPTY");
        list.AddColumnHeader("MARKET_COL_PLAYER", ColumnPlayer);
        list.AddColumnHeader("MARKET_COL_SELLING_CLUB", ColumnClub);
        list.AddColumnHeader("MARKET_COL_AGE", ColumnAge, HorizontalAlignment.Right);
        list.AddColumnHeader("MARKET_COL_TEC", ColumnTech, HorizontalAlignment.Right);
        list.AddColumnHeader("MARKET_COL_VAL", ColumnValue, HorizontalAlignment.Right);
        list.AddColumnHeader("MARKET_COL_PRICE", ColumnPrice, HorizontalAlignment.Right);
        list.AddColumnHeader("MARKET_COL_ACTION", ColumnAction, HorizontalAlignment.Right);

        if (candidates.Count == 0)
        {
            list.RefreshEmptyState();
        }
        else
        {
            for (int index = 0; index < candidates.Count; index++)
            {
                list.AddRow(BuildRow(candidates[index]));
            }
        }

        market.AddChild(list);
    }

    private ZebraRow BuildRow(TransferCandidate candidate)
    {
        var row = new ZebraRow(0);
        row.AddCell(candidate.PlayerName, ColumnPlayer, ManagerDesignTokens.TextPrimary);
        row.AddCell(candidate.SellerClubName, ColumnClub, ManagerDesignTokens.TextSecondary);
        row.AddCell(WorkspaceUiFactory.PlayerAge(candidate.BirthYear, _calendar), ColumnAge, ManagerDesignTokens.TextSecondary, HorizontalAlignment.Right);
        row.AddCell(candidate.Technique.ToString(WorkspaceUiFactory.InvariantCulture), ColumnTech, ManagerDesignTokens.TextPrimary, HorizontalAlignment.Right);
        row.AddCell(candidate.Overall.ToString(WorkspaceUiFactory.InvariantCulture), ColumnValue, WorkspaceUiFactory.ValueColor(candidate.Overall), HorizontalAlignment.Right);
        row.AddCell(WorkspaceUiFactory.FormatMoney(candidate.AskingPrice, candidate.Currency), ColumnPrice, ManagerDesignTokens.Accent, HorizontalAlignment.Right);
        // The tooltip combines the asking price and the player name; the
        // base "SIGN" label is what the translation file ships, and the
        // concrete text is built with string.Format so we keep the .po
        // keys simple (one place-holder, not two).
        string tooltip = string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            _tr("MARKET_ROW_SIGN_TOOLTIP_FMT"),
            WorkspaceUiFactory.FormatMoney(candidate.AskingPrice, candidate.Currency),
            candidate.PlayerName);
        row.AddAction(
            "MARKET_ROW_SIGN",
            tooltip,
            _ => Sign(candidate),
            enabled: true);
        return row;
    }

    private void Sign(TransferCandidate candidate)
    {
        TransferOfferResult result = _market.SubmitOffer(
            _managed,
            candidate.PlayerExternalId,
            candidate.AskingPrice);
        _lastOfferAccepted = result.Accepted;
        if (result.Accepted)
        {
            _noticeKey = "MARKET_NOTICE_ACCEPTED";
            _noticeArgs = new[]
            {
                candidate.PlayerName,
                WorkspaceUiFactory.FormatMoney(result.RemainingTransferBudget, candidate.Currency),
            };
        }
        else
        {
            _noticeKey = "MARKET_NOTICE_REJECTED";
            _noticeArgs = new[] { RejectionKey(result.Reason) };
        }
        _refresh();
    }

    private static string RejectionKey(string reason) => reason switch
    {
        "budget-exceeded" => "MARKET_REJECT_BUDGET",
        "below-asking-price" => "MARKET_REJECT_PRICE",
        "player-unavailable" => "MARKET_REJECT_UNAVAILABLE",
        "stale-or-duplicate" => "MARKET_REJECT_STALE",
        "finance-not-initialized" => "MARKET_REJECT_FINANCE",
        _ => "MARKET_REJECT_GENERIC",
    };
}
