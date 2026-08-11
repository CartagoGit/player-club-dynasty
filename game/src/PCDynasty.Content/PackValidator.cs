using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content;

public interface IContentPackValidator
{
    PackValidationResult Validate(NormalizedContentModel model);
}

public sealed class PackValidationResult
{
    public PackValidationResult(IEnumerable<ContentDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        Diagnostics = diagnostics.Select(diagnostic =>
        {
            ArgumentNullException.ThrowIfNull(diagnostic);
            return diagnostic;
        }).ToImmutableArray();
    }

    public ImmutableArray<ContentDiagnostic> Diagnostics { get; }
    public bool IsValid =>
        !Diagnostics.Any(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error);
}

public sealed class PackValidator : IContentPackValidator
{
    private const string DependencyRelationPrefix = "dependency:";

    public PackValidationResult Validate(NormalizedContentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ImmutableDictionary<string, NormalizedContentEntity> entities = model.Entities
            .ToImmutableDictionary(entity => entity.ExternalId, StringComparer.Ordinal);
        var diagnostics = ImmutableArray.CreateBuilder<ContentDiagnostic>();

        ValidateReferences(model.Entities, entities, diagnostics);
        ValidateDependencyGraph(model.Entities, entities, diagnostics);

        return new PackValidationResult(diagnostics);
    }

    private static void ValidateReferences(
        IEnumerable<NormalizedContentEntity> entities,
        ImmutableDictionary<string, NormalizedContentEntity> index,
        ImmutableArray<ContentDiagnostic>.Builder diagnostics)
    {
        foreach (NormalizedContentEntity entity in entities)
        {
            foreach (ContentReference reference in entity.References)
            {
                if (index.ContainsKey(reference.TargetExternalId))
                {
                    continue;
                }

                diagnostics.Add(new ContentDiagnostic(
                    reference.Required
                        ? ContentDiagnosticSeverity.Error
                        : ContentDiagnosticSeverity.Warning,
                    reference.Required
                        ? "content.reference.missing"
                        : "content.reference.optional-missing",
                    $"Reference '{reference.RelationId}' targets missing entity '{reference.TargetExternalId}'.",
                    entity.ExternalId));
            }
        }
    }

    private static void ValidateDependencyGraph(
        IEnumerable<NormalizedContentEntity> entities,
        ImmutableDictionary<string, NormalizedContentEntity> index,
        ImmutableArray<ContentDiagnostic>.Builder diagnostics)
    {
        var states = new Dictionary<string, VisitState>(StringComparer.Ordinal);
        var path = new List<string>();
        var reportedCycles = new HashSet<string>(StringComparer.Ordinal);

        foreach (NormalizedContentEntity entity in entities.OrderBy(
                     item => item.ExternalId,
                     StringComparer.Ordinal))
        {
            Visit(entity, index, states, path, reportedCycles, diagnostics);
        }
    }

    private static void Visit(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, NormalizedContentEntity> index,
        Dictionary<string, VisitState> states,
        IList<string> path,
        ISet<string> reportedCycles,
        ImmutableArray<ContentDiagnostic>.Builder diagnostics)
    {
        if (states.GetValueOrDefault(entity.ExternalId) == VisitState.Visited)
        {
            return;
        }

        states[entity.ExternalId] = VisitState.Visiting;
        path.Add(entity.ExternalId);

        foreach (NormalizedContentEntity dependency in GetDependencies(entity, index))
        {
            VisitState dependencyState = states.GetValueOrDefault(dependency.ExternalId);
            if (dependencyState == VisitState.Visiting)
            {
                int cycleStart = path.IndexOf(dependency.ExternalId);
                string[] cycle = path.Skip(cycleStart).Append(dependency.ExternalId).ToArray();
                string key = string.Join('\u001f', cycle.OrderBy(id => id, StringComparer.Ordinal));
                if (reportedCycles.Add(key))
                {
                    diagnostics.Add(new ContentDiagnostic(
                        ContentDiagnosticSeverity.Error,
                        "content.dependency.cycle",
                        $"Dependency cycle detected: {string.Join(" -> ", cycle)}.",
                        entity.ExternalId));
                }

                continue;
            }

            if (dependencyState == VisitState.Unvisited)
            {
                Visit(dependency, index, states, path, reportedCycles, diagnostics);
            }
        }

        path.RemoveAt(path.Count - 1);
        states[entity.ExternalId] = VisitState.Visited;
    }

    internal static IEnumerable<NormalizedContentEntity> GetDependencies(
        NormalizedContentEntity entity,
        ImmutableDictionary<string, NormalizedContentEntity> index) =>
        entity.References
            .Where(reference => reference.RelationId.StartsWith(
                DependencyRelationPrefix,
                StringComparison.Ordinal))
            .Select(reference => index.GetValueOrDefault(reference.TargetExternalId))
            .Where(dependency => dependency is not null)
            .Cast<NormalizedContentEntity>()
            .OrderBy(dependency => dependency.ExternalId, StringComparer.Ordinal);

    private enum VisitState
    {
        Unvisited,
        Visiting,
        Visited,
    }
}
