using System.Collections.Immutable;
using PCDynasty.Persistence.Abstractions;
using PCDynasty.Persistence.Sqlite;
using Xunit;

namespace PCDynasty.Persistence.Sqlite.Tests;

public sealed class SqliteWorldStoreTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        $"pcdynasty-save-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task CheckpointAppendAndReopenProduceAConsistentWorld()
    {
        Directory.CreateDirectory(temporaryDirectory);
        string databasePath = Path.Combine(temporaryDirectory, "external-career.pcdsave");
        var store = new SqliteWorldStore(databasePath);
        var initialManifest = Manifest(snapshotSequence: 0, lastFactSequence: 0);

        await store.CheckpointAsync(initialManifest, new WorldSnapshot(0, 7, [1, 2]));
        await store.AppendAsync(
            initialManifest.SaveId,
            expectedLastFactSequence: 0,
            ImmutableArray.Create(
                new WorldFact(1, "fact:person-created", 1, [3]),
                new WorldFact(2, "fact:attribute-changed", 1, [4])));

        var reopened = new SqliteWorldStore(databasePath);
        WorldLoadResult? loaded = await reopened.LoadAsync(initialManifest.SaveId);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Manifest.LastFactSequence);
        Assert.Equal([1, 2], loaded.Snapshot.Payload.AsSpan().ToArray());
        Assert.Equal([1L, 2L], loaded.Facts.Select(fact => fact.Sequence));
        Assert.Equal("sha256:abc123", loaded.Manifest.PackHashes["pack:football"]);
    }

    [Fact]
    public async Task FailedAppendDoesNotPartiallyWriteAndCheckpointIsAtomic()
    {
        Directory.CreateDirectory(temporaryDirectory);
        string databasePath = Path.Combine(temporaryDirectory, "external-career.pcdsave");
        var store = new SqliteWorldStore(databasePath);
        SaveManifest initialManifest = Manifest(0, 0);
        await store.CheckpointAsync(initialManifest, new WorldSnapshot(0, 7, [1]));
        await store.AppendAsync(
            initialManifest.SaveId,
            0,
            ImmutableArray.Create(new WorldFact(1, "fact:first", 1, [2])));

        await Assert.ThrowsAsync<WorldStoreConcurrencyException>(async () =>
            await store.AppendAsync(
                initialManifest.SaveId,
                expectedLastFactSequence: 0,
                ImmutableArray.Create(new WorldFact(1, "fact:duplicate", 1, [9]))));

        WorldLoadResult beforeCheckpoint = Assert.IsType<WorldLoadResult>(
            await store.LoadAsync(initialManifest.SaveId));
        Assert.Single(beforeCheckpoint.Facts);
        Assert.Equal("fact:first", beforeCheckpoint.Facts[0].TypeId);

        await store.CheckpointAsync(Manifest(1, 1), new WorldSnapshot(1, 7, [7, 8]));
        WorldLoadResult afterCheckpoint = Assert.IsType<WorldLoadResult>(
            await store.LoadAsync(initialManifest.SaveId));
        Assert.Empty(afterCheckpoint.Facts);
        Assert.Equal([7, 8], afterCheckpoint.Snapshot.Payload.AsSpan().ToArray());
    }

    public void Dispose()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static SaveManifest Manifest(long snapshotSequence, long lastFactSequence)
    {
        return new SaveManifest(
            "save:career:1",
            saveFormatVersion: 1,
            worldSchemaVersion: 7,
            snapshotSequence,
            lastFactSequence,
            new Dictionary<string, string> { ["pack:football"] = "sha256:abc123" });
    }
}
