using PCDynasty.Contracts.Content;
using PCDynasty.Contracts.Persistence;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class ContractSchemaTests
{
    [Fact]
    public void ContentImportSchemaIsVersionedAndProviderOwned()
    {
        ContentPackContract[] packs =
        [
            new ContentPackContract("pack:football", version: 3, "sha256:football"),
        ];
        var contract = new ContentImportContract(
            new ContractVersion(1, 0),
            "import:42",
            "provider:community",
            contentSchemaVersion: 3,
            formatId: "sqlite",
            sourceHash: "sha256:source",
            packs);
        packs[0] = new ContentPackContract("pack:changed", 1, "changed");

        Assert.Equal("provider:community", contract.ProviderId);
        Assert.Equal(3, contract.ContentSchemaVersion);
        Assert.Equal("pack:football", contract.Packs[0].PackId);
        Assert.Equal("sha256:source", contract.SourceHash);
    }

    [Fact]
    public void SaveManifestSchemaCopiesExternalPackHashesAndSequences()
    {
        var hashes = new Dictionary<string, string>
        {
            ["pack:football"] = "sha256:football",
        };
        var contract = new SaveManifestContract(
            new ContractVersion(1, 1),
            "save:career",
            saveFormatVersion: 2,
            worldSchemaVersion: 7,
            snapshotSequence: 80,
            lastFactSequence: 99,
            hashes);
        hashes["pack:football"] = "changed";

        Assert.Equal(80, contract.SnapshotSequence);
        Assert.Equal(99, contract.LastFactSequence);
        Assert.Equal("sha256:football", contract.PackHashes["pack:football"]);
    }

    [Fact]
    public void PublicSchemasDoNotReferenceDomainPersistenceOrImporterEntities()
    {
        string[] references = typeof(ContentImportContract).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Type[] propertyTypes = typeof(ContentImportContract).GetProperties()
            .Concat(typeof(ContentPackContract).GetProperties())
            .Concat(typeof(SaveManifestContract).GetProperties())
            .Select(property => property.PropertyType)
            .ToArray();

        Assert.DoesNotContain(references, name =>
            name.Contains("Core", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Persistence", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Content.Abstractions", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyTypes, type =>
            type.Namespace?.StartsWith("PCDynasty.Core", StringComparison.Ordinal) == true);
    }
}
