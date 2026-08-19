using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Market.Negotiation;
using PCDynasty.Market.Valuation;

namespace PCDynasty.Application.Market;

public sealed class CareerTransferMarketService : ICareerTransferMarketService, ICareerSquadProjection
{
    private const string Currency = "EUR";
    private const long MinimumAskingPrice = 250_000L;
    private readonly IManagerWorld _world;
    private readonly ICareerTransferStore _transferStore;
    private readonly ICareerFinanceStore _financeStore;
    private readonly TimeProvider _timeProvider;

    // The candidate universe is every player of every club the career does
    // not own, valued and sorted. On the target-scale world that is 160,000
    // people, and the screen asked for it on every render — including the
    // re-render that follows an offer. It only changes when a transfer
    // settles, so it is computed once per settle and kept.
    private ImmutableArray<TransferCandidate> _candidates;
    private ImmutableArray<PlayerRow> _managedSquad;
    private string? _cachedForClub;
    private bool _cacheIsWarm;

    public CareerTransferMarketService(
        IManagerWorld world,
        ICareerTransferStore transferStore,
        ICareerFinanceStore financeStore,
        TimeProvider timeProvider)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _transferStore = transferStore ?? throw new ArgumentNullException(nameof(transferStore));
        _financeStore = financeStore ?? throw new ArgumentNullException(nameof(financeStore));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IReadOnlyList<TransferCandidate> ListCandidates(ManagedClubContext managed)
    {
        ArgumentNullException.ThrowIfNull(managed);
        EnsureCache(managed.ClubExternalId);
        return _candidates;
    }

    public IReadOnlyList<PlayerRow> ListManagedSquad(ManagedClubContext managed)
    {
        ArgumentNullException.ThrowIfNull(managed);
        EnsureCache(managed.ClubExternalId);
        return _managedSquad;
    }

    /// <summary>
    /// Rebuild the two projections unless they already answer for this club
    /// and no transfer has settled since. Invalidation has exactly one
    /// trigger — <see cref="Invalidate"/> after a settle — so there is no
    /// second rule to keep in step.
    /// </summary>
    private void EnsureCache(string managedClubExternalId)
    {
        if (_cacheIsWarm && string.Equals(_cachedForClub, managedClubExternalId, StringComparison.Ordinal))
        {
            return;
        }

        _candidates = BuildCandidates(managedClubExternalId);
        _managedSquad = BuildManagedSquad(managedClubExternalId);
        _cachedForClub = managedClubExternalId;
        _cacheIsWarm = true;
    }

    /// <summary>Drop the projections after a settle changes who is available.</summary>
    private void Invalidate()
    {
        _cacheIsWarm = false;
    }

