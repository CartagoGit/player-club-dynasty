using System.Collections.Immutable;
using PCDynasty.Presentation.Core.Diagnostics;
using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Diagnostics;

/// <summary>
/// The application error is the unit-testable contract between any
/// failure in the game and the modal the player sees. The tests pin
/// four things:
///   - the builder covers every source: arbitrary exception,
///     activation outcome, picker rejection;
///   - the severity reflects the source (Fatal for an unhandled
///     exception, Fatal for a fatal activation, Warning for a
///     recoverable activation, Warning for a picker rejection);
///   - the items list carries the concrete diagnostics (missing
///     keys, stack frames, …) so the player can paste them;
///   - the report formats the whole thing into plain text in a
///     stable order so the player can paste it into a ticket and
///     the team can search for it.
/// </summary>
public sealed class ApplicationErrorTests
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
    public void AnExceptionBecomesAFatalErrorWithTypeMessageAndFrames()
    {
        try
        {
            throw new InvalidOperationException("kaboom");
        }
        catch (Exception ex)
        {
            ApplicationError error = ApplicationErrorBuilder.FromException(ex);

            Assert.Equal(ApplicationErrorSeverity.Fatal, error.Severity);
            Assert.Equal("Unexpected error", error.Title);
            Assert.Contains("Type", string.Join(",", error.Items.Select(i => i.Label)));
            Assert.Contains("Message", string.Join(",", error.Items.Select(i => i.Label)));
            Assert.Contains("at", string.Join(",", error.Items.Select(i => i.Label)));
            Assert.Equal("Close", error.PrimaryButtonLabel);
            Assert.Equal("Copy details", error.SecondaryButtonLabel);
        }
    }

    [Fact]
    public void AnActivationOutcomeRecoversItsSeverity()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid(
                "fr",
                "res://locale/langs/fr.json",
                LocaleLoadFailure.NotFound));

        ApplicationError error = ApplicationErrorBuilder.FromActivationOutcome(recovered);

        Assert.Equal(ApplicationErrorSeverity.Warning, error.Severity);
        Assert.Equal("translation.Recoverable", error.ErrorCode);
        Assert.Equal("Use English", error.PrimaryButtonLabel);
    }

    [Fact]
    public void AFatalActivationOutcomeBecomesAFatalError()
    {
        LocaleActivationOutcome.Fatal fatal = new(
            SavedFailure: BuildInvalid("fr", "res://locale/langs/fr.json", LocaleLoadFailure.NotFound),
            FallbackFailure: BuildInvalid("en", "res://locale/langs/en.json", LocaleLoadFailure.NotFound));

        ApplicationError error = ApplicationErrorBuilder.FromActivationOutcome(fatal);

        Assert.Equal(ApplicationErrorSeverity.Fatal, error.Severity);
        Assert.Equal("translation.Fatal", error.ErrorCode);
        Assert.Equal("Quit", error.PrimaryButtonLabel);
    }

    [Fact]
    public void APickerRejectionBecomesAWarningWithTheFilePath()
    {
        LocaleLoadResult.Invalid invalid = BuildInvalid(
            "fr",
            "res://locale/langs/fr.json",
            LocaleLoadFailure.InvalidShape,
            (LocaleDiagnosticKind.Missing, "market.col.price", "missing key"));

        ApplicationError error = ApplicationErrorBuilder.FromPickerRejection("fr", invalid);

        Assert.Equal(ApplicationErrorSeverity.Warning, error.Severity);
        Assert.Equal("translation.PickerRejected", error.ErrorCode);
        Assert.Equal("OK", error.PrimaryButtonLabel);
        Assert.Equal("res://locale/langs/fr.json", error.FilePath);
        Assert.Single(error.Items);
        Assert.Equal("missing", error.Items[0].Label);
        Assert.Contains("market.col.price", error.Items[0].Detail);
    }

    [Fact]
    public void TranslationItemsAreTaggedByTheirKind()
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

        ApplicationError error = ApplicationErrorBuilder.FromActivationOutcome(recovered);

        string[] labels = error.Items.Select(i => i.Label).ToArray();
        Assert.Equal(3, labels.Length);
        Assert.Equal(ExpectedDiagnosticLabels, labels);
    }

    private static readonly string[] ExpectedDiagnosticLabels = new[] { "missing", "invalid", "extra" };

    [Fact]
    public void TheReportIncludesSeverityTitleBodyPathCodeAndItems()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid(
                "fr",
                "res://locale/langs/fr.json",
                LocaleLoadFailure.InvalidShape,
                (LocaleDiagnosticKind.Missing, "market.col.price", "missing key")));

        ApplicationError error = ApplicationErrorBuilder.FromActivationOutcome(recovered);
        string report = ApplicationErrorReport.Format(error);

        Assert.Contains("[Warning]", report);
        Assert.Contains(error.Title, report);
        Assert.Contains(error.Body, report);
        Assert.Contains("fr.json", report);
        Assert.Contains("translation.Recoverable", report);
        Assert.Contains("Details:", report);
        Assert.Contains("market.col.price", report);
    }

    [Fact]
    public void TheReportIsStableAcrossTwoRuns()
    {
        LocaleActivationOutcome.Recovered recovered = new(
            File: null!,
            Source: LocaleSource.BuiltIn,
            OriginalFailure: BuildInvalid(
                "fr",
                "res://locale/langs/fr.json",
                LocaleLoadFailure.InvalidShape,
                (LocaleDiagnosticKind.Missing, "market.col.price", "missing key")));

        ApplicationError error = ApplicationErrorBuilder.FromActivationOutcome(recovered);
        string first = ApplicationErrorReport.Format(error);
        string second = ApplicationErrorReport.Format(error);
        Assert.Equal(first, second);
    }

    [Fact]
    public void TheReportOmitsPathAndCodeWhenAbsent()
    {
        ApplicationError error = new(
            Severity: ApplicationErrorSeverity.Warning,
            Title: "Title",
            Body: "Body",
            FilePath: null,
            PhysicalPath: null,
            ErrorCode: null,
            Items: ImmutableArray<ApplicationErrorItem>.Empty,
            PrimaryButtonLabel: "OK",
            SecondaryButtonLabel: null);

        string report = ApplicationErrorReport.Format(error);

        Assert.DoesNotContain("Path:", report);
        Assert.DoesNotContain("Code:", report);
        Assert.DoesNotContain("Details:", report);
    }
}
