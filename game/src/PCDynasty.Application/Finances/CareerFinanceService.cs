using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;
using PCDynasty.Economy.Ledgers;

namespace PCDynasty.Application.Finances;

public sealed class CareerFinanceService : ICareerFinanceService
{
    private const string Currency = "EUR";
    private readonly ICareerFinanceStore _store;
    private readonly TimeProvider _timeProvider;

    public CareerFinanceService(ICareerFinanceStore store, TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ClubFinanceOverview Open(ClubRow club, CareerFinanceMode mode)
    {
        ArgumentNullException.ThrowIfNull(club);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        if (mode == CareerFinanceMode.Continue
            && _store.TryLoad(club.ExternalId, out CareerFinanceState? existing))
        {
            return ToOverview(existing!);
        }

        if (mode == CareerFinanceMode.New)
        {
            _store.Clear();
        }

        CareerFinanceState initial = CreateOpeningState(club);
        _store.Save(initial);
        return ToOverview(initial);
    }

    public ClubFinanceOverview GetOverview(string clubExternalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clubExternalId);
        if (!_store.TryLoad(clubExternalId, out CareerFinanceState? state))
        {
            throw new InvalidOperationException(
                $"Finance state for club '{clubExternalId}' has not been initialized.");
        }

        return ToOverview(state!);
    }

    private CareerFinanceState CreateOpeningState(ClubRow club)
    {
        long revenue = checked((club.Prestige * 250_000L) + (club.Fanbase * 150_000L));
        long costs = checked(revenue * 55L / 100L);
        long debt = club.Prestige < 50 ? checked((50L - club.Prestige) * 200_000L) : 0L;
        ClubLedger ledger = CreateOpeningLedger(club.ExternalId, revenue, costs, debt);
        int year = _timeProvider.GetUtcNow().Year;
        AnnualClubFinancialState annual = ledger.Snapshot(year);
        long transferBudget = Math.Max(0L, annual.Cash * 60L / 100L);
        long wageBudget = annual.Cost * 45L / 100L;

        return new CareerFinanceState(
            club.ExternalId,
            year,
            Currency,
            annual.Cash,
            annual.Revenue,
            annual.Cost,
            annual.Debt,
            transferBudget,
            wageBudget,
            _timeProvider.GetUtcNow());
    }

    private static ClubLedger CreateOpeningLedger(
        string clubId,
        long revenue,
        long costs,
        long debt)
    {
        const string cash = "account:cash";
        const string income = "account:revenue";
        const string expenses = "account:costs";
        const string liabilities = "account:debt";
        var transactions = new List<LedgerTransaction>
        {
            new("opening:revenue", 0, cash, income, revenue, Currency),
            new("opening:costs", 0, expenses, cash, costs, Currency),
        };
        if (debt > 0)
        {
            transactions.Add(new("opening:debt", 0, cash, liabilities, debt, Currency));
        }

        return new ClubLedger(
            clubId,
            Currency,
            [
                new LedgerAccount(cash, clubId, LedgerAccountKind.Cash),
                new LedgerAccount(income, clubId, LedgerAccountKind.Revenue),
                new LedgerAccount(expenses, clubId, LedgerAccountKind.Cost),
                new LedgerAccount(liabilities, clubId, LedgerAccountKind.Debt),
            ],
            transactions);
    }

    private static ClubFinanceOverview ToOverview(CareerFinanceState state) =>
        new(
            state.ClubExternalId,
            state.SeasonYear,
            state.Currency,
            state.Cash,
            state.Revenue,
            state.Costs,
            checked(state.Revenue - state.Costs),
            state.Debt,
            state.TransferBudget,
            state.WageBudget,
            state.UpdatedAtUtc);
}
