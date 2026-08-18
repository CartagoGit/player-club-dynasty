using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Worlds;
using Xunit;

namespace PCDynasty.Content.Tests;

/// <summary>
/// The loader is the only supported way into a world. Audit a00001 found the
/// runtime skipping the validator and the compiler and coercing unparsable
/// numbers to zero; these tests are the gate that keeps that route closed.
/// </summary>
public sealed class WorldPackLoaderTests
{
    private static readonly string[] NumericKeys = ["prestige", "technique"];

    [Fact]
    public async Task LoadMergesDocumentsCompilesAndHashes()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "54")],
            ["people"] = [Player("person:alven", "club:aurora", "62")],
        });

        WorldPackLoadResult result = await new WorldPackLoader(reader).LoadAsync(Source("clubs", "people"));

        Assert.True(result.IsSuccess, result.DescribeErrors());
        Assert.Equal(2, result.Pack!.Entities.Count);
        Assert.Single(result.Pack.EntitiesByKind["club"]);
        Assert.Equal(64, result.ContentHash.Length);
    }

    [Fact]
    public async Task ReferencesAcrossDocumentsAreValidated()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "54")],
            ["people"] = [Player("person:alven", "club:missing", "62")],
        });

        WorldPackLoadResult result = await new WorldPackLoader(reader).LoadAsync(Source("clubs", "people"));

        Assert.False(result.IsSuccess);
        Assert.Contains("club:missing", result.DescribeErrors(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnUnparsableNumberFailsInsteadOfBecomingZero()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "muy alto")],
        });

        WorldPackLoadResult result = await new WorldPackLoader(reader).LoadAsync(Source("clubs"));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Pack);
        string errors = result.DescribeErrors();
        Assert.Contains("club:aurora", errors, StringComparison.Ordinal);
        Assert.Contains("prestige", errors, StringComparison.Ordinal);
        Assert.Contains("clubs", errors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnEntityDeclaredTwiceIsRejectedWithBothDocuments()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "54")],
            ["extra"] = [Club("club:aurora", "60")],
        });

        WorldPackLoadResult result = await new WorldPackLoader(reader).LoadAsync(Source("clubs", "extra"));

        Assert.False(result.IsSuccess);
        Assert.Contains("clubs", result.DescribeErrors(), StringComparison.Ordinal);
        Assert.Contains("extra", result.DescribeErrors(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHashIgnoresDocumentOrderAndValueOrder()
    {
        var straight = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "54")],
            ["people"] = [Player("person:alven", "club:aurora", "62")],
        });
        var swapped = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["people"] = [Player("person:alven", "club:aurora", "62")],
            ["clubs"] = [Club("club:aurora", "54")],
        });

        WorldPackLoadResult first = await new WorldPackLoader(straight).LoadAsync(Source("clubs", "people"));
        WorldPackLoadResult second = await new WorldPackLoader(swapped).LoadAsync(Source("people", "clubs"));

        Assert.True(second.IsSuccess, second.DescribeErrors());
        Assert.Equal(first.ContentHash, second.ContentHash);
    }

    [Fact]
    public async Task DifferentContentProducesADifferentHash()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "54")],
        });
        var changed = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>
        {
            ["clubs"] = [Club("club:aurora", "55")],
        });

        WorldPackLoadResult first = await new WorldPackLoader(reader).LoadAsync(Source("clubs"));
        WorldPackLoadResult second = await new WorldPackLoader(changed).LoadAsync(Source("clubs"));

        Assert.NotEqual(first.ContentHash, second.ContentHash);
    }

    [Fact]
    public async Task ReaderDiagnosticsAreSurfacedAndStopTheLoad()
    {
        var reader = new FakeReader(new Dictionary<string, IReadOnlyList<NormalizedContentEntity>>())
        {
            Failure = new ContentDiagnostic(
                ContentDiagnosticSeverity.Error,
                "content.source.missing",
                "clubs.json was not found."),
        };

        WorldPackLoadResult result = await new WorldPackLoader(reader).LoadAsync(Source("clubs"));

        Assert.False(result.IsSuccess);
        Assert.Contains("clubs.json", result.DescribeErrors(), StringComparison.Ordinal);
    }

    private static WorldPackSource Source(params string[] documents) =>
        new("starter-world", 1, documents, NumericKeys);

    private static NormalizedContentEntity Club(string externalId, string prestige) =>
        new(externalId, "club", [KeyValuePair.Create("prestige", prestige)], []);

    private static NormalizedContentEntity Player(string externalId, string clubId, string technique) =>
        new(
            externalId,
            "person",
            [KeyValuePair.Create("technique", technique)],
            [new ContentReference("club", clubId, required: true)]);

    private sealed class FakeReader(IReadOnlyDictionary<string, IReadOnlyList<NormalizedContentEntity>> documents)
        : IContentReader
    {
        internal ContentDiagnostic? Failure { get; set; }

        public string FormatId => "fake";

        public ValueTask<ContentReadResult> ReadAsync(
            ContentReadRequest request,
            CancellationToken cancellationToken = default)
        {
            if (Failure is not null)
            {
                return ValueTask.FromResult(new ContentReadResult(null, [Failure]));
            }

            IReadOnlyList<NormalizedContentEntity> entities =
                documents.TryGetValue(request.SourcePath, out IReadOnlyList<NormalizedContentEntity>? found)
                    ? found
                    : [];
            return ValueTask.FromResult(new ContentReadResult(
                new NormalizedContentModel(
                    request.ProviderId,
                    request.ExpectedSchemaVersion,
                    entities.ToImmutableArray()),
                []));
        }
    }
}
