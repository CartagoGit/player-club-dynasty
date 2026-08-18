using PCDynasty.Application.Finances;
using PCDynasty.Application.Matches;
using PCDynasty.Application.Market;
using PCDynasty.Application.Workspace;
using PCDynasty.Persistence.Sqlite;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Workspace.Persistence;

namespace PCDynasty.Presentation.Careers;

internal enum CareerRuntimeMode
{
    New = 0,
    Continue = 1,
}

/// <summary>
/// Composition adapter for the single active career database. Every call
/// returns a fresh in-memory session; only Continue hydrates persisted history.
/// </summary>
internal sealed class CareerRuntimeFactory
{
    private readonly string _databasePath;
    private readonly TimeProvider _timeProvider;

    internal CareerRuntimeFactory(string databasePath, TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path cannot be empty.", nameof(databasePath));
        }

        _databasePath = databasePath;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    internal CareerRuntime Create(CareerRuntimeMode mode, ClubRow club, IManagerWorld world)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(world);
        var historyStore = new SqliteCareerHistoryStore(_databasePath);
        var lineupStore = new SqliteCareerLineupStore(_databasePath);
        var financeStore = new SqliteCareerFinanceStore(_databasePath);
        var transferStore = new SqliteCareerTransferStore(_databasePath);
        var sessionFactory = new CareerSessionFactory(historyStore);

        CareerSession session;
        if (mode == CareerRuntimeMode.New)
        {
            lineupStore.Clear();
            transferStore.Clear();
            session = sessionFactory.CreateNew();
        }
        else
        {
            session = sessionFactory.Restore();
        }

        var finances = new CareerFinanceService(financeStore, _timeProvider);
        finances.Open(
            club,
            mode == CareerRuntimeMode.New
                ? CareerFinanceMode.New
                : CareerFinanceMode.Continue);
        var transferMarket = new CareerTransferMarketService(
            world,
            transferStore,
            financeStore,
            _timeProvider);

        return new CareerRuntime(
            session,
            new CareerLineupService(lineupStore, _timeProvider),
            new CareerHistoryPersistence(historyStore, session),
            finances,
            transferMarket,
            transferMarket);
    }
}
