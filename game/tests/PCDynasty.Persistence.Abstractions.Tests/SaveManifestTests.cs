using System.Collections.Immutable;
using PCDynasty.Persistence.Abstractions;
using Xunit;

namespace PCDynasty.Persistence.Abstractions.Tests;

public sealed class SaveManifestTests
{
    [Fact]
    public void SeparatesFormatSchemaSnapshotFactsAndPackHashes()
    {
        var hashes = new Dictionary<string, string>
        {
            ["pack:football"] = "sha256:abc123",
        };
        var manifest = new SaveManifest(
            "save:career:1",
            saveFormatVersion: 2,
            worldSchemaVersion: 7,
            snapshotSequence: 40,
            lastFactSequence: 42,
            hashes);

        hashes["pack:football"] = "changed";

        Assert.Equal(2, manifest.SaveFormatVersion);
        Assert.Equal(7, manifest.WorldSchemaVersion);
        Assert.Equal(40, manifest.SnapshotSequence);
        Assert.Equal(42, manifest.LastFactSequence);
        Assert.Equal("sha256:abc123", manifest.PackHashes["pack:football"]);
    }

    [Fact]
    public void CopiesSnapshotAndFactPayloadsAtTheContractBoundary()
    {
        var snapshotBytes = new byte[] { 1, 2, 3 };
        var factBytes = new byte[] { 4, 5 };
        var snapshot = new WorldSnapshot(sequence: 40, schemaVersion: 7, snapshotBytes);
        var fact = new WorldFact(sequence: 41, typeId: "fact:person-updated", schemaVersion: 3, factBytes);

        snapshotBytes[0] = 99;
        factBytes[0] = 99;

        Assert.Equal((byte)1, snapshot.Payload[0]);
        Assert.Equal((byte)4, fact.Payload[0]);
    }

    [Fact]
    public void RejectsInconsistentSequenceBoundaries()
    {
        Assert.Throws<ArgumentException>(() => new SaveManifest(
            "save:career:1",
            2,
            7,
            snapshotSequence: 43,
            lastFactSequence: 42,
            new Dictionary<string, string> { ["pack:football"] = "sha256:abc123" }));

        var manifest = new SaveManifest(
            "save:career:1",
            2,
            7,
            40,
            42,
            new Dictionary<string, string> { ["pack:football"] = "sha256:abc123" });
        var snapshot = new WorldSnapshot(40, 7, [1]);

        Assert.Throws<ArgumentException>(() => new WorldLoadResult(
            manifest,
            snapshot,
            ImmutableArray.Create(
                new WorldFact(42, "fact:skipped-sequence", 1, [2]))));
    }

    [Fact]
    public void PersistenceContractHasNoSqliteDependency()
    {
        string[] references = typeof(IWorldStore).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name =>
            name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }
}
