using System.Globalization;
using System.Text;
using System.Text.Json;
using PCDynasty.Content.Abstractions;

namespace PCDynasty.Content.Importers;

public sealed class CsvContentReader : IContentReader
{
    private static readonly string[] ExpectedHeader =
    [
        "schema_version",
        "external_id",
        "kind_id",
        "values_json",
        "references_json",
    ];

    public string FormatId => "csv";

    public async ValueTask<ContentReadResult> ReadAsync(
        ContentReadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            string content = await File.ReadAllTextAsync(request.SourcePath, cancellationToken);
            IReadOnlyList<string[]> rows = CsvParser.Parse(content);
            if (rows.Count == 0 || !rows[0].SequenceEqual(ExpectedHeader, StringComparer.Ordinal))
            {
                return ContentImportResult.Error(
                    "content.csv.invalid",
                    $"The CSV header must be: {string.Join(',', ExpectedHeader)}.");
            }

            var entities = new List<JsonContentEntity>(Math.Max(0, rows.Count - 1));
            int? schemaVersion = null;
            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];
                if (row.Length != ExpectedHeader.Length)
                {
                    throw new FormatException($"CSV row {rowIndex + 1} has {row.Length} columns instead of {ExpectedHeader.Length}.");
                }

                if (!int.TryParse(row[0], NumberStyles.None, CultureInfo.InvariantCulture, out int rowSchemaVersion))
                {
                    throw new FormatException($"CSV row {rowIndex + 1} has an invalid schema version.");
                }

                schemaVersion ??= rowSchemaVersion;
                if (schemaVersion != rowSchemaVersion)
                {
                    throw new FormatException("Every CSV row must use the same schema version.");
                }

                entities.Add(new JsonContentEntity
                {
                    ExternalId = row[1],
                    KindId = row[2],
                    Values = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        row[3],
                        ContentJson.Options),
                    References = JsonSerializer.Deserialize<List<JsonContentReference>>(
                        row[4],
                        ContentJson.Options),
                });
            }

            return ContentImportResult.CreateModel(
                request,
                schemaVersion ?? request.ExpectedSchemaVersion,
                entities);
        }
        catch (JsonException exception)
        {
            return ContentImportResult.Error("content.csv.invalid", exception.Message);
        }
        catch (FormatException exception)
        {
            return ContentImportResult.Error("content.csv.invalid", exception.Message);
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
            return ContentImportResult.Error("content.csv.invalid", exception.Message);
        }
    }
}

internal static class CsvParser
{
    public static IReadOnlyList<string[]> Parse(string content)
    {
        var rows = new List<string[]>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        bool hasInput = false;

        for (int index = 0; index < content.Length; index++)
        {
            char character = content[index];
            hasInput = true;
            if (quoted)
            {
                if (character == '"')
                {
                    if (index + 1 < content.Length && content[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            switch (character)
            {
                case '"' when field.Length == 0:
                    quoted = true;
                    break;
                case '"':
                    throw new FormatException("A quote may only begin an empty CSV field.");
                case ',':
                    AddField(row, field);
                    break;
                case '\r':
                case '\n':
                    AddField(row, field);
                    rows.Add(row.ToArray());
                    row.Clear();
                    if (character == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                    {
                        index++;
                    }

                    hasInput = false;
                    break;
                default:
                    field.Append(character);
                    break;
            }
        }

        if (quoted)
        {
            throw new FormatException("A quoted CSV field was not closed.");
        }

        if (hasInput || row.Count > 0 || field.Length > 0)
        {
            AddField(row, field);
            rows.Add(row.ToArray());
        }

        return rows;
    }

    private static void AddField(List<string> row, StringBuilder field)
    {
        row.Add(field.ToString());
        field.Clear();
    }
}
