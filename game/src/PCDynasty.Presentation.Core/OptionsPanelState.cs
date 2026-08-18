using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// The draft model the options panel uses. A snapshot is taken on
/// <see cref="Open"/>; the working draft moves as the player toggles
/// controls; <see cref="Save"/> commits the draft to the store;
/// <see cref="Revert"/> restores the snapshot to the draft and
/// reports the persisted state.
/// </summary>
/// <remarks>
/// The state is the unit-testable core of the panel: tests pin every
/// branch (open → change → save; open → change → revert; open → no
/// change → revert is a no-op) without needing a Godot runtime. The
/// Godot side reads and writes the draft; the panel owns the UI.
/// </remarks>
public sealed class OptionsPanelState
{
    private readonly PresentationPreferencesStore _store;
    private PresentationPreferences _snapshot;
    private PresentationPreferences _draft;

    public OptionsPanelState(PresentationPreferencesStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
        _snapshot = store.Load();
        _draft = _snapshot;
    }

    /// <summary>The snapshot taken when the panel opened.</summary>
    public PresentationPreferences Snapshot => _snapshot;

    /// <summary>The current draft (changes since open).</summary>
    public PresentationPreferences Draft => _draft;

    /// <summary>True when the draft differs from the snapshot (i.e. save would change state).</summary>
    public bool IsDirty => !_snapshot.ValueEquals(_draft);

    /// <summary>
    /// Re-opens the panel: takes a new snapshot from the store and
    /// resets the draft to match.
    /// </summary>
    public void Open()
    {
        _snapshot = _store.Load();
        _draft = _snapshot;
    }

    /// <summary>
    /// Applies a transformation to the draft. The transformer is a
    /// pure function that takes the current draft and returns a new
    /// one; the panel calls it for every toggle, theme pick and
    /// language pick.
    /// </summary>
    public void Apply(Func<PresentationPreferences, PresentationPreferences> transformer)
    {
        ArgumentNullException.ThrowIfNull(transformer);
        _draft = transformer(_draft);
    }

    /// <summary>
    /// Persists the draft to the store and advances the snapshot.
    /// Returns the value that was written; callers use it to refresh
    /// the running scene.
    /// </summary>
    public PresentationPreferences Save()
    {
        _store.Save(_draft);
        _snapshot = _draft;
        return _draft;
    }

    /// <summary>
    /// Discards the draft and reloads from the store. Returns the
    /// snapshot so the caller can reset the running scene to the
    /// pre-open state.
    /// </summary>
    public PresentationPreferences Revert()
    {
        _draft = _snapshot;
        _store.Save(_snapshot);
        return _snapshot;
    }
}
