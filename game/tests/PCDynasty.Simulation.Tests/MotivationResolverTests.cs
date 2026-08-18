using PCDynasty.Simulation.Motivation;
using PCDynasty.Simulation.Teams;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class MotivationResolverTests
{
    [Fact]
    public void HighStakesCreateContextualMotivationAndPressure()
    {
        TeamChemistry chemistry = TeamChemistry.Create("team:7", 72, 68, 75);
        var context = new MatchContext(
            "contest:final",
            importance: 95,
            rivalry: 70,
            externalExpectation: 85,
            homeSupport: 60,
            recentMomentum: 55,
            squadFatigue: 25);

        MatchMotivation result = MotivationResolver.Resolve(context, chemistry);

        Assert.True(result.Motivation > 60);
        Assert.True(result.Pressure > 50);
        Assert.Contains(result.Factors, factor => factor.Kind == MotivationFactorKind.Stakes);
        Assert.Contains(result.Factors, factor => factor.Kind == MotivationFactorKind.Chemistry);
    }

    [Fact]
    public void AContextChangeChangesTheResultWithoutMutatingTeamState()
    {
        TeamChemistry chemistry = TeamChemistry.Create("team:7", 65, 65, 65);
        var routine = new MatchContext("contest:1", 30, 10, 35, 50, 50, 20);
        var decisive = new MatchContext("contest:2", 95, 80, 90, 50, 50, 20);

        MatchMotivation routineResult = MotivationResolver.Resolve(routine, chemistry);
        MatchMotivation decisiveResult = MotivationResolver.Resolve(decisive, chemistry);

        Assert.True(decisiveResult.Motivation > routineResult.Motivation);
        Assert.True(decisiveResult.Pressure > routineResult.Pressure);
        Assert.Equal(65, chemistry.Cohesion);
        Assert.DoesNotContain(
            typeof(MatchMotivation).GetProperties(),
            property => property.Name.Contains("Permanent", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Player", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FatigueLimitsMotivationAndAllScoresAreBounded()
    {
        TeamChemistry chemistry = TeamChemistry.Create("team:7", 90, 90, 90);
        MatchMotivation fresh = MotivationResolver.Resolve(
            new MatchContext("contest:1", 70, 40, 50, 60, 70, 10),
            chemistry);
        MatchMotivation tired = MotivationResolver.Resolve(
            new MatchContext("contest:1", 70, 40, 50, 60, 70, 90),
            chemistry);

        Assert.True(fresh.Motivation > tired.Motivation);
        Assert.InRange(tired.Motivation, 0, 100);
        Assert.InRange(tired.Pressure, 0, 100);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MatchContext("contest:bad", 101, 0, 0, 0, 0, 0));
    }
}
