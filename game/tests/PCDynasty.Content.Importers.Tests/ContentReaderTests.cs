using PCDynasty.Content.Abstractions;
using PCDynasty.Content.Importers;
using Xunit;

namespace PCDynasty.Content.Importers.Tests;

public sealed class ContentReaderTests
{
    [Fact]
    public async Task JsonAndCsvProduceTheSameNormalizedModel()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string jsonPath = Path.Combine(directory, "community.json");
            string csvPath = Path.Combine(directory, "community.csv");
            await File.WriteAllTextAsync(jsonPath, JsonContent);
            await File.WriteAllTextAsync(csvPath, CsvContent);
            var request = new ContentReadRequest(jsonPath, "provider:community", 3);

            ContentReadResult json = await new JsonContentReader().ReadAsync(request);
            ContentReadResult csv = await new CsvContentReader().ReadAsync(
                new ContentReadRequest(csvPath, request.ProviderId, request.ExpectedSchemaVersion));

            Assert.True(json.IsSuccess);
            Assert.True(csv.IsSuccess);
            AssertModelsEqual(json.Model!, csv.Model!);
            Assert.Empty(json.Diagnostics);
            Assert.Empty(csv.Diagnostics);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ReadersReportInvalidExternalContentWithoutLeakingParserExceptions()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string jsonPath = Path.Combine(directory, "invalid.json");
            string csvPath = Path.Combine(directory, "invalid.csv");
            await File.WriteAllTextAsync(jsonPath, "{ not-json }");
            await File.WriteAllTextAsync(csvPath, "wrong,header\nvalue");

            ContentReadResult json = await new JsonContentReader().ReadAsync(
                new ContentReadRequest(jsonPath, "provider:community", 3));
            ContentReadResult csv = await new CsvContentReader().ReadAsync(
                new ContentReadRequest(csvPath, "provider:community", 3));

            Assert.False(json.IsSuccess);
            Assert.False(csv.IsSuccess);
            Assert.Contains(json.Diagnostics, diagnostic =>
                diagnostic.Severity == ContentDiagnosticSeverity.Error &&
                diagnostic.Code == "content.json.invalid");
            Assert.Contains(csv.Diagnostics, diagnostic =>
                diagnostic.Severity == ContentDiagnosticSeverity.Error &&
                diagnostic.Code == "content.csv.invalid");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"pcdynasty-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void AssertModelsEqual(
        NormalizedContentModel expected,
        NormalizedContentModel actual)
    {
        Assert.Equal(expected.ProviderId, actual.ProviderId);
        Assert.Equal(expected.SchemaVersion, actual.SchemaVersion);
        Assert.Equal(expected.Entities.Length, actual.Entities.Length);
        for (int index = 0; index < expected.Entities.Length; index++)
        {
            NormalizedContentEntity expectedEntity = expected.Entities[index];
            NormalizedContentEntity actualEntity = actual.Entities[index];
            Assert.Equal(expectedEntity.ExternalId, actualEntity.ExternalId);
            Assert.Equal(expectedEntity.KindId, actualEntity.KindId);
            Assert.Equal(expectedEntity.Values, actualEntity.Values);
            Assert.Equal(expectedEntity.References.Length, actualEntity.References.Length);
            for (int referenceIndex = 0; referenceIndex < expectedEntity.References.Length; referenceIndex++)
            {
                ContentReference expectedReference = expectedEntity.References[referenceIndex];
                ContentReference actualReference = actualEntity.References[referenceIndex];
                Assert.Equal(expectedReference.RelationId, actualReference.RelationId);
                Assert.Equal(expectedReference.TargetExternalId, actualReference.TargetExternalId);
                Assert.Equal(expectedReference.Required, actualReference.Required);
            }
        }
    }

    private const string JsonContent = """
        {
          "schemaVersion": 3,
          "entities": [
            {
              "externalId": "club:atlantico",
              "kindId": "entity:club",
              "values": { "name": "Atlántico CF", "reputation": "74" },
              "references": [
                {
                  "relationId": "relation:country",
                  "targetExternalId": "country:monteluz",
                  "required": true
                }
              ]
            },
            {
              "externalId": "country:monteluz",
              "kindId": "entity:country",
              "values": { "name": "Monteluz" },
              "references": []
            }
          ]
        }
        """;

    private const string CsvContent = """
        schema_version,external_id,kind_id,values_json,references_json
        3,club:atlantico,entity:club,"{""name"":""Atlántico CF"",""reputation"":""74""}","[{""relationId"":""relation:country"",""targetExternalId"":""country:monteluz"",""required"":true}]"
        3,country:monteluz,entity:country,"{""name"":""Monteluz""}",[]
        """;
}
