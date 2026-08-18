using PCDynasty.Content.Abstractions;
using Xunit;

namespace PCDynasty.Content.Tests;

public sealed class PackCompilerTests
{
    [Fact]
    public void CompilesValidContentIntoDeterministicRuntimeIndexes()
    {
        NormalizedContentEntity club = Entity(
            "club:atlantico",
            "entity:club",
            new ContentReference("dependency:country", "country:monteluz", required: true));
        NormalizedContentEntity country = Entity("country:monteluz", "entity:country");
        var model = new NormalizedContentModel("provider:community", 3, [club, country]);

        PackCompilationResult result = new PackCompiler().Compile(model);

        Assert.True(result.IsSuccess);
        CompiledContentPack pack = Assert.IsType<CompiledContentPack>(result.Pack);
        Assert.True(pack.TryGetEntity("club:atlantico", out NormalizedContentEntity? indexedClub));
        Assert.Same(club, indexedClub);
        Assert.Equal(
            ["country:monteluz", "club:atlantico"],
            pack.DependencyOrder.Select(entity => entity.ExternalId));
        Assert.Same(club, Assert.Single(pack.EntitiesByKind["entity:club"]));
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void RejectsMissingRequiredReferencesWithSourceDiagnostics()
    {
        NormalizedContentEntity club = Entity(
            "club:atlantico",
            "entity:club",
            new ContentReference("relation:country", "country:missing", required: true));
        var model = new NormalizedContentModel("provider:community", 3, [club]);

        PackCompilationResult result = new PackCompiler().Compile(model);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Pack);
        ContentDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(ContentDiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("content.reference.missing", diagnostic.Code);
        Assert.Equal("club:atlantico", diagnostic.ExternalId);
        Assert.Contains("country:missing", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsDependencyCyclesBeforeRuntime()
    {
        NormalizedContentEntity first = Entity(
            "rules:first",
            "entity:rules",
            new ContentReference("dependency:rules", "rules:second", required: true));
        NormalizedContentEntity second = Entity(
            "rules:second",
            "entity:rules",
            new ContentReference("dependency:rules", "rules:first", required: true));
        var model = new NormalizedContentModel("provider:community", 3, [first, second]);

        PackCompilationResult result = new PackCompiler().Compile(model);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Pack);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Severity == ContentDiagnosticSeverity.Error &&
            diagnostic.Code == "content.dependency.cycle" &&
            diagnostic.Message.Contains("rules:first", StringComparison.Ordinal) &&
            diagnostic.Message.Contains("rules:second", StringComparison.Ordinal));
    }

    [Fact]
    public void AllowsMissingOptionalReferencesAsWarnings()
    {
        NormalizedContentEntity club = Entity(
            "club:atlantico",
            "entity:club",
            new ContentReference("relation:rival", "club:missing", required: false));
        var model = new NormalizedContentModel("provider:community", 3, [club]);

        PackCompilationResult result = new PackCompiler().Compile(model);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Pack);
        ContentDiagnostic warning = Assert.Single(result.Diagnostics);
        Assert.Equal(ContentDiagnosticSeverity.Warning, warning.Severity);
        Assert.Equal("content.reference.optional-missing", warning.Code);
    }

    private static NormalizedContentEntity Entity(
        string externalId,
        string kindId,
        params ContentReference[] references) =>
        new(externalId, kindId, new Dictionary<string, string>(), references);
}