    private ImmutableArray<TransferCandidate> BuildCandidates(string managedClubExternalId)
    {
        HashSet<string> unavailablePlayers = _world
            .ListPlayers(managedClubExternalId)
            .Select(player => player.ExternalId)
            .Concat(_transferStore.ListForClub(managedClubExternalId).Select(transfer => transfer.PlayerExternalId))
            .ToHashSet(StringComparer.Ordinal);

        return _world
            .ListClubs()
            .Where(club => !string.Equals(club.ExternalId, managedClubExternalId, StringComparison.Ordinal))
            .SelectMany(club => _world
                .ListPlayers(club.ExternalId)
                .Where(player => !unavailablePlayers.Contains(player.ExternalId))
                .Select(player => ToCandidate(club, player)))
            .OrderByDescending(candidate => candidate.Overall)
            .ThenBy(candidate => candidate.PlayerName, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.PlayerExternalId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.SellerClubExternalId, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private ImmutableArray<PlayerRow> BuildManagedSquad(string managedClubExternalId)
    {
        IEnumerable<PlayerRow> originalSquad = _world.ListPlayers(managedClubExternalId);
        HashSet<string> acquiredIds = _transferStore
            .ListForClub(managedClubExternalId)
            .Select(transfer => transfer.PlayerExternalId)
            .ToHashSet(StringComparer.Ordinal);
        IEnumerable<PlayerRow> acquisitions = _world
            .ListClubs()
            .SelectMany(club => _world.ListPlayers(club.ExternalId))
            .Where(player => acquiredIds.Contains(player.ExternalId));

        return originalSquad
            .Concat(acquisitions)
            .DistinctBy(player => player.ExternalId, StringComparer.Ordinal)
            .OrderByDescending(player => player.Overall)
            .ThenBy(player => player.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    public TransferOfferResult SubmitOffer(
        ManagedClubContext managed,
        string playerExternalId,
        long offeredMinorUnits)
    {
        ArgumentNullException.ThrowIfNull(managed);
        string managedClubExternalId = managed.ClubExternalId;
        ArgumentException.ThrowIfNullOrWhiteSpace(playerExternalId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(offeredMinorUnits);

        TransferCandidate? candidate = ListCandidates(managed)
            .FirstOrDefault(item => string.Equals(
                item.PlayerExternalId,
                playerExternalId,
                StringComparison.Ordinal));
        if (candidate is null)
        {
            return TransferOfferResult.Rejected("player-unavailable");
        }

        if (!_financeStore.TryLoad(managedClubExternalId, out CareerFinanceState? finance))
        {
            return TransferOfferResult.Rejected("finance-not-initialized");
        }

        PlayerValuation valuation = CreateValuation(candidate, offeredMinorUnits);
        var bid = new MarketBid(
            TransferId(managedClubExternalId, playerExternalId),
            managedClubExternalId,
            valuation,
            new TransferPrice(new MoneyAmount(offeredMinorUnits, Currency)),
            needPriority: 500);
        NegotiationResolution resolution = NegotiationEngine.Evaluate(
            bid,
            new MoneyAmount(Math.Min(finance!.TransferBudget, Math.Max(0L, finance.Cash)), finance.Currency));
        if (!resolution.Accepted)
        {
            return TransferOfferResult.Rejected(resolution.Reason, finance.TransferBudget);
        }

        DateTimeOffset completedAt = NextTimestamp(finance.UpdatedAtUtc);
        var transfer = new CareerTransferRecord(
            bid.BidId,
            managedClubExternalId,
            candidate.SellerClubExternalId,
            playerExternalId,
            offeredMinorUnits,
            finance.Currency,
            completedAt);
        CareerFinanceState updatedFinance = Debit(finance, offeredMinorUnits, completedAt);
        if (!_transferStore.TrySettle(transfer, finance, updatedFinance))
        {
            return TransferOfferResult.Rejected("stale-or-duplicate", finance.TransferBudget);
        }

        // The signed player leaves the market and joins the squad, so both
        // projections are stale from here.
        Invalidate();
        return TransferOfferResult.Success(transfer, updatedFinance.TransferBudget);
    }

    private static TransferCandidate ToCandidate(ClubRow club, PlayerRow player)
    {
        int technique = NormalizeRating(player.Technique);
        int form = NormalizeRating(player.Form);
        int physical = NormalizeRating(player.Physical);
        int mental = NormalizeRating(player.Mental);
        int overall = NormalizeRating(player.Overall);
        return new(
            player.ExternalId,
            player.DisplayName,
            club.ExternalId,
            club.DisplayName,
            player.BirthYear,
            technique,
            form,
            physical,
            mental,
            overall,
            AskingPrice(technique, form, physical, mental, overall),
            Currency);
    }

    private PlayerValuation CreateValuation(TransferCandidate candidate, long offeredMinorUnits)
    {
        long asking = candidate.AskingPrice;
        int day = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime).DayNumber;
        return new PlayerValuation(
            candidate.PlayerExternalId,
            day,
            new MarketPrice(Eur(asking * 95L / 100L)),
            new InternalPrice(Eur(asking * 90L / 100L)),
            new AskingPrice(Eur(asking)),
            new TransferPrice(Eur(offeredMinorUnits)));
    }

    private static long AskingPrice(
        int technique,
        int form,
        int physical,
        int mental,
        int overall) => Math.Max(
        MinimumAskingPrice,
        checked(
            ((long)overall * 45_000L) +
            (((long)technique + form + physical + mental) * 5_000L)));

    private static int NormalizeRating(int rating) => Math.Clamp(rating, 0, 100);

    private static MoneyAmount Eur(long amount) => new(amount, Currency);

    private static string TransferId(string clubExternalId, string playerExternalId) =>
        $"transfer:{clubExternalId}:{playerExternalId}";

    private DateTimeOffset NextTimestamp(DateTimeOffset previous)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow().ToUniversalTime();
        return now > previous ? now : previous.AddTicks(1L);
    }

    private static CareerFinanceState Debit(
        CareerFinanceState finance,
        long amount,
        DateTimeOffset completedAt) =>
        new(
            finance.ClubExternalId,
            finance.SeasonYear,
            finance.Currency,
            checked(finance.Cash - amount),
            finance.Revenue,
            finance.Costs,
            finance.Debt,
            checked(finance.TransferBudget - amount),
            finance.WageBudget,
            completedAt);
}
