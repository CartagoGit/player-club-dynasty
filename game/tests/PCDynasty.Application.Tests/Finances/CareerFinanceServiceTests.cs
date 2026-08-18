using PCDynasty.Application.Finances;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Application.Tests.Finances;

public sealed class CareerFinanceServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly ClubRow Club =
        new("club:aurora", "Aurora Athletic", "Aurora", "Myrglass", "NOR", "1938", 54, 48, 22);

    [Fact]
    public void NewCareerCreatesDeterministicLedgerAndPersistsOverview()
    {
        var store = new MemoryFinanceStore();
        var service = new CareerFinanceService(store, new FixedTimeProvider(FixedNow));

        ClubFinanceOverview overview = service.Open(Club, CareerFinanceMode.New);

        Assert.Equal(20_700_000, overview.Revenue);
        Assert.Equal(11_385_000, overview.Costs);
        Assert.Equal(9_315_000, overview.Cash);
        Assert.Equal(9_315_000, overview.OperatingResult);
        Assert.Equal(5_589_000, overview.TransferBudget);
        Assert.Equal(5_123_250, overview.WageBudget);
        Assert.Equal(2026, overview.SeasonYear);
        Assert.Same(store.Saved, store.Require(Club.ExternalId));
        Assert.Equal(1, store.ClearCount);
    }

    [Fact]
    public void ContinueRestoresExistingStateWithoutReplacingIt()
    {
        var stored = new CareerFinanceState(
            Club.ExternalId,
            2027,
            "EUR",
            123,
            500,
            200,
            50,
            75,
            25,
            FixedNow.AddDays(-1));
        var store = new MemoryFinanceStore(stored);
        var service = new CareerFinanceService(store, new FixedTimeProvider(FixedNow));

        ClubFinanceOverview overview = service.Open(Club, CareerFinanceMode.Continue);

        Assert.Equal(123, overview.Cash);
        Assert.Equal(300, overview.OperatingResult);
        Assert.Equal(0, store.ClearCount);
        Assert.Equal(0, store.SaveCount);
    }

    [Fact]
    public void ContinueCreatesStateWhenExternalDatabaseHasNone()
    {
        var store = new MemoryFinanceStore();
        var service = new CareerFinanceService(store, new FixedTimeProvider(FixedNow));

        ClubFinanceOverview overview = service.Open(Club, CareerFinanceMode.Continue);

        Assert.Equal(Club.ExternalId, overview.ClubExternalId);
        Assert.Equal(1, store.SaveCount);
        Assert.Equal(0, store.ClearCount);
        Assert.Equal(overview, service.GetOverview(Club.ExternalId));
    }

    private sealed class MemoryFinanceStore : ICareerFinanceStore
    {
        internal MemoryFinanceStore(CareerFinanceState? saved = null) => Saved = saved;
        internal CareerFinanceState? Saved { get; private set; }
        internal int SaveCount { get; private set; }
        internal int ClearCount { get; private set; }

        public void Save(CareerFinanceState state)
        {
            Saved = state;
            SaveCount++;
        }

        public bool TryLoad(string clubExternalId, out CareerFinanceState? state)
        {
            state = Saved?.ClubExternalId == clubExternalId ? Saved : null;
            return state is not null;
        }

        public void Clear()
        {
            Saved = null;
            ClearCount++;
        }

        internal CareerFinanceState Require(string clubExternalId) =>
            TryLoad(clubExternalId, out CareerFinanceState? state)
                ? state!
                : throw new InvalidOperationException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
