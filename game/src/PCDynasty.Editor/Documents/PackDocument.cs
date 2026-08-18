using System.Collections.Immutable;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Editor.Documents;

public sealed record PackValueDocument(string Key, string Value);

public sealed record PackReferenceDocument(
    string RelationId,
    string TargetExternalId,
    bool Required);

public sealed class PackEntityDocument
{
    public PackEntityDocument(
        string externalId,
        string kindId,
        IEnumerable<PackValueDocument> values,
        IEnumerable<PackReferenceDocument> references)
    {
        ExternalId = externalId;
        KindId = kindId;
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(references);
        Values = values.ToImmutableArray();
        References = references.ToImmutableArray();
    }

    public string ExternalId { get; }

    public string KindId { get; }

    public ImmutableArray<PackValueDocument> Values { get; }

    public ImmutableArray<PackReferenceDocument> References { get; }
}

public sealed class PackDocument
{
    public PackDocument(
        string providerId,
        int schemaVersion,
        IEnumerable<PackEntityDocument> entities)
    {
        ProviderId = providerId;
        SchemaVersion = schemaVersion;
        ArgumentNullException.ThrowIfNull(entities);
        Entities = entities.ToImmutableArray();
    }

    public string ProviderId { get; }

    public int SchemaVersion { get; }

    public ImmutableArray<PackEntityDocument> Entities { get; }

    public PackDocument Upsert(PackEntityDocument entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        int existingIndex = FindEntityIndex(entity.ExternalId);
        ImmutableArray<PackEntityDocument> next = existingIndex < 0
            ? Entities.Add(entity)
            : Entities.SetItem(existingIndex, entity);
        return new PackDocument(ProviderId, SchemaVersion, next);
    }

    public PackDocument Remove(string externalId)
    {
        ArgumentNullException.ThrowIfNull(externalId);
        return new PackDocument(
            ProviderId,
            SchemaVersion,
            Entities.Where(item => !string.Equals(
                item.ExternalId,
                externalId,
                StringComparison.Ordinal)));
    }

    private int FindEntityIndex(string externalId)
    {
        for (int index = 0; index < Entities.Length; index++)
        {
            if (string.Equals(
                    Entities[index].ExternalId,
                    externalId,
                    StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}

public sealed record PackDocumentDiagnostic(
    string Code,
    string Message,
    string? ExternalId = null);

public sealed class PackDocumentResult
{
    internal PackDocumentResult(
        NormalizedContentModel? model,
        ImmutableArray<PackDocumentDiagnostic> diagnostics)
    {
        Model = model;
        Diagnostics = diagnostics;
    }

    public NormalizedContentModel? Model { get; }

    public ImmutableArray<PackDocumentDiagnostic> Diagnostics { get; }

    public bool IsValid => Model is not null && Diagnostics.IsEmpty;
}

public interface IPackDocumentService
{
    PackDocumentResult ValidateAndNormalize(PackDocument document);
}

public sealed class PackDocumentService : IPackDocumentService
{
    public PackDocumentResult ValidateAndNormalize(PackDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = ImmutableArray.CreateBuilder<PackDocumentDiagnostic>();
        string? providerId = Required(document.ProviderId)
            ? document.ProviderId.Trim()
            : null;
        if (providerId is null)
        {
            diagnostics.Add(new(
                "editor.provider.required",
                "A provider identifier is required."));
        }

        if (document.SchemaVersion <= 0)
        {
            diagnostics.Add(new(
                "editor.schema-version.invalid",
                "Schema version must be greater than zero."));
        }

        var entityIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (PackEntityDocument entity in document.Entities)
        {
            if (!Required(entity.ExternalId))
            {
                diagnostics.Add(new(
                    "editor.entity.id-required",
                    "An entity external identifier is required."));
                continue;
            }

            string externalId = entity.ExternalId.Trim();
            if (!entityIds.Add(externalId))
            {
                diagnostics.Add(new(
                    "editor.entity.duplicate-id",
                    $"External identifier '{externalId}' is duplicated.",
                    externalId));
            }

            if (!Required(entity.KindId))
            {
                diagnostics.Add(new(
                    "editor.entity.kind-required",
                    "An entity kind is required.",
                    externalId));
            }

            ValidateValues(entity, externalId, diagnostics);
        }

        foreach (PackEntityDocument entity in document.Entities)
        {
            ValidateReferences(entity, entityIds, diagnostics);
        }

        if (diagnostics.Count > 0 || providerId is null)
        {
            return new PackDocumentResult(null, diagnostics.ToImmutable());
        }

        var model = new NormalizedContentModel(
            providerId,
            document.SchemaVersion,
            document.Entities.Select(entity => new NormalizedContentEntity(
                entity.ExternalId.Trim(),
                entity.KindId.Trim(),
                entity.Values.Select(value => new KeyValuePair<string, string>(
                    value.Key.Trim(),
                    value.Value)),
                entity.References.Select(reference => new ContentReference(
                    reference.RelationId.Trim(),
                    reference.TargetExternalId.Trim(),
                    reference.Required)))));
        return new PackDocumentResult(model, []);
    }

    private static void ValidateValues(
        PackEntityDocument entity,
        string externalId,
        ImmutableArray<PackDocumentDiagnostic>.Builder diagnostics)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (PackValueDocument value in entity.Values)
        {
            if (!Required(value.Key) || value.Value is null)
            {
                diagnostics.Add(new(
                    "editor.value.invalid",
                    "Value keys must be non-empty and values cannot be null.",
                    externalId));
                continue;
            }

            if (!keys.Add(value.Key.Trim()))
            {
                diagnostics.Add(new(
                    "editor.value.duplicate-key",
                    $"Value key '{value.Key.Trim()}' is duplicated.",
                    externalId));
            }
        }
    }

    private static void ValidateReferences(
        PackEntityDocument entity,
        HashSet<string> entityIds,
        ImmutableArray<PackDocumentDiagnostic>.Builder diagnostics)
    {
        foreach (PackReferenceDocument reference in entity.References)
        {
            if (!Required(reference.RelationId) || !Required(reference.TargetExternalId))
            {
                diagnostics.Add(new(
                    "editor.reference.invalid",
                    "Reference identifiers must be non-empty.",
                    entity.ExternalId));
                continue;
            }

            if (reference.Required && !entityIds.Contains(reference.TargetExternalId.Trim()))
            {
                diagnostics.Add(new(
                    "editor.reference.missing",
                    $"Required target '{reference.TargetExternalId.Trim()}' does not exist.",
                    entity.ExternalId));
            }
        }
    }

    private static bool Required(string? value) => !string.IsNullOrWhiteSpace(value);
}
