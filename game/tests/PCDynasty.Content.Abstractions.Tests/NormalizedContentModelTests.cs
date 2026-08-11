using PCDynasty.Content.Abstractions;
using Xunit;

namespace PCDynasty.Content.Abstractions.Tests;

public sealed class NormalizedContentModelTests
{
    [Fact]
    public void CopiesVersionedProviderDtosAtTheBoundary()
    {
        var values = new Dictionary<string, string> { ["name"] = "Atlántico CF" };
        var references = new[]
        {
            new ContentReference("relation:country", "country:monteluz", required: true),
        };
        var entities = new[]
        {
            new NormalizedContentEntity("club:atlantico", "entity:club", values, references),
        };
        var model = new NormalizedContentModel("provider:community", schemaVersion: 3, entities);

        values["name"] = "changed";
        entities[0] = new NormalizedContentEntity("changed", "changed", new Dictionary<string, string>(), []);

        Assert.Equal("provider:community", model.ProviderId);
        Assert.Equal(3, model.SchemaVersion);
        Assert.Equal("club:atlantico", model.Entities[0].ExternalId);
        Assert.Equal("Atlántico CF", model.Entities[0].Values["name"]);
        Assert.Equal("country:monteluz", model.Entities[0].References[0].TargetExternalId);
    }

    [Fact]
    public void RepresentsDiagnosticsWithoutImporterDependencies()
    {
        var result = new ContentReadResult(
            model: null,
            [new ContentDiagnostic(
                ContentDiagnosticSeverity.Error,
                "content.invalid-schema",
                "Schema version is not supported.",
                externalId: null)]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ContentDiagnosticSeverity.Error, result.Diagnostics[0].Severity);
        Assert.Null(result.Model);

        string[] references = typeof(IContentReader).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(references, name =>
            name.Contains("Json", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RequiresUniqueExternalIdsAndPositiveSchemaVersions()
    {
        var duplicate = new NormalizedContentEntity(
            "club:atlantico",
            "entity:club",
            new Dictionary<string, string>(),
            []);

        Assert.Throws<ArgumentException>(() => new NormalizedContentModel(
            "provider:community",
            3,
            [duplicate, duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContentReadRequest("/external/content.json", "provider:community", expectedSchemaVersion: 0));
    }
}
