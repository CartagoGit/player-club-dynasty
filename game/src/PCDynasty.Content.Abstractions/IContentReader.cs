namespace PCDynasty.Content.Abstractions;

public interface IContentReader
{
    string FormatId { get; }

    ValueTask<ContentReadResult> ReadAsync(
        ContentReadRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class ContentReadRequest
{
    public ContentReadRequest(string sourcePath, string providerId, int expectedSchemaVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedSchemaVersion);

        SourcePath = ContentValue.NormalizeRequired(sourcePath, nameof(sourcePath));
        ProviderId = ContentValue.NormalizeIdentifier(providerId, nameof(providerId));
        ExpectedSchemaVersion = expectedSchemaVersion;
    }

    public string SourcePath { get; }
    public string ProviderId { get; }
    public int ExpectedSchemaVersion { get; }
}
