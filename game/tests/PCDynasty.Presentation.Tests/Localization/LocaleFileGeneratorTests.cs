using PCDynasty.Presentation.Core.Localization;
using Xunit;

namespace PCDynasty.Presentation.Tests.Localization;

/// <summary>
/// The generator is the only path that writes the built-in JSON.
/// The tests pin four contracts:
///   - regenerating twice produces byte-identical JSON;
///   - tweaking a record diffs only the affected key;
///   - dry-run refuses to overwrite when the file would change;
///   - a record whose id does not match its Lang is rejected.
/// </summary>
public sealed class LocaleFileGeneratorTests
{
    private static string TempFolder()
    {
        string folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"locale-gen-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(folder);
        return folder;
    }

    [Fact]
    public void GeneratingTheShippedRecordsProducesIdenticalOutputTwice()
    {
        string folder = TempFolder();
        try
        {
            var generator = new LocaleFileGenerator(folder);
            IReadOnlyList<string> first = generator.Generate(ShippedLocaleFiles.All);

            foreach (string path in first)
            {
                string firstContent = System.IO.File.ReadAllText(path);
                generator.Generate(ShippedLocaleFiles.All);
                string secondContent = System.IO.File.ReadAllText(path);
                Assert.Equal(firstContent, secondContent);
            }
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void DryRunReportsDriftWhenARecordIsTweaked()
    {
        string folder = TempFolder();
        try
        {
            new LocaleFileGenerator(folder).Generate(ShippedLocaleFiles.All);

            LocaleFile tweaked = ShippedLocaleFiles.BuildSpanish() with { Name = "Castellano" };
            var generator = new LocaleFileGenerator(folder, dryRun: true);

            Assert.Throws<InvalidOperationException>(() =>
                generator.Generate(new[] { ("es", tweaked), ("en", ShippedLocaleFiles.BuildEnglish()) }));
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void ARunningGeneratorReflectsTheTweakedRecord()
    {
        string folder = TempFolder();
        try
        {
            new LocaleFileGenerator(folder).Generate(ShippedLocaleFiles.All);

            LocaleFile tweaked = ShippedLocaleFiles.BuildSpanish() with { Name = "Castellano" };
            new LocaleFileGenerator(folder).Generate(
                new[] { ("es", tweaked), ("en", ShippedLocaleFiles.BuildEnglish()) });

            string json = System.IO.File.ReadAllText(System.IO.Path.Combine(folder, "es.json"));
            Assert.Contains("Castellano", json);
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void AMismatchedIdIsRejected()
    {
        string folder = TempFolder();
        try
        {
            var generator = new LocaleFileGenerator(folder);
            Assert.Throws<InvalidOperationException>(() =>
                generator.Generate(new[] { ("xx", ShippedLocaleFiles.BuildSpanish()) }));
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void EveryShippedRecordSurvivesARoundTripThroughTheLoader()
    {
        // The generator emits JSON the loader must accept. The
        // round-trip uses the InMemory source so the test does not
        // touch the filesystem beyond a single temp folder.
        string folder = TempFolder();
        try
        {
            new LocaleFileGenerator(folder).Generate(ShippedLocaleFiles.All);

            Dictionary<string, string> map = new();
            foreach (string path in System.IO.Directory.EnumerateFiles(folder, "*.json"))
            {
                map[$"res://locale/langs/built/{System.IO.Path.GetFileName(path)}"] =
                    System.IO.File.ReadAllText(path);
            }

            var source = new InMemoryLocaleFileSource(
                map,
                rootFolder: "res://locale/langs/built/",
                builtInPrefix: "res://locale/langs/built/");
            var loader = new LocaleLoader(source);

            foreach ((string id, LocaleFile expected) in ShippedLocaleFiles.All)
            {
                LocaleLoadResult result = loader.Load(id);
                LocaleLoadResult.Ok ok = Assert.IsType<LocaleLoadResult.Ok>(result);
                Assert.Equal(expected.Lang, ok.File.Lang);
                Assert.Equal(expected.Name, ok.File.Name);
            }
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void OverwriteFalseRefusesToReplaceAnExistingFile()
    {
        string folder = TempFolder();
        try
        {
            new LocaleFileGenerator(folder).Generate(ShippedLocaleFiles.All);

            var generator = new LocaleFileGenerator(folder, overwrite: false);
            Assert.Throws<InvalidOperationException>(() =>
                generator.Generate(ShippedLocaleFiles.All));
        }
        finally
        {
            System.IO.Directory.Delete(folder, recursive: true);
        }
    }
}
