using System.Text.Json;
using System.Text.Json.Serialization;
using PCDynasty.Content.Worlds;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Remembers which world the player chose, so an external source survives a
/// restart instead of silently reverting to the integrated one.
/// </summary>
public sealed class ContentSourcePreference
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _filePath;

    public ContentSourcePreference(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    /// <summary>The stored external path, or null when the integrated world applies.</summary>
    public string? TryLoadExternalPath()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            StoredPreference? stored = JsonSerializer.Deserialize<StoredPreference>(
                File.ReadAllText(_filePath),
                JsonOptions);
            string? path = stored?.ExternalPath;
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }
        catch (JsonException)
        {
            // A corrupt preference must never stop the game from opening: the
            // integrated world is always a valid fallback.
            return null;
        }
    }

    public void SaveExternalPath(string externalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalPath);
        Write(new StoredPreference { ExternalPath = externalPath.Trim() });
    }

    public void ClearExternalPath() => Write(new StoredPreference { ExternalPath = null });

    /// <summary>The stored choice, falling back to the integrated world.</summary>
    public ContentSourceDescriptor Resolve(string integratedContentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(integratedContentRoot);
        string? external = TryLoadExternalPath();
        if (external is null)
        {
            return ContentSourceDescriptor.Integrated(integratedContentRoot);
        }

        try
        {
            return ContentSourceDescriptor.External(external);
        }
        catch (ArgumentException)
        {
            // An unsupported stored path is a stale choice, not a crash.
            return ContentSourceDescriptor.Integrated(integratedContentRoot);
        }
    }

    private void Write(StoredPreference preference)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_filePath, JsonSerializer.Serialize(preference, JsonOptions));
    }

    private sealed class StoredPreference
    {
        public string? ExternalPath { get; set; }
    }
}
