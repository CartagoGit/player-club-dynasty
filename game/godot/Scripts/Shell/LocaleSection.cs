using Godot;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Core.Localization;
using PCDynasty.Presentation.Theme;

namespace PCDynasty.Presentation.Shell;

/// <summary>
/// A row of buttons inside the options panel that lets the player
/// pick a locale. The section reads the registry on
/// <see cref="_Ready"/> and renders one button per
/// <see cref="LocaleMetadata"/>, sorted by id.
/// </summary>
/// <remarks>
/// The section owns the spinner and the rejection panel: tapping a
/// flag shows the spinner, calls
/// <see cref="LocaleActivationService.TryLoad"/>, OK applies the
/// locale, failure shows panel C and keeps the previous active
/// locale. The section never persists the player's choice — that is
/// the options panel's job (S8), which fires the same
/// <see cref="LocaleSelected"/> event with the same id.
/// </remarks>
public sealed partial class LocaleSection : VBoxContainer
{
    private readonly LocaleSession _session;
    private readonly LocaleActivationService _service;
    private readonly List<SelectableTile> _tiles = [];

    /// <summary>Fired when the player picks a locale (OK or failure — see <see cref="LocaleSelectionResult"/>).</summary>
    public event Action<LocaleSelectionResult>? LocaleSelected;

    /// <summary>The id the section last activated (after a successful load).</summary>
    public string SelectedId => _session.ActiveId;

    public LocaleSection(LocaleSession session, LocaleActivationService service)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(service);
        _session = session;
        _service = service;
        Name = "LocaleSection";
        AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
    }

    public override void _Ready()
    {
        // The plate that hosts this section already carries the group's name
        // on its caption bar; repeating it inside is noise.
        AddChild(Caption(
            _session.ActiveFile.Options.LanguageHint,
            ManagerDesignTokens.TextSecondary,
            12));

        var rows = new HFlowContainer { Name = "LocaleRows" };
        rows.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space8);
        rows.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space8);

        LocaleRegistryResult scan = _service.Scan();
        foreach (LocaleMetadata entry in scan.Locales)
        {
            rows.AddChild(LocaleTile(entry));
        }

        AddChild(rows);
    }

    /// <remarks>
    /// A <see cref="SelectableTile"/> rather than a <see cref="Button"/>: the
    /// tile pairs a flag with a caption, and a button given <see cref="Control"/>
    /// children has them swallow the click and draw outside its frame — which is
    /// exactly why this picker used to be impossible to press.
    /// </remarks>
    private SelectableTile LocaleTile(LocaleMetadata entry)
    {
        var tile = new SelectableTile(_tiles)
        {
            Name = $"Locale_{entry.Id}",
            TooltipText = entry.Name,
            Area = ShellArea.Settings,
            CustomMinimumSize = new Vector2(160, ManagerDesignTokens.MinimumPointerTarget + 12),
        };

        var row = new HBoxContainer { Name = "LocaleLayout" };
        row.AddThemeConstantOverride("separation", ManagerDesignTokens.Space8);
        // The procedural flag is a placeholder until the real flag
        // assets ship with the content pack. The icon path lives in
        // the JSON metadata; this fallback keeps the picker non-empty
        // when the path is missing.
        var flag = new FlagIcon { Name = "Flag" };
        flag.SetGeometry(ProceduralFlagFor(entry.Id));
        row.AddChild(flag);

        var label = new Label
        {
            Name = "Name",
            Text = entry.Name,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        label.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        row.AddChild(label);
        tile.AddContent(row);

        tile.Selected = entry.Id == _session.ActiveId;
        tile.Pressed += () => Select(entry);
        return tile;
    }

    private void Select(LocaleMetadata entry)
    {
        var spinner = new LocaleSpinner();
        AddChild(spinner);

        LocaleLoadResult result = _service.TryLoad(entry.Id);
        if (result is LocaleLoadResult.Ok ok)
        {
            _session.Apply(ok);
            spinner.QueueFree();
            LocaleSelected?.Invoke(new LocaleSelectionResult(ok.File.Lang, ok.Source, null));
            return;
        }

        spinner.QueueFree();
        LocaleLoadResult.Invalid failure = result as LocaleLoadResult.Invalid
            ?? new LocaleLoadResult.Invalid(
                Id: entry.Id,
                Path: entry.Path,
                Failure: LocaleLoadFailure.NotFound,
                Diagnostics: System.Collections.Immutable.ImmutableArray<LocaleDiagnostic>.Empty);

        LocaleSelected?.Invoke(new LocaleSelectionResult(
            Selected: entry.Id,
            Source: entry.Source,
            Failure: PCDynasty.Presentation.Core.Diagnostics.ApplicationErrorBuilder.FromPickerRejection(entry.Id, failure)));
    }

    private static Label Caption(string text, Color color, int fontSize)
    {
        var label = new Label { Text = text };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        return label;
    }

    /// <summary>
    /// Placeholder flag for any locale id. The real flag assets ship
    /// with the content pack; until they do, every language gets the
    /// same neutral geometric tile so the picker is not empty.
    /// </summary>
    private static FlagGeometry ProceduralFlagFor(string id) =>
        new(
            primary: new ThemeColor(0x1F / 255f, 0x3A / 255f, 0x5F / 255f),
            secondary: new ThemeColor(1f, 1f, 1f),
            tertiary: new ThemeColor(0x1F / 255f, 0x3A / 255f, 0x5F / 255f),
            operations: new[]
            {
                FlagOperation.Rect(new ThemeColor(0x1F / 255f, 0x3A / 255f, 0x5F / 255f), 0f, 0f, 1f, 1f),
            });
}

/// <summary>Result of a <see cref="LocaleSection"/> tap.</summary>
/// <param name="Selected">The id the player picked.</param>
/// <param name="Source">Where the metadata was found (built-in vs external).</param>
/// <param name="Failure">When non-null, panel C to render.</param>
public sealed record LocaleSelectionResult(string Selected, LocaleSource Source, PCDynasty.Presentation.Core.Diagnostics.ApplicationError? Failure)
{
    /// <summary>True when the load succeeded and the session was updated.</summary>
    public bool Succeeded => Failure is null;
}
