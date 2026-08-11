using System.Collections.Immutable;

namespace PCDynasty.Content.Abstractions;

public enum ContentDiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

public sealed class ContentDiagnostic
{
    public ContentDiagnostic(
        ContentDiagnosticSeverity severity,
        string code,
        string message,
        string? externalId = null)
    {
        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(nameof(severity));
        }

        Severity = severity;
        Code = ContentValue.NormalizeIdentifier(code, nameof(code));
        Message = ContentValue.NormalizeRequired(message, nameof(message));
        ExternalId = externalId is null
            ? null
            : ContentValue.NormalizeIdentifier(externalId, nameof(externalId));
    }

    public ContentDiagnosticSeverity Severity { get; }
    public string Code { get; }
    public string Message { get; }
    public string? ExternalId { get; }
}

public sealed class ContentReference
{
    public ContentReference(string relationId, string targetExternalId, bool required)
    {
        RelationId = ContentValue.NormalizeIdentifier(relationId, nameof(relationId));
        TargetExternalId = ContentValue.NormalizeIdentifier(targetExternalId, nameof(targetExternalId));
        Required = required;
    }

    public string RelationId { get; }
    public string TargetExternalId { get; }
    public bool Required { get; }
}

public sealed class NormalizedContentEntity
{
    public NormalizedContentEntity(
        string externalId,
        string kindId,
        IEnumerable<KeyValuePair<string, string>> values,
        IEnumerable<ContentReference> references)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(references);

        var normalizedValues = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in values)
        {
            string normalizedKey = ContentValue.NormalizeIdentifier(key, nameof(values));
            ArgumentNullException.ThrowIfNull(value);
            if (!normalizedValues.TryAdd(normalizedKey, value))
            {
                throw new ArgumentException("Value identifiers must be unique.", nameof(values));
            }
        }

        ExternalId = ContentValue.NormalizeIdentifier(externalId, nameof(externalId));
        KindId = ContentValue.NormalizeIdentifier(kindId, nameof(kindId));
        Values = normalizedValues.ToImmutable();
        References = references.Select(reference =>
        {
            ArgumentNullException.ThrowIfNull(reference);
            return reference;
        }).ToImmutableArray();
    }

    public string ExternalId { get; }
    public string KindId { get; }
    public ImmutableDictionary<string, string> Values { get; }
    public ImmutableArray<ContentReference> References { get; }
}

public sealed class NormalizedContentModel
{
    public NormalizedContentModel(
        string providerId,
        int schemaVersion,
        IEnumerable<NormalizedContentEntity> entities)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
        ArgumentNullException.ThrowIfNull(entities);

        var normalizedEntities = ImmutableArray.CreateBuilder<NormalizedContentEntity>();
        var externalIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (NormalizedContentEntity entity in entities)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if (!externalIds.Add(entity.ExternalId))
            {
                throw new ArgumentException("External identifiers must be unique.", nameof(entities));
            }

            normalizedEntities.Add(entity);
        }

        ProviderId = ContentValue.NormalizeIdentifier(providerId, nameof(providerId));
        SchemaVersion = schemaVersion;
        Entities = normalizedEntities.ToImmutable();
    }

    public string ProviderId { get; }
    public int SchemaVersion { get; }
    public ImmutableArray<NormalizedContentEntity> Entities { get; }
}

public sealed class ContentReadResult
{
    public ContentReadResult(
        NormalizedContentModel? model,
        IEnumerable<ContentDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        Model = model;
        Diagnostics = diagnostics.Select(diagnostic =>
        {
            ArgumentNullException.ThrowIfNull(diagnostic);
            return diagnostic;
        }).ToImmutableArray();
    }

    public NormalizedContentModel? Model { get; }
    public ImmutableArray<ContentDiagnostic> Diagnostics { get; }
    public bool IsSuccess =>
        Model is not null &&
        !Diagnostics.Any(diagnostic => diagnostic.Severity == ContentDiagnosticSeverity.Error);
}

internal static class ContentValue
{
    public static string NormalizeIdentifier(string value, string parameterName) =>
        NormalizeRequired(value, parameterName);

    public static string NormalizeRequired(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
