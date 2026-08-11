using PCDynasty.Careers.Development;
using Xunit;

namespace PCDynasty.Careers.Tests;

public sealed class BiologicalProfileTests
{
    [Fact]
    public void CombinesDevelopmentFactorsWithoutPublishingAFixedCeiling()
    {
        var biology = new BiologicalProfile(
            maturationDay: 6_200,
            peakDay: 9_100,
            declineDay: 11_000,
            resilience: 740,
            recoveryCapacity: 680);
        var profile = new DevelopmentProfile(
            talentFactor: 810,
            workFactor: 760,
            opportunityFactor: 530,
            biology);

        Assert.Equal(810, profile.TalentFactor);
        Assert.Equal(760, profile.WorkFactor);
        Assert.Equal(530, profile.OpportunityFactor);
        Assert.Same(biology, profile.Biology);
        Assert.DoesNotContain(
            typeof(DevelopmentProfile).GetProperties(),
            property => property.Name.Contains("Potential", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Ceiling", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RejectsInvalidBiologicalTimelinesAndUnscaledFactors()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BiologicalProfile(
            maturationDay: 9_100,
            peakDay: 9_100,
            declineDay: 11_000,
            resilience: 500,
            recoveryCapacity: 500));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DevelopmentProfile(
            talentFactor: 1_001,
            workFactor: 500,
            opportunityFactor: 500,
            new BiologicalProfile(6_000, 9_000, 11_000, 500, 500)));
    }
}
