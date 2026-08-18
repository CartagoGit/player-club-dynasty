using System.Collections.Immutable;
using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The shape is the unit-testable contract between the activation
/// pipeline and the Godot renderer. The tests pin three things:
///   - the three panels have the right kind, title, body and button;
///   - the diagnostic lists carry every missing / extra / invalid key
///     the loader reported, in the right buckets;
///   - the maximum list height is the documented default (so the
///     renderer can wire it into a ScrollContainer without surprises).
/// </summary>
public sealed class TranslationErrorPanelShapeTests
{
    private static LocaleLoadResult.Invalid BuildInvalid(
        string id,
        string path,
        LocaleLoadFailure failure,
        params (LocaleDiagnosticKind kind, string path, string message)[] diagnostics) =>
        new(
            Id: id,
            Path: path,
            Failure: failure,
            Diagnostics: diagnostics
                .Select(d => new LocaleDiagnostic(d.kind, d.path, d.message))
                .ToImmutableArray());

    [Fact]
    public void ForActivationWithRecoveredReturnsPanelA()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid(
                "fr",
                "res://locale/langs/fr.json",
                LocaleLoadFailure.InvalidShape,
                (LocaleDiagnosticKind.Missing, "market.col.price", "missing key"),
                (LocaleDiagnosticKind.Unknown, "market.unlock_secret", "unknown key"),
                (LocaleDiagnosticKind.Invalid, "icon", "icon is empty")));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForActivation(recovered);

        Assert.Equal(TranslationErrorPanelKind.Recoverable, shape.Kind);
        Assert.Equal("Translation error", shape.Title);
        Assert.Contains("fr.json", shape.Body);
        Assert.Contains("English", shape.Body);
        Assert.Equal("Use English", shape.PrimaryButtonLabel);
        Assert.Null(shape.SecondaryButtonLabel);
        Assert.Equal("res://locale/langs/fr.json", shape.FilePath);
        Assert.Single(shape.MissingKeys);
        Assert.Equal("market.col.price", shape.MissingKeys[0].Path);
        Assert.Single(shape.ExtraKeys);
        Assert.Equal("market.unlock_secret", shape.ExtraKeys[0].Path);
        Assert.Single(shape.InvalidKeys);
        Assert.Equal("icon", shape.InvalidKeys[0].Path);
        Assert.True(shape.MaximumListHeight > 0);
    }

    [Fact]
    public void ForActivationWithFatalReturnsPanelB()
    {
        LocaleActivationOutcome.Fatal fatal = new(
            SavedFailure: BuildInvalid("fr", "res://locale/langs/fr.json", LocaleLoadFailure.NotFound),
            FallbackFailure: BuildInvalid(
                "en",
                "res://locale/langs/en.json",
                LocaleLoadFailure.InvalidShape,
                (LocaleDiagnosticKind.Missing, "menu.title", "missing key")));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForActivation(fatal);

        Assert.Equal(TranslationErrorPanelKind.Fatal, shape.Kind);
        Assert.Equal("Fatal translation error", shape.Title);
        Assert.Contains("fr", shape.Body);
        Assert.Contains("en", shape.Body);
        Assert.Equal("Quit", shape.PrimaryButtonLabel);
        Assert.Single(shape.MissingKeys);
        Assert.Equal("menu.title", shape.MissingKeys[0].Path);
    }

    [Fact]
    public void ForPickerReturnsPanelC()
    {
        LocaleLoadResult.Invalid failure = BuildInvalid(
            "fr",
            "res://locale/langs/fr.json",
            LocaleLoadFailure.InvalidShape,
            (LocaleDiagnosticKind.Missing, "market.col.price", "missing key"));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForPicker("fr", failure);

        Assert.Equal(TranslationErrorPanelKind.PickerRejected, shape.Kind);
        Assert.Equal("Translation error", shape.Title);
        Assert.Contains("cannot be selected", shape.Body);
        Assert.Equal("OK", shape.PrimaryButtonLabel);
        Assert.Null(shape.SecondaryButtonLabel);
        Assert.Single(shape.MissingKeys);
        Assert.Equal("market.col.price", shape.MissingKeys[0].Path);
    }

    [Fact]
    public void AnActivatedOutcomeDoesNotProduceAPanel()
    {
        LocaleActivationOutcome.Activated activated = new(File: null!, Source: LocaleSource.BuiltIn);

        Assert.Throws<ArgumentException>(() => TranslationErrorPanelShapeBuilder.ForActivation(activated));
    }

    [Fact]
    public void EmptyDiagnosticsProduceEmptyLists()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid("fr", "res://locale/langs/fr.json", LocaleLoadFailure.NotFound));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForActivation(recovered);

        Assert.Empty(shape.MissingKeys);
        Assert.Empty(shape.ExtraKeys);
        Assert.Empty(shape.InvalidKeys);
    }

    [Fact]
    public void TheMaximumListHeightIsTheDocumentedDefault()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid("fr", "res://locale/langs/fr.json", LocaleLoadFailure.NotFound));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForActivation(recovered);

        Assert.Equal(TranslationErrorPanelShapeBuilder.DefaultMaximumListHeight, shape.MaximumListHeight);
        Assert.Equal(240, shape.MaximumListHeight);
    }

    [Fact]
    public void DiagnosticsAreSplitByKind()
    {
        LocaleLoadResult.Invalid failure = BuildInvalid(
            "fr",
            "res://locale/langs/fr.json",
            LocaleLoadFailure.InvalidShape,
            (LocaleDiagnosticKind.Missing, "a", "missing a"),
            (LocaleDiagnosticKind.Missing, "b", "missing b"),
            (LocaleDiagnosticKind.Unknown, "c", "unknown c"),
            (LocaleDiagnosticKind.Invalid, "d", "invalid d"),
            (LocaleDiagnosticKind.Invalid, "e", "invalid e"));

        TranslationErrorPanelShape shape = TranslationErrorPanelShapeBuilder.ForPicker("fr", failure);

        Assert.Equal(2, shape.MissingKeys.Length);
        Assert.Single(shape.ExtraKeys);
        Assert.Equal(2, shape.InvalidKeys.Length);
    }
}
