using System.Text.Json;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Importers;

public sealed class JsonContentReader : IContentReader
{
    public string FormatId => "json";

    public async ValueTask<ContentReadResult> ReadAsync(
        ContentReadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using FileStream stream = File.OpenRead(request.SourcePath);
            JsonContentDocument? document = await JsonSerializer.DeserializeAsync<JsonContentDocument>(
                stream,
                ContentJson.Options,
                cancellationToken);
            return document is null
                ? ContentImportResult.Error("content.json.invalid", "The JSON content document is empty.")
                : ContentImportResult.CreateModel(
                    request,
                    document.SchemaVersion,
                    document.Entities ?? []);
        }
        catch (JsonException exception)
        {
            return ContentImportResult.Error("content.json.invalid", exception.Message);
        }
        catch (IOException exception)
        {
            return ContentImportResult.Error("content.source.unreadable", exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return ContentImportResult.Error("content.source.unreadable", exception.Message);
        }
        catch (ArgumentException exception)
        {
            return ContentImportResult.Error("content.json.invalid", exception.Message);
        }
    }
}

internal static class ContentJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

internal sealed class JsonContentDocument
{
    public int SchemaVersion { get; init; }
    public List<JsonContentEntity>? Entities { get; init; }
}

internal sealed class JsonContentEntity
{
    public string? ExternalId { get; init; }
    public string? KindId { get; init; }
    public Dictionary<string, string>? Values { get; init; }
    public List<JsonContentReference>? References { get; init; }
}

internal sealed class JsonContentReference
{
    public string? RelationId { get; init; }
    public string? TargetExternalId { get; init; }
    public bool Required { get; init; }
}

internal static class ContentImportResult
{
    public static ContentReadResult CreateModel(
        ContentReadRequest request,
        int schemaVersion,
        IEnumerable<JsonContentEntity> entities)
    {
        if (schemaVersion != request.ExpectedSchemaVersion)
        {
            return Error(
                "content.schema.mismatch",
                $"Expected schema {request.ExpectedSchemaVersion}, but found {schemaVersion}.");
        }

        NormalizedContentEntity[] normalizedEntities = entities.Select(entity =>
            new NormalizedContentEntity(
                entity.ExternalId!,
                entity.KindId!,
                entity.Values ?? [],
                (entity.References ?? []).Select(reference => new ContentReference(
                    reference.RelationId!,
                    reference.TargetExternalId!,
                    reference.Required)))).ToArray();

        return new ContentReadResult(
            new NormalizedContentModel(request.ProviderId, schemaVersion, normalizedEntities),
            []);
    }

    public static ContentReadResult Error(string code, string message) =>
        new(null, [new ContentDiagnostic(ContentDiagnosticSeverity.Error, code, message)]);
}
