using Godot;
using PCDynasty.Content.Worlds;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// Lets the player load a world from outside the game and go back to the
/// integrated one, showing which world is active and its content hash.
/// </summary>
/// <remarks>
/// A node rather than a builder: the scene tree owns these controls, and the
/// section keeps no rules — only its controls and the callbacks the
/// composition root hands it.
/// </remarks>
public sealed partial class ContentSourceSection : VBoxContainer
{
    private Action<string>? _loadExternal;
    private Action? _restoreIntegrated;

    private Label Status => GetNode<Label>("ContentSourceStatus");

    private LineEdit ExternalPath => GetNode<LineEdit>("ExternalPath");

    /// <param name="loadExternal">Starts loading the path; reports back through Refresh or ReportError.</param>
    /// <param name="restoreIntegrated">Starts restoring the shipped world.</param>
    public void Configure(
        ContentSourceDescriptor source,
        string contentHash,
        Action<string> loadExternal,
        Action restoreIntegrated)
    {
        ArgumentNullException.ThrowIfNull(source);
        _loadExternal = loadExternal ?? throw new ArgumentNullException(nameof(loadExternal));
        _restoreIntegrated = restoreIntegrated ?? throw new ArgumentNullException(nameof(restoreIntegrated));

        Name = "ContentSource";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        AddChild(Caption("FUENTE DE DATOS", ManagerDesignTokens.Accent, 12));

        Label status = Caption(Describe(source, contentHash), ManagerDesignTokens.TextSecondary, 13);
        status.Name = "ContentSourceStatus";
        status.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        AddChild(status);

        AddChild(new LineEdit
        {
            Name = "ExternalPath",
            PlaceholderText = "Carpeta de documentos JSON, o archivo .json, .csv, .sqlite",
            Text = source.IsIntegrated ? string.Empty : source.Path,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        });

        var actions = new HBoxContainer { Name = "ContentSourceActions" };
        actions.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        actions.AddChild(Action("LoadExternalSource", "Cargar mundo externo", LoadExternal));
        actions.AddChild(Action("RestoreIntegratedSource", "Restaurar integrado", RestoreIntegrated));
        AddChild(actions);
    }

    /// <summary>Refreshes the line after the world changed.</summary>
    public void Refresh(ContentSourceDescriptor source, string contentHash)
    {
        ArgumentNullException.ThrowIfNull(source);
        Report(Describe(source, contentHash), ManagerDesignTokens.TextSecondary);
    }

    /// <summary>Shows why a world could not be loaded, keeping the active one.</summary>
    public void ReportError(string message) => Report(message, ManagerDesignTokens.Negative);

    private void LoadExternal()
    {
        string path = ExternalPath.Text.Trim();
        if (path.Length == 0)
        {
            Report("Escribe la ruta de un mundo externo.", ManagerDesignTokens.Negative);
            return;
        }

        // Loading never blocks the frame: the section shows it is working and
        // the composition root reports the outcome when the load finishes.
        Report("Cargando mundo externo…", ManagerDesignTokens.TextSecondary);
        _loadExternal!(path);
    }

    private void RestoreIntegrated()
    {
        Report("Restaurando el mundo integrado…", ManagerDesignTokens.TextSecondary);
        _restoreIntegrated!();
    }

    private void Report(string message, Color color)
    {
        Label status = Status;
        status.Text = message;
        status.AddThemeColorOverride("font_color", color);
    }

    private static string Describe(ContentSourceDescriptor source, string contentHash) =>
        $"{(source.IsIntegrated ? "Integrado" : "Externo")} · {source.DisplayName} · {source.FormatId.ToUpperInvariant()} · hash {Short(contentHash)}";

    private static string Short(string contentHash) =>
        contentHash.Length <= 12 ? contentHash : contentHash[..12];

    private static Label Caption(string text, Color color, int fontSize)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        return label;
    }

    private static Button Action(string name, string text, Action handler)
    {
        var button = new Button
        {
            Name = name,
            Text = text,
            CustomMinimumSize = new Vector2(0, ManagerDesignTokens.MinimumPointerTarget),
        };
        button.Pressed += handler;
        return button;
    }
}
