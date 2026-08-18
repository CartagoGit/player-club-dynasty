using PCDynasty.Contracts.Careers;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Creates one isolated in-memory history owner for each career opening.
/// Persistence remains behind the external career-store contract.
/// </summary>
public sealed class CareerSessionFactory
{
    private readonly ICareerHistoryStore _store;

    public CareerSessionFactory(ICareerHistoryStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public CareerSession CreateNew()
    {
        _store.Clear();
        return new CareerSession();
    }

    public CareerSession Restore()
    {
        var session = new CareerSession();
        foreach (HistoryEntry match in _store.LoadMatches())
        {
            session.Record(match);
        }

        foreach (SeasonHistoryEntry season in _store.LoadSeasons())
        {
            session.Record(season);
        }

        return session;
    }
}
