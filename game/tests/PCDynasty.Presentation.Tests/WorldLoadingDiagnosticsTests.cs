using System.IO;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The runtime must load through the canonical pipeline. Audit a00001 found it
/// reading fixed files without validator or compiler, so a broken number
/// became a zero-rated player and a reference to a missing club was never
/// noticed. These tests exercise the productive path, not the library.
/// </summary>
public sealed class WorldLoadingDiagnosticsTests
{
    private static readonly string StarterWorld = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "starter-world"));

    [Fact]
    public async Task TheStarterWorldCompilesToAStableHash()
    {
        var first = new WorldService(new JsonContentReader(), StarterWorld);
        var second = new WorldService(new JsonContentReader(), StarterWorld);

        await first.ReloadAsync();
        await second.ReloadAsync();

        Assert.Equal(64, first.ContentHash.Length);
        Assert.Equal(first.ContentHash, second.ContentHash);
    }

    [Fact]
    public async Task AnUnparsableAttributeStopsTheLoadInsteadOfBecomingZero()
    {
        string root = CopyStarterWorld();
        try
        {
            Corrupt(root, "people.json", "\"technique\": \"62\"", "\"technique\": \"muy alto\"");
            var service = new WorldService(new JsonContentReader(), root);

            InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ReloadAsync());

            Assert.Contains("technique", failure.Message, StringComparison.Ordinal);
            Assert.Contains("person:alven-roake", failure.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AReferenceToAMissingEntityStopsTheLoad()
    {
        string root = CopyStarterWorld();
        try
        {
            Corrupt(root, "people.json", "\"targetExternalId\": \"club:aurora-quay\"", "\"targetExternalId\": \"club:nowhere\"");
            var service = new WorldService(new JsonContentReader(), root);

            InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ReloadAsync());

            Assert.Contains("club:nowhere", failure.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task QueryingBeforeLoadingIsRejected()
    {
        var service = new WorldService(new JsonContentReader(), StarterWorld);

        Assert.Throws<InvalidOperationException>(() => service.ListClubs());
        await Task.CompletedTask;
    }

    private static string CopyStarterWorld()
    {
        string root = Directory.CreateTempSubdirectory("pcdynasty-world-").FullName;
        Directory.CreateDirectory(Path.Combine(root, "data"));
        foreach (string file in Directory.EnumerateFiles(Path.Combine(StarterWorld, "data")))
        {
            File.Copy(file, Path.Combine(root, "data", Path.GetFileName(file)));
        }

        return root;
    }

    private static void Corrupt(string root, string document, string search, string replacement)
    {
        string path = Path.Combine(root, "data", document);
        string content = File.ReadAllText(path);
        Assert.Contains(search, content, StringComparison.Ordinal);
        File.WriteAllText(path, content.Replace(search, replacement, StringComparison.Ordinal));
    }
}
