using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Reads and writes <see cref="PresentationPreferences"/> to a JSON file next
/// to the existing <c>content-source.json</c>. The store is forgiving: a
/// missing or corrupt file returns the defaults, an unknown theme or locale
/// falls back to the catalog's default, never throwing at the application
/// boundary.
/// </summary>
public sealed class PresentationPreferencesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;
    private readonly ThemePaletteCatalog _palettes;
    private readonly LocaleCatalog _locales;

    public PresentationPreferencesStore(
        string filePath,
        ThemePaletteCatalog palettes,
        LocaleCatalog locales)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
        _palettes = palettes ?? throw new ArgumentNullException(nameof(palettes));
        _locales = locales ?? throw new ArgumentNullException(nameof(locales));
    }

    /// <summary>Returns the stored preferences, falling back to the defaults.</summary>
    public PresentationPreferences Load()
    {
        if (!File.Exists(_filePath))
        {
            return PresentationPreferences.Default;
        }

        StoredPreferences? stored;
        try
        {
            stored = JsonSerializer.Deserialize<StoredPreferences>(File.ReadAllText(_filePath), JsonOptions);
        }
        catch (JsonException)
        {
            return PresentationPreferences.Default;
        }

        if (stored is null)
        {
            return PresentationPreferences.Default;
        }

        return Normalise(stored);
    }

    /// <summary>Writes the preferences to disk, creating the directory if needed.</summary>
    public void Save(PresentationPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_filePath, JsonSerializer.Serialize(ToStored(preferences), JsonOptions));
    }

    private PresentationPreferences Normalise(StoredPreferences stored) =>
        new(
            fullscreen: stored.Fullscreen ?? PresentationPreferences.Default.Fullscreen,
            reducedMotion: stored.ReducedMotion ?? PresentationPreferences.Default.ReducedMotion,
            denseTables: stored.DenseTables ?? PresentationPreferences.Default.DenseTables,
            themeId: _palettes.Resolve(stored.ThemeId).Id,
            localeId: _locales.Resolve(stored.LocaleId).Id);

    private static StoredPreferences ToStored(PresentationPreferences preferences)
    {
        StoredPreferences stored = new()
        {
            Fullscreen = preferences.Fullscreen,
            ReducedMotion = preferences.ReducedMotion,
            DenseTables = preferences.DenseTables,
            ThemeId = preferences.ThemeId.Value,
            LocaleId = preferences.LocaleId.Value,
        };
        return stored;
    }

    /// <summary>The on-disk shape. Nullable fields accept partial / older saves.</summary>
    private sealed class StoredPreferences
    {
        public bool? Fullscreen { get; set; }
        public bool? ReducedMotion { get; set; }
        public bool? DenseTables { get; set; }
        public string? ThemeId { get; set; }
        public string? LocaleId { get; set; }
    }
}
