using System.Collections.Immutable;
using System.Globalization;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Worlds;

/// <summary>
/// The canonical way to turn external documents into a playable world:
/// read → merge → validate → compile → hash.
/// </summary>
/// <remarks>
/// Audit a00001 found the runtime skipping this pipeline: it read five fixed
/// files, kept them as separate normalized models, never ran the validator or
/// the compiler, and silently turned unparsable numbers into zero — which is
/// how a manager with no attributes ended up rendered as a zero-value player.
/// Merging the documents also makes cross-document references verifiable,
/// which per-file validation could never do.
/// </remarks>
public sealed class WorldPackLoader
{
    private const string DuplicateEntityCode = "content.entity.duplicate";
    private const string NotAnIntegerCode = "content.value.not-integer";

    private readonly IContentReader _reader;
    private readonly PackCompiler _compiler;

    public WorldPackLoader(IContentReader reader, PackCompiler? compiler = null)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _compiler = compiler ?? new PackCompiler();
    }

    public async Task<WorldPackLoadResult> LoadAsync(
        WorldPackSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var diagnostics = ImmutableArray.CreateBuilder<ContentDiagnostic>();
        var entities = ImmutableArray.CreateBuilder<NormalizedContentEntity>();
        var seenExternalIds = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (string documentPath in source.DocumentPaths)
        {
            ContentReadResult read = await _reader
                .ReadAsync(
                    new ContentReadRequest(documentPath, source.ProviderId, source.SchemaVersion),
                    cancellationToken)
                .ConfigureAwait(false);
            diagnostics.AddRange(read.Diagnostics);
            if (read.Model is null)
            {
                continue;
            }

            foreach (NormalizedContentEntity entity in read.Model.Entities)
            {
                if (seenExternalIds.TryGetValue(entity.ExternalId, out string? firstDocument))
                {
                    diagnostics.Add(new ContentDiagnostic(
                        ContentDiagnosticSeverity.Error,
                        DuplicateEntityCode,
                        $"'{entity.ExternalId}' is declared in both '{firstDocument}' and '{documentPath}'.",
                        entity.ExternalId));
                    continue;
                }

                seenExternalIds.Add(entity.ExternalId, documentPath);
                entities.Add(entity);
                diagnostics.AddRange(ValidateNumericValues(entity, documentPath, source.NumericValueKeys));
            }
        }

        ImmutableArray<NormalizedContentEntity> merged = entities.ToImmutable();
        if (HasError(diagnostics))
        {
            return new WorldPackLoadResult(null, string.Empty, diagnostics);
        }

        var model = new NormalizedContentModel(source.ProviderId, source.SchemaVersion, merged);
        PackCompilationResult compilation = _compiler.Compile(model);
        diagnostics.AddRange(compilation.Diagnostics);

        return compilation.Pack is null || HasError(diagnostics)
            ? new WorldPackLoadResult(null, string.Empty, diagnostics)
            : new WorldPackLoadResult(
                compilation.Pack,
                WorldContentHash.Compute(model),
                diagnostics);
    }

    /// <summary>
    /// A declared numeric attribute that does not parse is an error naming the
    /// entity, the attribute, the value and the document. It is never zero.
    /// </summary>
    private static IEnumerable<ContentDiagnostic> ValidateNumericValues(
        NormalizedContentEntity entity,
        string documentPath,
        ImmutableSortedSet<string> numericKeys)
    {
        foreach (string key in numericKeys)
        {
            if (!entity.Values.TryGetValue(key, out string? raw))
            {
                continue;
            }

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                continue;
            }

            yield return new ContentDiagnostic(
                ContentDiagnosticSeverity.Error,
                NotAnIntegerCode,
                $"'{entity.ExternalId}' declares '{key}' as '{raw}' in '{documentPath}', which is not an integer.",
                entity.ExternalId);
        }
    }

    private static bool HasError(ImmutableArray<ContentDiagnostic>.Builder diagnostics) =>
        diagnostics.Any(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error);
}
