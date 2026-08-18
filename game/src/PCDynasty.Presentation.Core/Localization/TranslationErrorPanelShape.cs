using System.Collections.Immutable;

namespace PCDynasty.Presentation.Core.Localization;

/// <summary>
/// The three panel shapes the activation / picker pipeline can
/// request. The shape determines the copy and the buttons the panel
/// renders; the renderer is a thin pass-through over the shape.
/// </summary>
public enum TranslationErrorPanelKind
{
    /// <summary>Panel A: saved locale failed, English recovered. Player can continue.</summary>
    Recoverable,

    /// <summary>Panel B: every candidate failed. Player must quit.</summary>
    Fatal,

    /// <summary>Panel C: picker rejected a locale the player tapped. Player can pick another.</summary>
    PickerRejected,
}

/// <summary>
/// The description of what a <see cref="TranslationErrorPanel"/>
/// should render. The shape is a pure value: no Godot types, no I/O.
/// Tests pin the shape; the Godot renderer reads it and turns it
/// into controls.
/// </summary>
/// <param name="Kind">Which of the three panels to render.</param>
/// <param name="Title">The bolded headline.</param>
/// <param name="Body">The first paragraph (always English).</param>
/// <param name="FilePath">The on-disk path of the broken file, when known.</param>
/// <param name="MissingKeys">Keys the loader flagged as missing.</param>
/// <param name="ExtraKeys">Keys the loader flagged as unknown (rendered as warnings).</param>
/// <param name="InvalidKeys">Keys the loader flagged with bad values.</param>
/// <param name="PrimaryButtonLabel">Label of the always-present primary button.</param>
/// <param name="SecondaryButtonLabel">Optional label of a secondary action.</param>
/// <param name="MaximumListHeight">Pixel cap for the scrollable list (0 = unbounded).</param>
public sealed record TranslationErrorPanelShape(
    TranslationErrorPanelKind Kind,
    string Title,
    string Body,
    string FilePath,
    ImmutableArray<TranslationErrorPanelItem> MissingKeys,
    ImmutableArray<TranslationErrorPanelItem> ExtraKeys,
    ImmutableArray<TranslationErrorPanelItem> InvalidKeys,
    string PrimaryButtonLabel,
    string? SecondaryButtonLabel,
    int MaximumListHeight);

/// <summary>One diagnostic the panel renders as a single line.</summary>
public sealed record TranslationErrorPanelItem(string Path, string Message);

/// <summary>
/// Translates a typed <see cref="LocaleActivationOutcome"/> into a
/// <see cref="TranslationErrorPanelShape"/>. The copy is always
/// English because the panel exists precisely because the locale is
/// not trustworthy.
/// </summary>
public static class TranslationErrorPanelShapeBuilder
{
    /// <summary>The pixel cap for the scrollable diagnostic list.</summary>
    public const int DefaultMaximumListHeight = 240;

    /// <summary>Panel A or B for the boot activation outcome.</summary>
    public static TranslationErrorPanelShape ForActivation(LocaleActivationOutcome outcome) =>
        outcome switch
        {
            LocaleActivationOutcome.Recovered recovered => BuildRecoverable(recovered),
            LocaleActivationOutcome.Fatal fatal => BuildFatal(fatal),
            _ => throw new ArgumentException(
                $"Outcome '{outcome.GetType().Name}' does not render a panel.",
                nameof(outcome)),
        };

    /// <summary>Panel C for a locale the player tried to select from the picker.</summary>
    public static TranslationErrorPanelShape ForPicker(string id, LocaleLoadResult.Invalid failure) =>
        new(
            Kind: TranslationErrorPanelKind.PickerRejected,
            Title: "Translation error",
            Body: $"The language file '{id}.json' is invalid. This language cannot be selected. Try another or fix the file.",
            FilePath: failure.Path,
            MissingKeys: ToItems(failure, LocaleDiagnosticKind.Missing),
            ExtraKeys: ToItems(failure, LocaleDiagnosticKind.Unknown),
            InvalidKeys: ToItems(failure, LocaleDiagnosticKind.Invalid),
            PrimaryButtonLabel: "OK",
            SecondaryButtonLabel: null,
            MaximumListHeight: DefaultMaximumListHeight);

    private static TranslationErrorPanelShape BuildRecoverable(LocaleActivationOutcome.Recovered recovered) =>
        new(
            Kind: TranslationErrorPanelKind.Recoverable,
            Title: "Translation error",
            Body: $"The language file '{recovered.OriginalFailure.Id}.json' is invalid and could not be loaded. " +
                  "The game will use English as the active language.",
            FilePath: recovered.OriginalFailure.Path,
            MissingKeys: ToItems(recovered.OriginalFailure, LocaleDiagnosticKind.Missing),
            ExtraKeys: ToItems(recovered.OriginalFailure, LocaleDiagnosticKind.Unknown),
            InvalidKeys: ToItems(recovered.OriginalFailure, LocaleDiagnosticKind.Invalid),
            PrimaryButtonLabel: "Use English",
            SecondaryButtonLabel: null,
            MaximumListHeight: DefaultMaximumListHeight);

    private static TranslationErrorPanelShape BuildFatal(LocaleActivationOutcome.Fatal fatal) =>
        new(
            Kind: TranslationErrorPanelKind.Fatal,
            Title: "Fatal translation error",
            Body: $"Both the active language ('{fatal.SavedFailure.Id}') and the fallback ('{fatal.FallbackFailure.Id}') failed to load. " +
                  "The game cannot continue without a working English file.",
            FilePath: fatal.FallbackFailure.Path,
            MissingKeys: ToItems(fatal.FallbackFailure, LocaleDiagnosticKind.Missing),
            ExtraKeys: ToItems(fatal.FallbackFailure, LocaleDiagnosticKind.Unknown),
            InvalidKeys: ToItems(fatal.FallbackFailure, LocaleDiagnosticKind.Invalid),
            PrimaryButtonLabel: "Quit",
            SecondaryButtonLabel: null,
            MaximumListHeight: DefaultMaximumListHeight);

    private static ImmutableArray<TranslationErrorPanelItem> ToItems(
        LocaleLoadResult.Invalid invalid,
        LocaleDiagnosticKind kind) =>
        invalid.Diagnostics
            .Where(d => d.Kind == kind)
            .Select(d => new TranslationErrorPanelItem(d.Path, d.Message))
            .ToImmutableArray();
}
