using System.Collections.Immutable;
using PCDynasty.Presentation.Core.Localization;

namespace PCDynasty.Presentation.Core.Diagnostics;

/// <summary>
/// Severity of an application error. Drives the colour of the
/// header strip and the icon glyph so the player can tell at a
/// glance whether the dialog blocks them or merely asks them to
/// confirm.
/// </summary>
public enum ApplicationErrorSeverity
{
    /// <summary>The error is recoverable; the player can continue with a degraded experience.</summary>
    Warning,

    /// <summary>The error is fatal; the only path forward is the primary button's action.</summary>
    Fatal,
}

/// <summary>
/// One bullet inside an <see cref="ApplicationError"/>. Used for the
/// scrollable details list: missing keys, stack frames, validation
/// issues, anything the player might want to copy and paste.
/// </summary>
/// <param name="Label">A short tag the player can scan, e.g. <c>menu.title</c> or <c>at System.IO.File…</c>.</param>
/// <param name="Detail">The full explanation that follows the label.</param>
public sealed record ApplicationErrorItem(string Label, string Detail);

/// <summary>
/// The pure-value contract every error dialog renders. The Godot
/// renderer reads it; tests assert on it; any caller anywhere in the
/// game can build one and surface the same friendly modal.
/// </summary>
public sealed record ApplicationError(
    ApplicationErrorSeverity Severity,
    string Title,
    string Body,
    string? FilePath,
    string? ErrorCode,
    ImmutableArray<ApplicationErrorItem> Items,
    string PrimaryButtonLabel,
    string? SecondaryButtonLabel);

/// <summary>Builders that turn a typed source into an <see cref="ApplicationError"/>.</summary>
public static class ApplicationErrorBuilder
{
    /// <summary>The default pixel cap for the scrollable details list.</summary>
    public const int DefaultMaxItemsHeight = 280;

    /// <summary>
    /// Wraps an arbitrary <see cref="Exception"/> as a fatal dialog.
    /// The body explains the situation in plain language; the items
    /// list carries the stack frames so the player can paste them
    /// when they report the bug.
    /// </summary>
    public static ApplicationError FromException(
        Exception exception,
        string? filePath = null,
        string? errorCode = null,
        string primaryButtonLabel = "Close",
        string? secondaryButtonLabel = "Copy details")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ImmutableArray<ApplicationErrorItem>.Builder items =
            ImmutableArray.CreateBuilder<ApplicationErrorItem>();
        items.Add(new ApplicationErrorItem("Type", exception.GetType().FullName ?? exception.GetType().Name));
        items.Add(new ApplicationErrorItem("Message", exception.Message));
        foreach (string frame in EnumerateFrames(exception))
        {
            items.Add(new ApplicationErrorItem("at", frame));
        }

        return new ApplicationError(
            Severity: ApplicationErrorSeverity.Fatal,
            Title: "Unexpected error",
            Body: "The game hit an unrecoverable problem. The details below describe what happened; "
                + "please copy them and send them to the team when you report the bug.",
            FilePath: filePath,
            ErrorCode: errorCode,
            Items: items.ToImmutable(),
            PrimaryButtonLabel: primaryButtonLabel,
            SecondaryButtonLabel: secondaryButtonLabel);
    }

    /// <summary>
    /// Builds the panel shape for a translation failure that the
    /// activation service emitted. The existing translation copy is
    /// preserved verbatim so S5 acceptance criteria keep holding.
    /// </summary>
    public static ApplicationError FromActivationOutcome(LocaleActivationOutcome outcome)
    {
        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForActivation(outcome);
        return FromTranslationShape(shape);
    }

    /// <summary>
    /// Builds the panel shape for a translation failure that the
    /// language picker rejected. Panel C.
    /// </summary>
    public static ApplicationError FromPickerRejection(string id, LocaleLoadResult.Invalid failure) =>
        FromTranslationShape(TranslationErrorPanelShapeBuilder.ForPicker(id, failure));

    private static ApplicationError FromTranslationShape(TranslationErrorPanelShape shape)
    {
        ImmutableArray<ApplicationErrorItem>.Builder items =
            ImmutableArray.CreateBuilder<ApplicationErrorItem>();
        AppendTranslationItems(items, "missing", shape.MissingKeys);
        AppendTranslationItems(items, "invalid", shape.InvalidKeys);
        AppendTranslationItems(items, "extra", shape.ExtraKeys);

        ApplicationErrorSeverity severity = shape.Kind switch
        {
            TranslationErrorPanelKind.Fatal => ApplicationErrorSeverity.Fatal,
            _ => ApplicationErrorSeverity.Warning,
        };

        return new ApplicationError(
            Severity: severity,
            Title: shape.Title,
            Body: shape.Body,
            FilePath: string.IsNullOrEmpty(shape.FilePath) ? null : shape.FilePath,
            ErrorCode: $"translation.{shape.Kind}",
            Items: items.ToImmutable(),
            PrimaryButtonLabel: shape.PrimaryButtonLabel,
            SecondaryButtonLabel: shape.SecondaryButtonLabel);
    }

    private static void AppendTranslationItems(
        ImmutableArray<ApplicationErrorItem>.Builder items,
        string tag,
        ImmutableArray<TranslationErrorPanelItem> entries)
    {
        if (entries.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (TranslationErrorPanelItem entry in entries)
        {
            items.Add(new ApplicationErrorItem(tag, $"{entry.Path} — {entry.Message}"));
        }
    }

    private static IEnumerable<string> EnumerateFrames(Exception exception)
    {
        string? stack = exception.StackTrace;
        if (string.IsNullOrWhiteSpace(stack))
        {
            yield break;
        }

        foreach (string line in stack.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                yield return trimmed;
            }
        }
    }
}
