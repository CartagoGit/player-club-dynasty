using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;
using PCDynasty.Content.Worlds;
using Xunit;

namespace PCDynasty.Content.Integration.Tests;

/// <summary>
/// A world is identified by what it says, not by the file format it arrived
/// in. The same starter world exported to CSV and to SQLite must compile to
/// the same content hash as the shipped JSON, otherwise an external database
/// could never stand in for the integrated pack (audit a00001, stage 3).
/// </summary>
public sealed class EquivalentSourceHashTests
{
    private static readonly string ContentRoot =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "starter-world");

    private static readonly string[] DocumentNames =
    [
        "countries.json",
        "clubs.json",
        "people.json",
        "competitions.json",
        "rules.json",
    ];

    private static readonly string[] NumericKeys =
    [
        "prestige",
        "fanbase",
        "birth-year",
        "technique",
        "form",
        "physical",
        "mental",
        "reputation",
    ];

    [Fact]
    public async Task JsonCsvAndSqliteSourcesCompileToTheSameHash()
    {
        WorldPackLoadResult json = await LoadJsonAsync();
        Assert.True(json.IsSuccess, json.DescribeErrors());

        string workspace = Directory.CreateTempSubdirectory("pcdynasty-sources-").FullName;
        try
        {
            IReadOnlyList<NormalizedContentEntity> entities = json.Pack!.Entities.Values
                .OrderBy(entity => entity.ExternalId, StringComparer.Ordinal)
                .ToArray();

            string csvPath = Path.Combine(workspace, "world.csv");
            await File.WriteAllTextAsync(csvPath, BuildCsv(entities));
            WorldPackLoadResult csv = await new WorldPackLoader(new CsvContentReader())
                .LoadAsync(Source([csvPath]));

            string databasePath = Path.Combine(workspace, "world.sqlite");
            await BuildSqliteAsync(databasePath, entities);
            WorldPackLoadResult sqlite = await new WorldPackLoader(new SqliteContentReader())
                .LoadAsync(Source([databasePath]));

            Assert.True(csv.IsSuccess, csv.DescribeErrors());
            Assert.True(sqlite.IsSuccess, sqlite.DescribeErrors());
            Assert.Equal(json.ContentHash, csv.ContentHash);
            Assert.Equal(json.ContentHash, sqlite.ContentHash);
            Assert.Equal(json.Pack.Entities.Count, sqlite.Pack!.Entities.Count);
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    [Fact]
    public async Task AMutatedValueChangesTheHash()
    {
        WorldPackLoadResult json = await LoadJsonAsync();
        string workspace = Directory.CreateTempSubdirectory("pcdynasty-mutated-").FullName;
        try
        {
            NormalizedContentEntity[] mutated = json.Pack!.Entities.Values
                .OrderBy(entity => entity.ExternalId, StringComparer.Ordinal)
                .Select((entity, index) => index == 0 ? WithBumpedValue(entity) : entity)
                .ToArray();

            string csvPath = Path.Combine(workspace, "world.csv");
            await File.WriteAllTextAsync(csvPath, BuildCsv(mutated));
            WorldPackLoadResult csv = await new WorldPackLoader(new CsvContentReader())
                .LoadAsync(Source([csvPath]));

            Assert.True(csv.IsSuccess, csv.DescribeErrors());
            Assert.NotEqual(json.ContentHash, csv.ContentHash);
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    private static Task<WorldPackLoadResult> LoadJsonAsync() =>
        new WorldPackLoader(new JsonContentReader()).LoadAsync(
            Source(DocumentNames.Select(name => Path.Combine(ContentRoot, "data", name)).ToArray()));

    private static WorldPackSource Source(IReadOnlyList<string> documents) =>
        new("starter-world", 1, documents, NumericKeys);

    private static NormalizedContentEntity WithBumpedValue(NormalizedContentEntity entity)
    {
        KeyValuePair<string, string>[] values = entity.Values
            .Select(pair => pair.Key == "display-name"
                ? KeyValuePair.Create(pair.Key, pair.Value + " Reserves")
                : pair)
            .ToArray();
        return new NormalizedContentEntity(entity.ExternalId, entity.KindId, values, entity.References);
    }

    private static string BuildCsv(IReadOnlyList<NormalizedContentEntity> entities)
    {
        var csv = new StringBuilder("schema_version,external_id,kind_id,values_json,references_json\n");
        foreach (NormalizedContentEntity entity in entities)
        {
            string values = JsonSerializer.Serialize(
                entity.Values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
            string references = JsonSerializer.Serialize(entity.References
                .Select(reference => new
                {
                    relationId = reference.RelationId,
                    targetExternalId = reference.TargetExternalId,
                    required = reference.Required,
                })
                .ToArray());
            csv.Append("1,")
                .Append(Quote(entity.ExternalId)).Append(',')
                .Append(Quote(entity.KindId)).Append(',')
                .Append(Quote(values)).Append(',')
                .Append(Quote(references)).Append('\n');
        }

        return csv.ToString();
    }

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static async Task BuildSqliteAsync(
        string databasePath,
        IReadOnlyList<NormalizedContentEntity> entities)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        // One transaction for the whole world: a per-row commit turned a
        // thousand-entity fixture into minutes.
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        await ExecuteAsync(connection, """
            CREATE TABLE content_metadata (key TEXT PRIMARY KEY, value TEXT NOT NULL);
            CREATE TABLE content_entities (external_id TEXT PRIMARY KEY, kind_id TEXT NOT NULL);
            CREATE TABLE content_values (
                entity_external_id TEXT NOT NULL,
                value_id TEXT NOT NULL,
                value TEXT NOT NULL);
            CREATE TABLE content_references (
                source_external_id TEXT NOT NULL,
                relation_id TEXT NOT NULL,
                target_external_id TEXT NOT NULL,
                required INTEGER NOT NULL,
                ordinal INTEGER NOT NULL);
            INSERT INTO content_metadata (key, value) VALUES ('schema_version', '1');
            """);

        foreach (NormalizedContentEntity entity in entities)
        {
            await ExecuteAsync(
                connection,
                "INSERT INTO content_entities (external_id, kind_id) VALUES ($id, $kind);",
                ("$id", entity.ExternalId),
                ("$kind", entity.KindId));

            foreach ((string key, string value) in entity.Values)
            {
                await ExecuteAsync(
                    connection,
                    "INSERT INTO content_values (entity_external_id, value_id, value) VALUES ($id, $key, $value);",
                    ("$id", entity.ExternalId),
                    ("$key", key),
                    ("$value", value));
            }

            for (int ordinal = 0; ordinal < entity.References.Length; ordinal++)
            {
                ContentReference reference = entity.References[ordinal];
                await ExecuteAsync(
                    connection,
                    """
                    INSERT INTO content_references
                        (source_external_id, relation_id, target_external_id, required, ordinal)
                    VALUES ($id, $relation, $target, $required, $ordinal);
                    """,
                    ("$id", entity.ExternalId),
                    ("$relation", reference.RelationId),
                    ("$target", reference.TargetExternalId),
                    ("$required", reference.Required ? "1" : "0"),
                    ("$ordinal", ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        await transaction.CommitAsync();
    }

    private static async Task ExecuteAsync(
        SqliteConnection connection,
        string sql,
        params (string Name, string Value)[] parameters)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        foreach ((string name, string value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync();
    }
}
