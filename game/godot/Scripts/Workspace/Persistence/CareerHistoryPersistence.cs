using Godot;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Workspace.Persistence;

internal sealed class CareerHistoryPersistence
{
    private readonly CareerStore _store;
    private readonly CareerSession _session;
    private bool _loaded;

    private CareerHistoryPersistence(CareerStore store, CareerSession session)
    {
        _store = store;
        _session = session;
    }

    internal static CareerHistoryPersistence CreateDefault(CareerSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        string savesRoot = Path.GetFullPath(Path.Combine(
            ProjectSettings.GlobalizePath("res://../"),
            "..",
            "saves"));
        return new CareerHistoryPersistence(
            new CareerStore(Path.Combine(savesRoot, "career.sqlite")),
            session);
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
