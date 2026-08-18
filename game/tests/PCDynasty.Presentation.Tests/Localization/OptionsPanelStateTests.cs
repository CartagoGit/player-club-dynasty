using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The options panel's draft/revert model is the unit-testable core
/// of the panel. The tests pin the three contracts:
///   - open captures the snapshot and resets the draft;
///   - apply mutates the draft without touching the snapshot;
///   - save persists the draft and advances the snapshot;
///   - revert restores the snapshot, both in memory and on disk.
/// </summary>
public sealed class OptionsPanelStateTests
{
    private static PresentationPreferencesStore BuildStore()
    {
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"options-{Guid.NewGuid():N}.json");
        return new PresentationPreferencesStore(path, ThemePaletteCatalog.Shipped, LocaleCatalog.Shipped);
    }

    [Fact]
    public void OpenCapturesTheSnapshotAndResetsTheDraft()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        Assert.Equal(state.Snapshot, state.Draft);
        Assert.False(state.IsDirty);
    }

    [Fact]
    public void ApplyMutatesTheDraftWithoutTouchingTheSnapshot()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        PresentationPreferences original = state.Snapshot;
        state.Apply(d => d.WithFullscreen(!d.Fullscreen));

        Assert.NotEqual(original.Fullscreen, state.Draft.Fullscreen);
        Assert.Equal(original.Fullscreen, state.Snapshot.Fullscreen);
        Assert.True(state.IsDirty);
    }

    [Fact]
    public void SavePersistsTheDraftAndAdvancesTheSnapshot()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        state.Apply(d => d.WithLocale(LocaleId.English));
        PresentationPreferences saved = state.Save();

        Assert.Equal(LocaleId.English, saved.LocaleId);
        Assert.Equal(state.Snapshot, state.Draft);
        Assert.False(state.IsDirty);

        PresentationPreferences reloaded = store.Load();
        Assert.Equal(LocaleId.English, reloaded.LocaleId);
    }

    [Fact]
    public void RevertRestoresTheSnapshotBothInMemoryAndOnDisk()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        state.Apply(d => d.WithFullscreen(true));
        Assert.True(state.IsDirty);

        PresentationPreferences reverted = state.Revert();

        Assert.Equal(state.Snapshot, state.Draft);
        Assert.False(state.IsDirty);
        Assert.Equal(reverted.Fullscreen, state.Snapshot.Fullscreen);
        Assert.False(reverted.Fullscreen);
    }

    [Fact]
    public void RevertAfterNoChangeIsANoOp()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        PresentationPreferences before = state.Snapshot;
        state.Revert();

        Assert.Equal(before, state.Draft);
        Assert.False(state.IsDirty);
    }

    [Fact]
    public void ReopeningAfterSaveStartsWithTheNewSnapshot()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        state.Apply(d => d.WithLocale(LocaleId.English));
        state.Save();

        state.Open();

        Assert.Equal(LocaleId.English, state.Snapshot.LocaleId);
        Assert.Equal(state.Snapshot, state.Draft);
        Assert.False(state.IsDirty);
    }

    [Fact]
    public void ANewApplyAfterSaveIsDirtyAgain()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        state.Apply(d => d.WithLocale(LocaleId.English));
        state.Save();

        state.Apply(d => d.WithFullscreen(!d.Fullscreen));
        Assert.True(state.IsDirty);
    }

    [Fact]
    public void ThemeAndLocaleAndTogglesShareTheSameDraftModel()
    {
        var store = BuildStore();
        var state = new OptionsPanelState(store);

        ThemePaletteId originalTheme = state.Snapshot.ThemeId;
        LocaleId originalLocale = state.Snapshot.LocaleId;
        bool originalFullscreen = state.Snapshot.Fullscreen;

        state.Apply(d => d.WithTheme(ThemePaletteId.Default).WithLocale(LocaleId.English).WithFullscreen(true));
        Assert.True(state.IsDirty);

        state.Revert();

        Assert.Equal(originalTheme, state.Draft.ThemeId);
        Assert.Equal(originalLocale, state.Draft.LocaleId);
        Assert.Equal(originalFullscreen, state.Draft.Fullscreen);
    }
}
