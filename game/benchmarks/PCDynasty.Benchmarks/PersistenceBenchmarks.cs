using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using PCDynasty.Persistence.Abstractions;
using PCDynasty.Persistence.Sqlite;

namespace PCDynasty.Benchmarks;

[MemoryDiagnoser]
public class PersistenceBenchmarks
{
    public const int SnapshotBytes = 1_048_576;
    public const int FactCount = 256;

    private string temporaryDirectory = string.Empty;
    private SqliteWorldStore loadStore = null!;
    private SqliteWorldStore saveStore = null!;
    private SaveManifest saveManifest = null!;
    private WorldSnapshot saveSnapshot = null!;

    [GlobalSetup]
    public void Setup()
    {
        temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"pcdynasty-bench-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        byte[] payload = CreatePayload(SnapshotBytes);
        saveManifest = CreateManifest("benchmark:save", 0, 0);
        saveSnapshot = new WorldSnapshot(0, 1, payload);
        saveStore = new SqliteWorldStore(Path.Combine(temporaryDirectory, "save.pcdsave"));
        saveStore.CheckpointAsync(saveManifest, saveSnapshot).AsTask().GetAwaiter().GetResult();

        loadStore = new SqliteWorldStore(Path.Combine(temporaryDirectory, "load.pcdsave"));
        SaveManifest loadManifest = CreateManifest("benchmark:load", 0, 0);
        loadStore.CheckpointAsync(
            loadManifest,
            new WorldSnapshot(0, 1, payload)).AsTask().GetAwaiter().GetResult();
        loadStore.AppendAsync(
            loadManifest.SaveId,
            expectedLastFactSequence: 0,
            CreateFacts()).AsTask().GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task<int> LoadSaveAsync()
    {
        WorldLoadResult result = await loadStore.LoadAsync("benchmark:load") ??
            throw new InvalidOperationException("Benchmark save was not found.");
        return result.Snapshot.Payload.Length + result.Facts.Length;
    }

    [Benchmark]
    public async Task<int> SaveCheckpointAsync()
    {
        await saveStore.CheckpointAsync(saveManifest, saveSnapshot);
        return saveSnapshot.Payload.Length;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static SaveManifest CreateManifest(
        string saveId,
        long snapshotSequence,
        long lastFactSequence)
    {
        return new SaveManifest(
            saveId,
            saveFormatVersion: 1,
            worldSchemaVersion: 1,
            snapshotSequence,
            lastFactSequence,
            new Dictionary<string, string>
            {
                ["pcdynasty.starter-world"] = "sha256:benchmark-fixture",
            });
    }

    private static ImmutableArray<WorldFact> CreateFacts()
    {
        var facts = ImmutableArray.CreateBuilder<WorldFact>(FactCount);
        for (int index = 1; index <= FactCount; index++)
        {
            facts.Add(new WorldFact(
                index,
                "benchmark:fact",
                schemaVersion: 1,
                CreatePayload(256)));
        }

        return facts.MoveToImmutable();
    }

    private static byte[] CreatePayload(int length)
    {
        var payload = new byte[length];
        for (int index = 0; index < payload.Length; index++)
        {
            payload[index] = (byte)((BenchmarkConfig.ReproducibleSeed + index) & 0xFF);
        }

        return payload;
    }
}
