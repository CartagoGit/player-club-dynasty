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
        ExpectedSnapshot expected = await ReadExpectedAsync();
        CompiledContentPack forward = await ImportAsync(DocumentNames);
        CompiledContentPack reversed = await ImportAsync(DocumentNames.Reverse());

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
        canonical.Append(pack.ProviderId).Append('|').Append(pack.SchemaVersion).AppendLine();
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

            canonical.AppendLine();
        }

        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(canonical.ToString()))).ToLowerInvariant();
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
