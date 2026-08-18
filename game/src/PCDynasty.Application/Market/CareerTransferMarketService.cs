using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Market.Negotiation;
using PCDynasty.Market.Valuation;

namespace PCDynasty.Application.Market;

public sealed class CareerTransferMarketService : ICareerTransferMarketService, ICareerSquadProjection
{
    private const string Currency = "EUR";
    private readonly IManagerWorld _world;
    private readonly ICareerTransferStore _transferStore;
    private readonly ICareerFinanceStore _financeStore;
    private readonly TimeProvider _timeProvider;

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

    public IReadOnlyList<TransferCandidate> ListCandidates(string managedClubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(managedClubExternalId);
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

    public IReadOnlyList<PlayerRow> ListManagedSquad(string managedClubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(managedClubExternalId);
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
        string managedClubExternalId,
        string playerExternalId,
        long offeredMinorUnits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(managedClubExternalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(playerExternalId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(offeredMinorUnits);

        TransferCandidate? candidate = ListCandidates(managedClubExternalId)
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
        return _transferStore.TrySettle(transfer, finance, updatedFinance)
            ? TransferOfferResult.Success(transfer, updatedFinance.TransferBudget)
            : TransferOfferResult.Rejected("stale-or-duplicate", finance.TransferBudget);
    }

    private static TransferCandidate ToCandidate(ClubRow club, PlayerRow player) =>
        new(
            player.ExternalId,
            player.DisplayName,
            club.ExternalId,
            club.DisplayName,
            player.BirthYear,
            player.Technique,
            player.Form,
            player.Physical,
            player.Mental,
            player.Overall,
            AskingPrice(player),
            Currency);

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

    private static long AskingPrice(PlayerRow player) => checked(
        (player.Overall * 45_000L) +
        ((player.Technique + player.Form + player.Physical + player.Mental) * 5_000L));

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
