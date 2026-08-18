using Godot;
using PCDynasty.Contracts.Careers;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Workspace.Persistence;

internal sealed class CareerHistoryPersistence
{
    private readonly ICareerHistoryStore _store;
    private readonly CareerSession _session;
    private bool _loaded;

    internal CareerHistoryPersistence(ICareerHistoryStore store, CareerSession session)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(session);
        _store = store;
        _session = session;
    }

    internal void Load()
    {
        if (_loaded)
        {
            return;
        }

        TryRun(() =>
        {
            foreach (HistoryEntry match in _store.LoadMatches())
            {
                _session.Record(match);
            }

            foreach (SeasonHistoryEntry season in _store.LoadSeasons())
            {
                _session.Record(season);
            }

            _loaded = true;
        }, "load career");
    }

    internal void Save(HistoryEntry entry)
    {
        _session.Record(entry);
        TryRun(() => _store.SaveMatch(entry), "persist match");
    }

    internal void Save(SeasonHistoryEntry entry)
    {
        _session.Record(entry);
        TryRun(() => _store.SaveSeason(entry), "persist season");
    }

    private static void TryRun(Action action, string operation)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            GD.PushWarning($"[CareerHistoryPersistence] Could not {operation}: {exception.Message}");
        }
    }
}
