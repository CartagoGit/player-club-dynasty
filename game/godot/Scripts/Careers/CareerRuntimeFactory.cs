using PCDynasty.Application.Finances;
using PCDynasty.Application.Matches;
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

    internal CareerRuntime Create(CareerRuntimeMode mode, ClubRow club)
    {
        ArgumentNullException.ThrowIfNull(club);
        var historyStore = new SqliteCareerHistoryStore(_databasePath);
        var lineupStore = new SqliteCareerLineupStore(_databasePath);
        var financeStore = new SqliteCareerFinanceStore(_databasePath);
        var sessionFactory = new CareerSessionFactory(historyStore);

        CareerSession session;
        if (mode == CareerRuntimeMode.New)
        {
            lineupStore.Clear();
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

        return new CareerRuntime(
            session,
            new CareerLineupService(lineupStore, _timeProvider),
            new CareerHistoryPersistence(historyStore, session),
            finances);
    }
}
