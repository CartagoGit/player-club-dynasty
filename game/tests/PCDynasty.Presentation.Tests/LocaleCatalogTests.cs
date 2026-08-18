using PCDynasty.Presentation.Core;
using Xunit;

namespace PCDynasty.Presentation.Tests;

/// <summary>
/// The shipped catalog must expose Spanish and English with the right display
/// metadata, and the geometry must describe a non-empty list of operations
/// that fits the 0..1 normalised coordinate space.
/// </summary>
public sealed class LocaleCatalogTests
{
    [Fact]
    public void TheShippedCatalogIncludesSpanishAndEnglish()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (LocaleEntry entry in catalog.Locales)
        {
            ids.Add(entry.Id.Value);
        }

        Assert.Contains("es", ids);
        Assert.Contains("en", ids);
    }

    [Fact]
    public void TheDefaultLocaleIsSpanish()
    {
        Assert.Equal(LocaleId.Spanish, LocaleCatalog.Shipped.Default.Id);
    }

    [Fact]
    public void ResolvingAnUnknownIdReturnsTheDefault()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        Assert.Equal(catalog.Default.Id, catalog.Resolve(new LocaleId("fr")).Id);
    }

    [Fact]
    public void ResolvingARealIdReturnsTheEntry()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        LocaleEntry english = catalog.Resolve("en");

        Assert.Equal("en", english.Id.Value);
        Assert.Equal("en", english.GodotLocaleCode);
    }

    [Fact]
    public void EveryShippedLocaleCarriesAFlag()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        foreach (LocaleEntry entry in catalog.Locales)
        {
            Assert.NotEmpty(entry.Flag.Operations);
        }
    }

    [Fact]
    public void EveryFlagOperationStaysWithinTheZeroToOneSpace()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        foreach (LocaleEntry entry in catalog.Locales)
        {
            foreach (FlagOperation operation in entry.Flag.Operations)
            {
                Assert.InRange(operation.X, 0f, 1f);
                Assert.InRange(operation.Y, 0f, 1f);
                Assert.InRange(operation.Width, 0f, 1f);
                Assert.InRange(operation.Height, 0f, 1f);
                Assert.InRange(operation.EndX, 0f, 1f);
                Assert.InRange(operation.EndY, 0f, 1f);
            }
        }
    }

    [Fact]
    public void ConstructingAnEmptyCatalogIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new LocaleCatalog(Array.Empty<LocaleEntry>()));
    }

    [Fact]
    public void ConstructingACatalogWithDuplicateIdsIsRejected()
    {
        LocaleEntry entry = LocaleCatalog.Shipped.Default;

        Assert.Throws<ArgumentException>(() => new LocaleCatalog([entry, entry]));
    }

    [Fact]
    public void EveryEntryHasANonEmptyGodotLocaleCode()
    {
        LocaleCatalog catalog = LocaleCatalog.Shipped;

        foreach (LocaleEntry entry in catalog.Locales)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.GodotLocaleCode));
        }
    }
}
