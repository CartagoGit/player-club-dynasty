using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content;

public sealed class PackCompiler
{
    private readonly IContentPackValidator validator;

    public PackCompiler()
        : this(new PackValidator())
    {
    }

    public PackCompiler(IContentPackValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        this.validator = validator;
    }

    public PackCompilationResult Compile(NormalizedContentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        PackValidationResult validation = validator.Validate(model);
        if (!validation.IsValid)
        {
            return new PackCompilationResult(null, validation.Diagnostics);
        }

        var entities = model.Entities.ToImmutableDictionary(
            entity => entity.ExternalId,
            StringComparer.Ordinal);
        ImmutableDictionary<string, ImmutableArray<NormalizedContentEntity>> entitiesByKind =
            model.Entities
                .GroupBy(entity => entity.KindId, StringComparer.Ordinal)
                .ToImmutableDictionary(
                    group => group.Key,
                    group => group.OrderBy(
                        entity => entity.ExternalId,
                        StringComparer.Ordinal).ToImmutableArray(),
                    StringComparer.Ordinal);
        ImmutableArray<NormalizedContentEntity> dependencyOrder = BuildDependencyOrder(
            model.Entities,
            entities);

        return new PackCompilationResult(
            new CompiledContentPack(
                model.ProviderId,
                model.SchemaVersion,
                entities,
                entitiesByKind,
                dependencyOrder),
            validation.Diagnostics);
    }

    private static ImmutableArray<NormalizedContentEntity> BuildDependencyOrder(
        IEnumerable<NormalizedContentEntity> source,
        ImmutableDictionary<string, NormalizedContentEntity> index)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var ordered = ImmutableArray.CreateBuilder<NormalizedContentEntity>();

        foreach (NormalizedContentEntity entity in source.OrderBy(
                     item => item.ExternalId,
                     StringComparer.Ordinal))
        {
            Visit(entity, index, visited, ordered);
        }

        return ordered.ToImmutable();
    }

    private static void Visit(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, NormalizedContentEntity> index,
        ISet<string> visited,
        ImmutableArray<NormalizedContentEntity>.Builder ordered)
    {
        if (!visited.Add(entity.ExternalId))
        {
            return;
        }

        foreach (NormalizedContentEntity dependency in PackValidator.GetDependencies(entity, index))
        {
            Visit(dependency, index, visited, ordered);
        }

        ordered.Add(entity);
    }
}

public sealed class CompiledContentPack
{
    internal CompiledContentPack(
        string providerId,
        int schemaVersion,
        ImmutableDictionary<string, NormalizedContentEntity> entities,
        ImmutableDictionary<string, ImmutableArray<NormalizedContentEntity>> entitiesByKind,
        ImmutableArray<NormalizedContentEntity> dependencyOrder)
    {
        ProviderId = providerId;
        SchemaVersion = schemaVersion;
        Entities = entities;
        EntitiesByKind = entitiesByKind;
        DependencyOrder = dependencyOrder;
    }

    public string ProviderId { get; }
    public int SchemaVersion { get; }
    public ImmutableDictionary<string, NormalizedContentEntity> Entities { get; }
    public ImmutableDictionary<string, ImmutableArray<NormalizedContentEntity>> EntitiesByKind { get; }
    public ImmutableArray<NormalizedContentEntity> DependencyOrder { get; }

    public bool TryGetEntity(string externalId, out NormalizedContentEntity? entity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        return Entities.TryGetValue(externalId.Trim(), out entity);
    }
}

public sealed class PackCompilationResult
{
    public PackCompilationResult(
        CompiledContentPack? pack,
        IEnumerable<ContentDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        Pack = pack;
        Diagnostics = diagnostics.Select(diagnostic =>
        {
            ArgumentNullException.ThrowIfNull(diagnostic);
            return diagnostic;
        }).ToImmutableArray();

        if (Pack is not null && Diagnostics.Any(diagnostic =>
                diagnostic.Severity == ContentDiagnosticSeverity.Error))
        {
            throw new ArgumentException("A compiled pack cannot contain error diagnostics.", nameof(diagnostics));
        }
    }

    public CompiledContentPack? Pack { get; }
    public ImmutableArray<ContentDiagnostic> Diagnostics { get; }
    public bool IsSuccess => Pack is not null;
}
