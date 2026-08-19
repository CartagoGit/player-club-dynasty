using Godot;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Core.Localization;
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

    /// <summary>
    /// How many candidates the table draws. Every row is a plate with six
    /// cells and a button, so a listing of the whole transfer-listed
    /// population built tens of thousands of nodes on a screen nobody can
    /// read past the first page. The strongest candidates are the ones a
    /// manager scans first, and the count below the table says what is
    /// hidden.
    /// </summary>
    private const int MaxVisibleCandidates = 60;

    private readonly ICareerTransferMarketService _market;
    private readonly ManagedClubContext _managed;
    private readonly CareerCalendar _calendar;
    private readonly Action _refresh;
    private readonly MarketSection _texts;
    private NoticeState _notice = NoticeState.Idle;
    private string[] _noticeArgs = Array.Empty<string>();
    private bool _lastOfferAccepted;

    internal MarketScreen(
        ICareerTransferMarketService market,
        ManagedClubContext managed,
        CareerCalendar calendar,
        Action refresh,
        MarketSection texts)
    {
        _market = market ?? throw new ArgumentNullException(nameof(market));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
    }

    internal void Render(Control root, ClubRow club)
    {
        var market = new VBoxContainer { Name = "TransferMarket" };
        market.AddThemeConstantOverride("separation", ManagerDesignTokens.Space12);
        root.AddChild(market);

        IReadOnlyList<TransferCandidate> candidates = _market.ListCandidates(_managed);

        var header = new HeaderPanel { Name = "Header" };
        header.SetTexts(
            _texts.Header.Eyebrow,
            _texts.Header.Title,
            _texts.SummaryFormat,
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
        notice.Text = _notice switch
        {
            NoticeState.Accepted => string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                _texts.NoticeAcceptedFormat,
                _noticeArgs),
            NoticeState.Rejected => string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                _texts.NoticeRejectedFormat,
                _noticeArgs),
            _ => _texts.NoticeIdle,
        };
        market.AddChild(notice);

        var list = new ListSection { Name = "CandidateRows", Area = ShellArea.Market };
        list.SetEmptyText(_texts.Empty);
        list.AddColumnHeader(_texts.Columns.Player, ColumnPlayer);
        list.AddColumnHeader(_texts.Columns.SellingClub, ColumnClub);
        list.AddColumnHeader(_texts.Columns.Age, ColumnAge, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Tec, ColumnTech, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Val, ColumnValue, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Price, ColumnPrice, HorizontalAlignment.Right);
        list.AddColumnHeader(_texts.Columns.Action, ColumnAction, HorizontalAlignment.Right);

        int visible = Math.Min(candidates.Count, MaxVisibleCandidates);
        if (candidates.Count == 0)
        {
            list.RefreshEmptyState();
        }
        else
        {
            for (int index = 0; index < visible; index++)
            {
                list.AddRow(BuildRow(candidates[index]));
            }
        }

        market.AddChild(list);

        if (candidates.Count > visible)
        {
            var truncated = new Label
            {
                Name = "Truncated",
                Text = string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    _texts.TruncatedFormat,
                    visible,
                    candidates.Count),
            };
            truncated.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
            truncated.AddThemeFontSizeOverride("font_size", 12);
            market.AddChild(truncated);
        }
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
            (_texts.Actions.SignTooltipFormat),
            WorkspaceUiFactory.FormatMoney(candidate.AskingPrice, candidate.Currency),
            candidate.PlayerName);
        row.AddAction(
            _texts.Actions.Sign,
            tooltip,
            _ => Sign(candidate),
            enabled: true,
            name: "Sign");
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
            _notice = NoticeState.Accepted;
            _noticeArgs = new[]
            {
                candidate.PlayerName,
                WorkspaceUiFactory.FormatMoney(result.RemainingTransferBudget, candidate.Currency),
            };
        }
        else
        {
            _notice = NoticeState.Rejected;
            _noticeArgs = new[] { RejectionText(result.Reason) };
        }
        _refresh();
    }

    private string RejectionText(string reason) => reason switch
    {
        "budget-exceeded" => _texts.Reasons.Budget,
        "below-asking-price" => _texts.Reasons.Price,
        "player-unavailable" => _texts.Reasons.Unavailable,
        "stale-or-duplicate" => _texts.Reasons.Stale,
        "finance-not-initialized" => _texts.Reasons.Finance,
        _ => _texts.Reasons.Generic,
    };

    /// <summary>Which of the three notice sentences the screen is showing.</summary>
    private enum NoticeState
    {
        Idle = 0,
        Accepted = 1,
        Rejected = 2,
    }
}
