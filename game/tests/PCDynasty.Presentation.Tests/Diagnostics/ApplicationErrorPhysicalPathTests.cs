using System.Collections.Immutable;
using PCDynasty.Presentation.Core.Diagnostics;
using Xunit;

namespace PCDynasty.Presentation.Tests.Diagnostics;

/// <summary>
/// The "Copy details" report and the panel both show the file the
/// error refers to. The player should see a path they can open in
/// their file browser, not a Godot resource path they cannot
/// navigate to.
/// </summary>
public sealed class ApplicationErrorPhysicalPathTests
{
    [Fact]
    public void ResPathGlobalizesWhenResolverMapsIt()
    {
        string resolved = ResPath.Globalize(
            "res://locale/langs/built/en.json",
            resolver: _ => "/home/user/game/locale/langs/built/en.json");

        Assert.Equal("/home/user/game/locale/langs/built/en.json", resolved);
    }

    [Fact]
    public void ResPathReturnsTheInputWhenItIsNotAResourcePath()
    {
        string input = "/home/user/something.json";
        Assert.Equal(input, ResPath.Globalize(input));
    }

    [Fact]
    public void ResPathFallsBackToTheInputWhenTheResolverReturnsEmpty()
    {
        string input = "res://does/not/exist";
        string resolved = ResPath.Globalize(input, resolver: _ => string.Empty);

        Assert.Equal(input, resolved);
    }

    [Fact]
    public void ResPathSwallowsPlatformErrorsAndReturnsTheInput()
    {
        string input = "res://locale/langs/built/en.json";
        string resolved = ResPath.Globalize(input, resolver: _ => throw new InvalidOperationException("boom"));

        Assert.Equal(input, resolved);
    }

    [Fact]
    public void ResPathHandlesNullAndEmptyInputGracefully()
    {
        Assert.Equal(string.Empty, ResPath.Globalize(""));
        Assert.Equal(string.Empty, ResPath.Globalize(null!));
    }

    [Fact]
    public void WithPhysicalPathReturnsACopyWithThePhysicalFieldSet()
    {
        ApplicationError original = new(
            Severity: ApplicationErrorSeverity.Warning,
            Title: "T",
            Body: "B",
            FilePath: "res://locale/langs/C.json",
            PhysicalPath: null,
            ErrorCode: null,
            Items: ImmutableArray<ApplicationErrorItem>.Empty,
            PrimaryButtonLabel: "OK",
            SecondaryButtonLabel: null);

        ApplicationError withPath = original.WithPhysicalPath("/game/locale/langs/C.json");

        Assert.Equal("res://locale/langs/C.json", withPath.FilePath);
        Assert.Equal("/game/locale/langs/C.json", withPath.PhysicalPath);
        // Original is untouched.
        Assert.Null(original.PhysicalPath);
    }

    [Fact]
    public void TheReportPrefersThePhysicalPathOverTheResourcePath()
    {
        ApplicationError error = new(
            Severity: ApplicationErrorSeverity.Warning,
            Title: "T",
            Body: "B",
            FilePath: "res://locale/langs/C.json",
            PhysicalPath: "/game/locale/langs/C.json",
            ErrorCode: "translation.Recoverable",
            Items: ImmutableArray<ApplicationErrorItem>.Empty,
            PrimaryButtonLabel: "OK",
            SecondaryButtonLabel: null);

        string report = ApplicationErrorReport.Format(error);

        Assert.Contains("Path: /game/locale/langs/C.json", report);
        Assert.DoesNotContain("res://", report);
    }

    [Fact]
    public void TheReportFallsBackToTheResourcePathWhenNoPhysicalPathIsAvailable()
    {
        ApplicationError error = new(
            Severity: ApplicationErrorSeverity.Warning,
            Title: "T",
            Body: "B",
            FilePath: "res://locale/langs/C.json",
            PhysicalPath: null,
            ErrorCode: null,
            Items: ImmutableArray<ApplicationErrorItem>.Empty,
            PrimaryButtonLabel: "OK",
            SecondaryButtonLabel: null);

        string report = ApplicationErrorReport.Format(error);

        Assert.Contains("Path: res://locale/langs/C.json", report);
    }
}
