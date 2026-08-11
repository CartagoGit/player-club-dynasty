using PCDynasty.Club.Reputation;
using PCDynasty.Club.Staff;
using Xunit;

namespace PCDynasty.Club.Tests;

public sealed class ClubEvolutionTests
{
    [Fact]
    public void ReusableStaffPoliciesProduceDomainSpecificEffects()
    {
        StaffContribution[] staff =
        [
            new("staff:1", expertise: 900, cohesion: 700, availability: 800),
            new("staff:2", expertise: 600, cohesion: 900, availability: 700),
        ];
        var expertisePolicy = new StaffEffectPolicy(
            "policy:expertise",
            expertiseWeight: 700,
            cohesionWeight: 200,
            availabilityWeight: 100);
        var cohesionPolicy = new StaffEffectPolicy(
            "policy:cohesion",
            expertiseWeight: 100,
            cohesionWeight: 800,
            availabilityWeight: 100);

        int expertiseEffect = StaffEffectResolver.Resolve(expertisePolicy, staff);
        int cohesionEffect = StaffEffectResolver.Resolve(cohesionPolicy, staff);

        Assert.InRange(expertiseEffect, 0, 1_000);
        Assert.InRange(cohesionEffect, 0, 1_000);
        Assert.NotEqual(expertiseEffect, cohesionEffect);
    }

    [Fact]
    public void PrestigeFanbaseAndStaffEvolveThroughExplicitPolicy()
    {
        var state = new ClubEvolutionState(
            "club:7",
            asOfDay: 10_000,
            prestige: 600,
            fanbase: 400);
        var policy = new ClubEvolutionPolicy(
            performanceWeight: 400,
            staffWeight: 300,
            communityWeight: 300,
            prestigeFanbaseWeight: 450,
            communityFanbaseWeight: 550);

        ClubEvolutionState strong = ClubEvolution.Advance(
            state,
            targetDay: 10_365,
            performance: 750,
            staffEffect: 800,
            communityEngagement: 700,
            policy);
        ClubEvolutionState weakStaff = ClubEvolution.Advance(
            state,
            targetDay: 10_365,
            performance: 750,
            staffEffect: 200,
            communityEngagement: 700,
            policy);

        Assert.True(strong.Prestige > state.Prestige);
        Assert.True(strong.Fanbase > state.Fanbase);
        Assert.True(strong.Prestige > weakStaff.Prestige);
        Assert.Equal(10_365, strong.AsOfDay);
        Assert.True(ClubEvolution.ResolveInfluence(
            strong,
            new ClubInfluencePolicy(prestigeWeight: 400, fanbaseWeight: 600)) > 0);
    }
}
