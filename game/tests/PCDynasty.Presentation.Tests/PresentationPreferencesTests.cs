using System.IO;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The store must round-trip the preferences, fall back to defaults when the
/// file is missing or corrupt, and downgrade unknown theme or locale ids to
/// the catalog default so a corrupted save never blocks the game.
/// </summary>
public sealed class PresentationPreferencesTests
{
    private static readonly ThemePaletteCatalog Palettes = ThemePaletteCatalog.Shipped;
    private static readonly LocaleCatalog Locales = LocaleCatalog.Shipped;

    [Fact]
    public void TheDefaultsMatchTheShippedShell()
    {
        PresentationPreferences defaults = PresentationPreferences.Default;

        Assert.False(defaults.Fullscreen);
        Assert.True(defaults.ReducedMotion);
        Assert.True(defaults.DenseTables);
        Assert.Equal(ThemePaletteId.ClassicDark, defaults.ThemeId);
        Assert.Equal(LocaleId.Spanish, defaults.LocaleId);
    }

    [Fact]
    public void AMissingFileYieldsDefaults()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string file = Path.Combine(directory, "preferences.json");
            var store = new PresentationPreferencesStore(file, Palettes, Locales);

            PresentationPreferences loaded = store.Load();

            Assert.Equal(PresentationPreferences.Default, new LoadedFlag(loaded));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ARoundTripPreservesEveryField()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string file = Path.Combine(directory, "preferences.json");
            var store = new PresentationPreferencesStore(file, Palettes, Locales);
            PresentationPreferences written = PresentationPreferences.Default
                .WithFullscreen(true)
                .WithReducedMotion(false)
                .WithDenseTables(false)
                .WithTheme(new ThemePaletteId("forest"))
                .WithLocale(LocaleId.English);

            store.Save(written);
            PresentationPreferences loaded = store.Load();

            Assert.True(loaded.ValueEquals(written));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ACorruptFileYieldsDefaults()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string file = Path.Combine(directory, "preferences.json");
            File.WriteAllText(file, "{ not json");
            var store = new PresentationPreferencesStore(file, Palettes, Locales);

            PresentationPreferences loaded = store.Load();

            Assert.Equal(PresentationPreferences.Default, new LoadedFlag(loaded));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void AnUnknownThemeFallsBackToTheCatalogDefault()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string file = Path.Combine(directory, "preferences.json");
            File.WriteAllText(
                file,
                "{\"fullscreen\":true,\"reducedMotion\":true,\"denseTables\":true,\"themeId\":\"not-a-palette\",\"localeId\":\"en\"}");
            var store = new PresentationPreferencesStore(file, Palettes, Locales);

            PresentationPreferences loaded = store.Load();

            Assert.Equal(ThemePaletteId.ClassicDark, loaded.ThemeId);
            Assert.True(loaded.Fullscreen);
            Assert.Equal(LocaleId.English, loaded.LocaleId);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void AnUnknownLocaleFallsBackToTheCatalogDefault()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string file = Path.Combine(directory, "preferences.json");
            File.WriteAllText(
                file,
                "{\"fullscreen\":false,\"reducedMotion\":true,\"denseTables\":true,\"themeId\":\"forest\",\"localeId\":\"fr\"}");
            var store = new PresentationPreferencesStore(file, Palettes, Locales);

            PresentationPreferences loaded = store.Load();

            Assert.Equal(LocaleId.Spanish, loaded.LocaleId);
            Assert.Equal(new ThemePaletteId("forest"), loaded.ThemeId);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void TheStoreCreatesItsDirectoryOnSave()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-prefs-").FullName;
        try
        {
            string nested = Path.Combine(directory, "nested", "preferences.json");
            var store = new PresentationPreferencesStore(nested, Palettes, Locales);

            store.Save(PresentationPreferences.Default);

            Assert.True(File.Exists(nested));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void TheWithHelpersProduceNewInstances()
    {
        PresentationPreferences original = PresentationPreferences.Default;

        PresentationPreferences withTheme = original.WithTheme(new ThemePaletteId("forest"));

        Assert.NotSame(original, withTheme);
        Assert.Equal(original.Fullscreen, withTheme.Fullscreen);
        Assert.Equal(original.LocaleId, withTheme.LocaleId);
        Assert.Equal(new ThemePaletteId("forest"), withTheme.ThemeId);
    }

    /// <summary>A wrapper that disables <see cref="PresentationPreferences"/>'s default reference equality for the assertion.</summary>
    private sealed class LoadedFlag
    {
        private readonly PresentationPreferences _value;

        public LoadedFlag(PresentationPreferences value) => _value = value;

        public override bool Equals(object? obj) =>
            obj is LoadedFlag other && _value.ValueEquals(other._value);

        public override int GetHashCode() => _value.GetHashCode();
    }
}
