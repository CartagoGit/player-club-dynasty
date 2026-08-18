using System.Diagnostics;
using Microsoft.Data.Sqlite;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;
using Xunit;

namespace PCDynasty.Content.Importers.Tests;

public sealed class SqliteContentReaderTests
{
    private const int EntityCount = 10_000;

    [Fact]
    public async Task ImportsALargeExternalDatabaseThroughTheContentReaderContract()
    {
        string databasePath = Path.Combine(
            Path.GetTempPath(),
            $"pcdynasty-content-{Guid.NewGuid():N}.db");

        try
        {
            await CreateLargeDatabaseAsync(databasePath);
            var reader = new SqliteContentReader();
            Assert.IsAssignableFrom<IContentReader>(reader);
            var stopwatch = Stopwatch.StartNew();

            ContentReadResult result = await reader.ReadAsync(
                new ContentReadRequest(databasePath, "provider:large-test", 3));

            stopwatch.Stop();
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Diagnostics);
            NormalizedContentModel model = Assert.IsType<NormalizedContentModel>(result.Model);
            Assert.Equal(EntityCount, model.Entities.Length);
            Assert.Equal("entity:00000", model.Entities[0].ExternalId);
            Assert.Equal("Entity 00000", model.Entities[0].Values["name"]);
            Assert.Equal("entity:09999", model.Entities[^1].ExternalId);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(5),
                $"Import took {stopwatch.Elapsed.TotalMilliseconds:F0} ms.");
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task ReportsInvalidDatabaseSchemasAsDiagnostics()
    {
        string databasePath = Path.Combine(
            Path.GetTempPath(),
            $"pcdynasty-invalid-content-{Guid.NewGuid():N}.db");

        try
        {
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
            }

            ContentReadResult result = await new SqliteContentReader().ReadAsync(
                new ContentReadRequest(databasePath, "provider:invalid", 3));

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Diagnostics, diagnostic =>
                diagnostic.Severity == ContentDiagnosticSeverity.Error &&
                diagnostic.Code == "content.sqlite.invalid");
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static async Task CreateLargeDatabaseAsync(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await using (SqliteCommand schema = connection.CreateCommand())
        {
            schema.CommandText = """
                CREATE TABLE content_metadata (
                    key TEXT PRIMARY KEY,
                    value TEXT NOT NULL
                );
                CREATE TABLE content_entities (
                    external_id TEXT PRIMARY KEY,
                    kind_id TEXT NOT NULL
                );
                CREATE TABLE content_values (
                    entity_external_id TEXT NOT NULL,
                    value_id TEXT NOT NULL,
                    value TEXT NOT NULL,
                    PRIMARY KEY (entity_external_id, value_id)
                );
                CREATE TABLE content_references (
                    source_external_id TEXT NOT NULL,
                    ordinal INTEGER NOT NULL,
                    relation_id TEXT NOT NULL,
                    target_external_id TEXT NOT NULL,
                    required INTEGER NOT NULL,
                    PRIMARY KEY (source_external_id, ordinal)
                );
                INSERT INTO content_metadata (key, value) VALUES ('schema_version', '3');
                """;
            await schema.ExecuteNonQueryAsync();
        }

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();
        await using SqliteCommand entity = connection.CreateCommand();
        entity.Transaction = transaction;
        entity.CommandText = """
            INSERT INTO content_entities (external_id, kind_id)
            VALUES ($external_id, 'entity:test');
            """;
        SqliteParameter entityId = entity.Parameters.Add("$external_id", SqliteType.Text);
        entity.Prepare();

        await using SqliteCommand value = connection.CreateCommand();
        value.Transaction = transaction;
        value.CommandText = """
            INSERT INTO content_values (entity_external_id, value_id, value)
            VALUES ($external_id, 'name', $value);
            """;
        SqliteParameter valueEntityId = value.Parameters.Add("$external_id", SqliteType.Text);
        SqliteParameter valueText = value.Parameters.Add("$value", SqliteType.Text);
        value.Prepare();

        for (int index = 0; index < EntityCount; index++)
        {
            string externalId = $"entity:{index:D5}";
            entityId.Value = externalId;
            await entity.ExecuteNonQueryAsync();
            valueEntityId.Value = externalId;
            valueText.Value = $"Entity {index:D5}";
            await value.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}
