using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Application.Tests.Market;

public sealed class CareerTransferMarketServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CandidatesAreDeterministicAndExcludeManagedOrAcquiredPlayers()
    {
        var transfers = new InMemoryTransferStore();
        transfers.Seed(Transfer("person:best"));
        var service = Service(transfers);
        var reversed = Service(transfers, world: new FakeManagerWorld(reverse: true));

        IReadOnlyList<TransferCandidate> first = service.ListCandidates("club:managed");
        IReadOnlyList<TransferCandidate> second = reversed.ListCandidates("club:managed");

        Assert.Equal(first, second);
        Assert.Collection(
            first,
            candidate => Assert.Equal("person:available", candidate.PlayerExternalId),
            candidate => Assert.Equal("person:available-z", candidate.PlayerExternalId));
        Assert.All(first, candidate =>
        {
            Assert.Equal("Selling Town", candidate.SellerClubName);
            Assert.True(candidate.AskingPrice > 0);
        });
    }

    [Fact]
    public void BelowAskingOfferIsRejectedWithoutMutation()
    {
        var transfers = new InMemoryTransferStore();
        var finances = new InMemoryFinanceStore(Finance());
        var service = Service(transfers, finances);
        TransferCandidate candidate = service.ListCandidates("club:managed")[0];

        TransferOfferResult result = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice - 1);

        Assert.False(result.Accepted);
        Assert.Equal("below-asking-price", result.Reason);
        Assert.Empty(transfers.ListForClub("club:managed"));
        Assert.Equal(10_000_000, finances.State.Cash);
    }

    [Fact]
    public void AffordableOfferSettlesFinanceAndAddsPlayerToManagedSquad()
    {
        var transfers = new InMemoryTransferStore();
        var finances = new InMemoryFinanceStore(Finance());
        var service = Service(transfers, finances);
        TransferCandidate candidate = service.ListCandidates("club:managed")[0];

        TransferOfferResult result = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice);

        Assert.True(result.Accepted);
        Assert.Equal(candidate.AskingPrice, Assert.Single(transfers.ListForClub("club:managed")).Amount);
        Assert.Equal(10_000_000 - candidate.AskingPrice, finances.State.Cash);
        Assert.Equal(8_000_000 - candidate.AskingPrice, finances.State.TransferBudget);
        Assert.Contains(
            service.ListManagedSquad("club:managed"),
            player => player.ExternalId == candidate.PlayerExternalId);
        Assert.DoesNotContain(
            service.ListCandidates("club:managed"),
            item => item.PlayerExternalId == candidate.PlayerExternalId);
    }

    [Fact]
    public void CashAlsoLimitsAffordability()
    {
        var transfers = new InMemoryTransferStore();
        var finances = new InMemoryFinanceStore(Finance(cash: 100));
        var service = Service(transfers, finances);
        TransferCandidate candidate = service.ListCandidates("club:managed")[0];

        TransferOfferResult result = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice);

        Assert.False(result.Accepted);
        Assert.Equal("budget-exceeded", result.Reason);
        Assert.Equal(100, finances.State.Cash);
        Assert.Empty(transfers.ListForClub("club:managed"));
    }

    [Fact]
    public void DuplicateOfferIsRejectedWithoutAdditionalDebit()
    {
        var transfers = new InMemoryTransferStore();
        var finances = new InMemoryFinanceStore(Finance());
        var service = Service(transfers, finances);
        TransferCandidate candidate = service.ListCandidates("club:managed")[0];
        TransferOfferResult first = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice);
        long cashAfterFirst = finances.State.Cash;

        TransferOfferResult duplicate = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice);

        Assert.True(first.Accepted);
        Assert.False(duplicate.Accepted);
        Assert.Equal("player-unavailable", duplicate.Reason);
        Assert.Equal(cashAfterFirst, finances.State.Cash);
        Assert.Single(transfers.ListForClub("club:managed"));
    }

    [Fact]
    public void StaleFinanceSnapshotRejectsSettlementWithoutTransferDebit()
    {
        var transfers = new InMemoryTransferStore();
        var finances = new InMemoryFinanceStore(Finance());
        var service = Service(transfers, finances);
        TransferCandidate candidate = service.ListCandidates("club:managed")[0];
        transfers.BeforeSettlement = () => finances.Save(Finance(cash: 9_999_999, updatedAt: Now.AddSeconds(1)));

        TransferOfferResult result = service.SubmitOffer(
            "club:managed",
            candidate.PlayerExternalId,
            candidate.AskingPrice);

        Assert.False(result.Accepted);
        Assert.Equal("stale-or-duplicate", result.Reason);
        Assert.Equal(9_999_999, finances.State.Cash);
        Assert.Empty(transfers.ListForClub("club:managed"));
    }

    [Fact]
    public void PublicReadModelsRejectImpossibleStates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Candidate(askingPrice: -1));
        Assert.Throws<ArgumentException>(() => Candidate(currency: "1$!"));
        Assert.Throws<ArgumentOutOfRangeException>(() => TransferOfferResult.Rejected("rejected", -1));
        Assert.Throws<ArgumentNullException>(() => TransferOfferResult.Success(null!, 0));
    }

    private static CareerTransferMarketService Service(
        InMemoryTransferStore? transfers = null,
        InMemoryFinanceStore? finances = null,
        IManagerWorld? world = null)
    {
        InMemoryTransferStore transferStore = transfers ?? new InMemoryTransferStore();
        InMemoryFinanceStore financeStore = finances ?? new InMemoryFinanceStore(Finance());
        transferStore.OnSettled = financeStore.Save;
        transferStore.CurrentFinance = () => financeStore.State;
        return new(
            world ?? new FakeManagerWorld(),
            transferStore,
            financeStore,
            new FixedTimeProvider(Now));
    }

    private static CareerFinanceState Finance(
        long cash = 10_000_000,
        long transferBudget = 8_000_000,
        DateTimeOffset? updatedAt = null) =>
        new("club:managed", 2026, "EUR", cash, 20_000_000, 10_000_000, 0, transferBudget, 4_000_000, updatedAt ?? Now);

    private static TransferCandidate Candidate(long askingPrice = 1, string currency = "EUR") =>
        new("person:1", "Player", "club:seller", "Seller", 2000, 50, 50, 50, 50, 50, askingPrice, currency);

    private static CareerTransferRecord Transfer(string playerId) =>
        new("transfer:existing", "club:managed", "club:seller", playerId, 1_000_000, "EUR", Now);

    private sealed class InMemoryFinanceStore(CareerFinanceState state) : ICareerFinanceStore
    {
        public CareerFinanceState State { get; private set; } = state;

        public void Save(CareerFinanceState value) => State = value;

        public bool TryLoad(string clubExternalId, out CareerFinanceState? value)
        {
            value = State;
            return string.Equals(clubExternalId, State.ClubExternalId, StringComparison.Ordinal);
        }

        public void Clear() => throw new NotSupportedException();
    }

    private sealed class InMemoryTransferStore : ICareerTransferStore
    {
        private readonly List<CareerTransferRecord> _records = [];

        internal Action<CareerFinanceState>? OnSettled { get; set; }

        internal Func<CareerFinanceState>? CurrentFinance { get; set; }

        internal Action? BeforeSettlement { get; set; }

        public IReadOnlyList<CareerTransferRecord> ListForClub(string clubExternalId) =>
            _records.Where(item => item.BuyerClubExternalId == clubExternalId).ToArray();

        public bool TrySettle(
            CareerTransferRecord transfer,
            CareerFinanceState expectedFinance,
            CareerFinanceState updatedFinance)
        {
            BeforeSettlement?.Invoke();
            if (_records.Any(item => item.PlayerExternalId == transfer.PlayerExternalId) ||
                CurrentFinance?.Invoke() != expectedFinance)
            {
                return false;
            }

            _records.Add(transfer);
            OnSettled?.Invoke(updatedFinance);
            return true;
        }

        public void Clear() => _records.Clear();

        internal void Seed(CareerTransferRecord transfer) => _records.Add(transfer);
    }

    private sealed class FakeManagerWorld(bool reverse = false) : IManagerWorld
    {
        private static readonly ClubRow Managed = Club("club:managed", "Managed Athletic");
        private static readonly ClubRow Seller = Club("club:seller", "Selling Town");
        private static readonly PlayerRow ManagedPlayer = Player("person:managed", "Home Player", 60);
        private static readonly PlayerRow BestPlayer = Player("person:best", "Best Player", 80);
        private static readonly PlayerRow AvailablePlayer = Player("person:available", "Available Player", 70);
        private static readonly PlayerRow AvailableTwin = Player("person:available-z", "Available Player", 70);

        public IReadOnlyList<ClubRow> ListClubs() => reverse ? [Seller, Managed] : [Managed, Seller];

        public IReadOnlyList<PlayerRow> ListPlayers(string clubExternalId) =>
            clubExternalId == Managed.ExternalId
                ? [ManagedPlayer]
                : reverse
                    ? [AvailableTwin, AvailablePlayer, BestPlayer]
                    : [BestPlayer, AvailablePlayer, AvailableTwin];

        public IReadOnlyList<CompetitionRow> ListCompetitionsForClub(string clubExternalId) => [];

        public IReadOnlyList<CompetitionRow> ListAllCompetitions() => [];

        public ClubRow? FindClub(string clubExternalId) =>
            ListClubs().FirstOrDefault(club => club.ExternalId == clubExternalId);

        private static ClubRow Club(string id, string name) =>
            new(id, name, name, "City", "Country", "1900", 50, 50, 1);

        private static PlayerRow Player(string id, string name, int overall) =>
            new(id, name, 2000, overall, overall, overall, overall, overall);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
