using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Worlds;

/// <summary>
/// Outcome of loading a world: the compiled pack, the hash that identifies its
/// exact content, and every diagnostic produced on the way.
/// </summary>
public sealed class WorldPackLoadResult
{
    public WorldPackLoadResult(
        CompiledContentPack? pack,
        string contentHash,
        IEnumerable<ContentDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        Pack = pack;
        ContentHash = contentHash;
        Diagnostics = diagnostics.Select(diagnostic =>
        {
            ArgumentNullException.ThrowIfNull(diagnostic);
            return diagnostic;
        }).ToImmutableArray();
    }

    public CompiledContentPack? Pack { get; }

    /// <summary>
    /// Hash of the normalized content, independent of the file format it came
    /// from. A save can bind to it and refuse to open against different data.
    /// </summary>
    public string ContentHash { get; }

    public ImmutableArray<ContentDiagnostic> Diagnostics { get; }

    public bool IsSuccess =>
        Pack is not null
        && !Diagnostics.Any(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error);

    public string DescribeErrors() =>
        string.Join(
            "; ",
            Diagnostics
                .Where(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error)
                .Select(diagnostic => $"[{diagnostic.Code}] {diagnostic.Message}"));
}
