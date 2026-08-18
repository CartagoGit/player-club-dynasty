using System.IO;
using PCDynasty.Content.Importers;
using PCDynasty.Content.Worlds;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The bootstrap used to pin the starter directory and a JSON reader, so
/// "externally loadable content" was theoretical (audit a00001). A source is
/// now described, chosen and remembered.
/// </summary>
public sealed class ContentSourceSelectionTests
{
    private static readonly string StarterWorld = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    [Fact]
    public void ADirectoryIsReadAsJsonDocuments()
    {
        ContentSourceDescriptor source = ContentSourceDescriptor.External("/worlds/community");

        Assert.Equal("json", source.FormatId);
        Assert.False(source.IsIntegrated);
        Assert.Equal("community", source.DisplayName);
        Assert.Equal(ContentSourceDescriptor.DefaultDocumentNames.Length, source.DocumentPaths.Length);
    }

    [Theory]
    [InlineData("/worlds/world.csv", "csv")]
    [InlineData("/worlds/world.sqlite", "sqlite")]
    [InlineData("/worlds/world.db", "sqlite")]
    [InlineData("/worlds/world.json", "json")]
    public void AFileIsReadByItsExtension(string path, string expectedFormat)
    {
        ContentSourceDescriptor source = ContentSourceDescriptor.External(path);

        Assert.Equal(expectedFormat, source.FormatId);
        Assert.Single(source.DocumentPaths);
    }

    [Fact]
    public void AnUnsupportedExtensionIsRejected()
    {
        Assert.Throws<ArgumentException>(() => ContentSourceDescriptor.External("/worlds/world.xlsx"));
    }

    [Fact]
    public void EveryDescribedFormatHasAReader()
    {
        Assert.All(
            ContentReaderFactory.SupportedFormats,
            format => Assert.NotNull(ContentReaderFactory.Create(format)));
        Assert.Throws<ArgumentException>(() => ContentReaderFactory.Create("parquet"));
    }

    [Fact]
    public void TheChosenSourceSurvivesARestart()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-preference-").FullName;
        try
        {
            string file = Path.Combine(directory, "content-source.json");
            var first = new ContentSourcePreference(file);
            Assert.Null(first.TryLoadExternalPath());
            Assert.True(first.Resolve(StarterWorld).IsIntegrated);

            first.SaveExternalPath("/worlds/community");

            var afterRestart = new ContentSourcePreference(file);
            Assert.Equal("/worlds/community", afterRestart.TryLoadExternalPath());
            Assert.False(afterRestart.Resolve(StarterWorld).IsIntegrated);

            afterRestart.ClearExternalPath();
            Assert.True(new ContentSourcePreference(file).Resolve(StarterWorld).IsIntegrated);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ACorruptOrUnsupportedPreferenceFallsBackToTheIntegratedWorld()
    {
        string directory = Directory.CreateTempSubdirectory("pcdynasty-preference-").FullName;
        try
        {
            string file = Path.Combine(directory, "content-source.json");
            File.WriteAllText(file, "{ not json");
            Assert.True(new ContentSourcePreference(file).Resolve(StarterWorld).IsIntegrated);

            File.WriteAllText(file, "{ \"externalPath\": \"/worlds/world.xlsx\" }");
            Assert.True(new ContentSourcePreference(file).Resolve(StarterWorld).IsIntegrated);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task AnExternalDirectoryLoadsThroughTheSameServiceAsTheIntegratedWorld()
    {
        ContentSourceDescriptor external = ContentSourceDescriptor.External(StarterWorld);
        var service = new WorldService(ContentReaderFactory.Create(external.FormatId), external);

        await service.ReloadAsync();

        var integrated = new WorldService(new JsonContentReader(), StarterWorld);
        await integrated.ReloadAsync();

        Assert.False(service.Source.IsIntegrated);
        Assert.True(integrated.Source.IsIntegrated);
        Assert.Equal(integrated.ContentHash, service.ContentHash);
        Assert.Equal(integrated.ListClubs().Count, service.ListClubs().Count);
    }
}
