using System.Globalization;
using Microsoft.Data.Sqlite;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Importers;

public sealed class SqliteContentReader : IContentReader
{
    public string FormatId => "sqlite";

    public async ValueTask<ContentReadResult> ReadAsync(
        ContentReadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = request.SourcePath,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Private,
            }.ToString();
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            int schemaVersion = await ReadSchemaVersionAsync(connection, cancellationToken);
            Dictionary<string, JsonContentEntity> entities = await ReadEntitiesAsync(
                connection,
                cancellationToken);
            await ReadValuesAsync(connection, entities, cancellationToken);
            await ReadReferencesAsync(connection, entities, cancellationToken);

            return ContentImportResult.CreateModel(
                request,
                schemaVersion,
                entities.Values);
        }
        catch (SqliteException exception)
        {
            return ContentImportResult.Error("content.sqlite.invalid", exception.Message);
        }
        catch (FormatException exception)
        {
            return ContentImportResult.Error("content.sqlite.invalid", exception.Message);
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
            return ContentImportResult.Error("content.sqlite.invalid", exception.Message);
        }
    }

    private static async Task<int> ReadSchemaVersionAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT value
            FROM content_metadata
            WHERE key = 'schema_version';
            """;
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is not string text ||
            !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int schemaVersion) ||
            schemaVersion <= 0)
        {
            throw new FormatException("The database must contain a positive schema_version metadata value.");
        }

        return schemaVersion;
    }

    private static async Task<Dictionary<string, JsonContentEntity>> ReadEntitiesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var entities = new Dictionary<string, JsonContentEntity>(StringComparer.Ordinal);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT external_id, kind_id
            FROM content_entities
            ORDER BY external_id;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string externalId = reader.GetString(0);
            if (!entities.TryAdd(externalId, new JsonContentEntity
                {
                    ExternalId = externalId,
                    KindId = reader.GetString(1),
                    Values = new Dictionary<string, string>(StringComparer.Ordinal),
                    References = [],
                }))
            {
                throw new FormatException($"Entity '{externalId}' appears more than once.");
            }
        }

        return entities;
    }

    private static async Task ReadValuesAsync(
        SqliteConnection connection,
        Dictionary<string, JsonContentEntity> entities,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT entity_external_id, value_id, value
            FROM content_values
            ORDER BY entity_external_id, value_id;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string externalId = reader.GetString(0);
            if (!entities.TryGetValue(externalId, out JsonContentEntity? entity))
            {
                throw new FormatException($"Value targets unknown entity '{externalId}'.");
            }

            string valueId = reader.GetString(1);
            if (!entity.Values!.TryAdd(valueId, reader.GetString(2)))
            {
                throw new FormatException($"Entity '{externalId}' repeats value '{valueId}'.");
            }
        }
    }

    private static async Task ReadReferencesAsync(
        SqliteConnection connection,
        Dictionary<string, JsonContentEntity> entities,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT source_external_id, relation_id, target_external_id, required
            FROM content_references
            ORDER BY source_external_id, ordinal;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string externalId = reader.GetString(0);
            if (!entities.TryGetValue(externalId, out JsonContentEntity? entity))
            {
                throw new FormatException($"Reference belongs to unknown entity '{externalId}'.");
            }

            long requiredValue = reader.GetInt64(3);
            bool required = requiredValue switch
            {
                0 => false,
                1 => true,
                _ => throw new FormatException(
                    $"Reference on entity '{externalId}' has an invalid required flag."),
            };
            entity.References!.Add(new JsonContentReference
            {
                RelationId = reader.GetString(1),
                TargetExternalId = reader.GetString(2),
                Required = required,
            });
        }
    }
}
