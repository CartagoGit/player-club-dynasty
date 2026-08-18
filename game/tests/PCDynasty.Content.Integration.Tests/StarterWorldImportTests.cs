using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PCDynasty.Content;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;
using Xunit;

namespace PCDynasty.Content.Integration.Tests;

public sealed class StarterWorldImportTests
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions GoldenWriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly string[] DocumentNames =
    [
        "countries.json",
        "clubs.json",
        "people.json",
        "competitions.json",
        "rules.json",
    ];

    [Fact]
    public async Task ImportsToAnImmutableReproduciblePack()
    {
        CompiledContentPack forward = await ImportAsync(DocumentNames);
        CompiledContentPack reversed = await ImportAsync(DocumentNames.Reverse());
        if (Environment.GetEnvironmentVariable("PCDYNASTY_UPDATE_GOLDEN") == "1")
        {
            await WriteExpectedAsync(forward);
        }

        ExpectedSnapshot expected = await ReadExpectedAsync();

        Assert.Equal(expected.ProviderId, forward.ProviderId);
        Assert.Equal(expected.SchemaVersion, forward.SchemaVersion);
        Assert.Equal(expected.EntityCount, forward.Entities.Count);
        Assert.Equal(
            expected.CountsByKind,
            forward.EntitiesByKind.ToDictionary(pair => pair.Key, pair => pair.Value.Length));
        Assert.Equal(expected.CanonicalSha256, CanonicalHash(forward));
        Assert.Equal(CanonicalHash(forward), CanonicalHash(reversed));
        Assert.Equal(
            expected.DependencyOrderTail,
            forward.DependencyOrder.TakeLast(expected.DependencyOrderTail.Length)
                .Select(entity => entity.ExternalId));

        Assert.All(typeof(CompiledContentPack).GetProperties(), property =>
            Assert.Null(property.SetMethod));
        Assert.All(typeof(NormalizedContentEntity).GetProperties(), property =>
            Assert.Null(property.SetMethod));
    }

    [Fact]
    public async Task ManifestDeclaresFictionalPublicDomainOriginAndExactDocuments()
    {
        string manifestPath = Path.Combine(ContentRoot, "pack.json");
        await using FileStream stream = File.OpenRead(manifestPath);
        using JsonDocument manifest = await JsonDocument.ParseAsync(stream);
        JsonElement root = manifest.RootElement;

        Assert.Equal("pcdynasty.starter-world", root.GetProperty("packId").GetString());
        Assert.Equal("1.0.0", root.GetProperty("version").GetString());
        Assert.Equal("CC0-1.0", root.GetProperty("license").GetProperty("spdxId").GetString());
        Assert.True(root.GetProperty("origin").GetProperty("fictional").GetBoolean());
        Assert.Equal(0, root.GetProperty("dependencies").GetArrayLength());
        Assert.Equal(
            DocumentNames.Select(name => $"data/{name}"),
            root.GetProperty("documents").EnumerateArray().Select(item => item.GetString()));
    }

    private static string ContentRoot => Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "starter-world");

    private static async Task<CompiledContentPack> ImportAsync(IEnumerable<string> documentNames)
    {
        var reader = new JsonContentReader();
        var entities = new List<NormalizedContentEntity>();
        foreach (string documentName in documentNames)
        {
            ContentReadResult result = await reader.ReadAsync(new ContentReadRequest(
                Path.Combine(ContentRoot, "data", documentName),
                "pcdynasty.starter-world",
                expectedSchemaVersion: 1));
            Assert.True(result.IsSuccess, string.Join(
                Environment.NewLine,
                result.Diagnostics.Select(item => $"{item.Code}: {item.Message}")));
            entities.AddRange(Assert.IsType<NormalizedContentModel>(result.Model).Entities);
        }

        var model = new NormalizedContentModel(
            "pcdynasty.starter-world",
            1,
            entities);
        PackCompilationResult compilation = new PackCompiler().Compile(model);
        Assert.True(compilation.IsSuccess, string.Join(
            Environment.NewLine,
            compilation.Diagnostics.Select(item => $"{item.Code}: {item.Message}")));
        return Assert.IsType<CompiledContentPack>(compilation.Pack);
    }

    private static string CanonicalHash(CompiledContentPack pack)
    {
        var canonical = new StringBuilder();
        canonical.Append(pack.ProviderId).Append('|').Append(pack.SchemaVersion).Append('\n');
        foreach (NormalizedContentEntity entity in pack.Entities.Values.OrderBy(
                     item => item.ExternalId,
                     StringComparer.Ordinal))
        {
            canonical.Append(entity.ExternalId).Append('|').Append(entity.KindId);
            foreach ((string key, string value) in entity.Values.OrderBy(
                         pair => pair.Key,
                         StringComparer.Ordinal))
            {
                canonical.Append("|v:").Append(key).Append('=').Append(value);
            }

            foreach (ContentReference reference in entity.References.OrderBy(
                         item => item.RelationId,
                         StringComparer.Ordinal).ThenBy(
                         item => item.TargetExternalId,
                         StringComparer.Ordinal))
            {
                canonical.Append("|r:")
                    .Append(reference.RelationId)
                    .Append('=')
                    .Append(reference.TargetExternalId)
                    .Append(':')
                    .Append(reference.Required ? '1' : '0');
            }

            canonical.Append('\n');
        }

        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(canonical.ToString()))).ToLowerInvariant();
    }

    /// <summary>
    /// Rewrites the golden snapshot from the current pack. Guarded by
    /// PCDYNASTY_UPDATE_GOLDEN=1 so it never runs in CI: content grows on
    /// purpose, and hand-editing a hash is how a golden file stops meaning
    /// anything.
    /// </summary>
    private static async Task WriteExpectedAsync(CompiledContentPack pack)
    {
        var snapshot = new ExpectedSnapshot
        {
            ProviderId = pack.ProviderId,
            SchemaVersion = pack.SchemaVersion,
            EntityCount = pack.Entities.Count,
            CountsByKind = pack.EntitiesByKind.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Length),
            CanonicalSha256 = CanonicalHash(pack),
            DependencyOrderTail = pack.DependencyOrder.TakeLast(3)
                .Select(entity => entity.ExternalId)
                .ToArray(),
        };
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "Fixtures",
            "StarterWorldExpected.json");
        await File.WriteAllTextAsync(
            Path.GetFullPath(path),
            JsonSerializer.Serialize(snapshot, GoldenWriteOptions) + "\n");
    }

    private static async Task<ExpectedSnapshot> ReadExpectedAsync()
    {
        await using FileStream stream = File.OpenRead(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "StarterWorldExpected.json"));
        return await JsonSerializer.DeserializeAsync<ExpectedSnapshot>(
            stream,
            SnapshotJsonOptions) ??
            throw new InvalidDataException("The starter-world expectation is empty.");
    }

    private sealed class ExpectedSnapshot
    {
        public string ProviderId { get; init; } = string.Empty;
        public int SchemaVersion { get; init; }
        public int EntityCount { get; init; }
        public Dictionary<string, int> CountsByKind { get; init; } = [];
        public string CanonicalSha256 { get; init; } = string.Empty;
        public string[] DependencyOrderTail { get; init; } = [];
    }
}
