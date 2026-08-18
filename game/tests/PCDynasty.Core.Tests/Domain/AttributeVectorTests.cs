using PCDynasty.Core.Domain;
using Xunit;

namespace PCDynasty.Core.Tests.Domain;

public sealed class AttributeVectorTests
{
    [Fact]
    public void StoresValuesByNumericAttributeId()
    {
        var attributes = new AttributeVector([61, 83, 47]);

        Assert.Equal(61, attributes[new AttributeId(0)]);
        Assert.Equal(83, attributes[new AttributeId(1)]);
        Assert.Equal(47, attributes[new AttributeId(2)]);
    }

    [Fact]
    public void CopiesValuesAtTheInputBoundary()
    {
        var source = new[] { 61, 83 };
        var attributes = new AttributeVector(source);

        source[0] = 12;

        Assert.Equal(61, attributes[new AttributeId(0)]);
    }

    [Fact]
    public void RejectsNegativeNumericIdsAtTheInputBoundary()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AttributeId(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PersonId(0));
    }

    [Fact]
    public void RejectsAnAttributeIdOutsideTheVector()
    {
        var attributes = new AttributeVector([61]);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = attributes[new AttributeId(1)]);
    }
}
