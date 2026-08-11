using PCDynasty.Core.Domain;
using PCDynasty.Football.Attributes;
using PCDynasty.Football.Positions;
using Xunit;

namespace PCDynasty.Football.Tests;

public sealed class PositionFamiliarityTests
{
    [Fact]
    public void StoresExperienceAndPreferenceWithoutOwningAttributes()
    {
        var familiarity = new PositionFamiliarity(
            "football:position:left-back",
            familiarity: 98,
            experience: 94,
            preference: 91);

        Assert.Equal("football:position:left-back", familiarity.PositionId);
        Assert.Equal(98, familiarity.Familiarity);
        Assert.Equal(94, familiarity.Experience);
        Assert.Equal(91, familiarity.Preference);
        Assert.DoesNotContain(
            typeof(PositionFamiliarity).GetProperties(),
            property => property.PropertyType == typeof(AttributeVector) ||
                property.PropertyType == typeof(AttributeId));
    }

    [Fact]
    public void OrdersTheCompleteCatalogueByRoleRelevance()
    {
        AttributeRelevance[] relevance = FootballAttributeCatalog.All
            .Select(attribute => new AttributeRelevance(
                attribute.Id,
                FootballAttributeCatalog.Count - attribute.Id.Value))
            .Reverse()
            .ToArray();

        var role = new RoleRelevance("football:role:playmaker", relevance);

        Assert.Equal("football:role:playmaker", role.RoleId);
        Assert.Equal(FootballAttributeCatalog.Count, role.OrderedAttributes.Length);
        Assert.Equal(FootballAttributeCatalog.BallControl, role.OrderedAttributes[0].AttributeId);
        Assert.Equal(FootballAttributeCatalog.Distribution, role.OrderedAttributes[^1].AttributeId);
    }

    [Fact]
    public void RejectsARelevanceMapThatDoesNotCoverTheExactCatalogue()
    {
        AttributeRelevance[] incomplete = FootballAttributeCatalog.All[..^1]
            .Select(attribute => new AttributeRelevance(attribute.Id, 50))
            .ToArray();
        AttributeRelevance[] duplicate = FootballAttributeCatalog.All
            .Select(attribute => new AttributeRelevance(attribute.Id, 50))
            .ToArray();
        duplicate[^1] = new AttributeRelevance(FootballAttributeCatalog.BallControl, 50);

        Assert.Throws<ArgumentException>(() => new RoleRelevance("football:role:test", incomplete));
        Assert.Throws<ArgumentException>(() => new RoleRelevance("football:role:test", duplicate));
    }

    [Fact]
    public void RejectsInvalidFamiliarityAndRelevanceScores()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PositionFamiliarity("football:position:left-back", 101, 50, 50));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AttributeRelevance(FootballAttributeCatalog.BallControl, -1));
    }
}
