using Godot;
using PCDynasty.Presentation.Core;

namespace PCDynasty.Presentation.Workspace.Persistence;

internal sealed class CareerHistoryPersistence
{
    private readonly CareerStore _store;
    private bool _loaded;

    private CareerHistoryPersistence(CareerStore store)
    {
        _store = store;
    }

    internal static CareerHistoryPersistence CreateDefault()
    {
        string savesRoot = Path.GetFullPath(Path.Combine(
            ProjectSettings.GlobalizePath("res://../"),
            "..",
            "saves"));
        return new CareerHistoryPersistence(new CareerStore(Path.Combine(savesRoot, "career.sqlite")));
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
                MatchHistory.Instance.Append(match);
            }

            foreach (SeasonHistoryEntry season in _store.LoadSeasons())
            {
                SeasonHistory.Instance.Append(season);
            }

            _loaded = true;
        }, "load career");
    }

    internal void Save(HistoryEntry entry)
    {
        MatchHistory.Instance.Append(entry);
        TryRun(() => _store.SaveMatch(entry), "persist match");
    }

    internal void Save(SeasonHistoryEntry entry)
    {
        SeasonHistory.Instance.Append(entry);
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
