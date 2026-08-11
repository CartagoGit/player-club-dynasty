using PCDynasty.Core.Domain;
using PCDynasty.Football.Attributes;
using Xunit;

namespace PCDynasty.Football.Tests;

public sealed class FootballAttributeCatalogTests
{
    [Fact]
    public void CompilesACompleteContiguousCatalogue()
    {
        FootballAttributeDefinition[] attributes = FootballAttributeCatalog.All.ToArray();

        Assert.Equal(30, attributes.Length);
        Assert.Equal(
            Enumerable.Range(0, attributes.Length),
            attributes.Select(attribute => attribute.Id.Value));
        Assert.Equal(
            attributes.Length,
            attributes.Select(attribute => attribute.Key).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            Enum.GetValues<FootballAttributeGroup>().Order(),
            attributes.Select(attribute => attribute.Group).Distinct().Order());
    }

    [Fact]
    public void CreatesOneVectorContainingOutfieldAndGoalkeepingAttributes()
    {
        int[] values = Enumerable.Range(1, FootballAttributeCatalog.Count).ToArray();

        AttributeVector attributes = FootballAttributeCatalog.Compile(values);

        Assert.Equal(FootballAttributeCatalog.Count, attributes.Count);
        Assert.Equal(1, attributes[FootballAttributeCatalog.BallControl]);
        Assert.Equal(30, attributes[FootballAttributeCatalog.Distribution]);
    }

    [Theory]
    [InlineData(29)]
    [InlineData(31)]
    public void RejectsAnIncompleteOrExtendedVector(int valueCount)
    {
        var values = new int[valueCount];

        Assert.Throws<ArgumentException>(() => FootballAttributeCatalog.Compile(values));
    }

    [Fact]
    public void RejectsAnOutOfRangeCompiledAttribute()
    {
        var values = new int[FootballAttributeCatalog.Count];
        values[FootballAttributeCatalog.Finishing.Value] = 101;

        Assert.Throws<ArgumentOutOfRangeException>(() => FootballAttributeCatalog.Compile(values));
    }
}
