using System.Collections.Immutable;
using PCDynasty.Simulation.Teams;

namespace PCDynasty.Simulation.Motivation;

public enum MotivationFactorKind
{
    Stakes,
    Rivalry,
    Expectations,
    HomeSupport,
    Momentum,
    Chemistry,
    Fatigue,
}

public sealed record MotivationFactor(
    MotivationFactorKind Kind,
    int MotivationContribution,
    int PressureContribution,
    string Explanation);

public sealed record MatchMotivation(
    string ContestId,
    int Motivation,
    int Pressure,
    ImmutableArray<MotivationFactor> Factors);

public static class MotivationResolver
{
    public static MatchMotivation Resolve(
        MatchContext context,
        TeamChemistry chemistry)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(chemistry);

        int chemistryScore = (chemistry.Cohesion + chemistry.Trust + chemistry.Familiarity) / 3;
        ImmutableArray<MotivationFactor> factors =
        [
            Factor(
                MotivationFactorKind.Stakes,
                context.Importance * 0.25,
                context.Importance * 0.45,
                "Contest importance raises both motivation and pressure."),
            Factor(
                MotivationFactorKind.Rivalry,
                context.Rivalry * 0.10,
                context.Rivalry * 0.15,
                "Rivalry adds emotional intensity."),
            Factor(
                MotivationFactorKind.Expectations,
                0,
                context.ExternalExpectation * 0.40,
                "External expectations increase contextual pressure."),
            Factor(
                MotivationFactorKind.HomeSupport,
                context.HomeSupport * 0.05,
                context.HomeSupport * -0.10,
                "Home support encourages the team and absorbs some pressure."),
            Factor(
                MotivationFactorKind.Momentum,
                (context.RecentMomentum - 50) * 0.10,
                0,
                "Recent momentum changes immediate belief."),
            Factor(
                MotivationFactorKind.Chemistry,
                (chemistryScore - 50) * 0.25,
                0,
                "Current team chemistry helps turn intent into collective motivation."),
            Factor(
                MotivationFactorKind.Fatigue,
                context.SquadFatigue * -0.25,
                context.SquadFatigue * 0.05,
                "Squad fatigue limits usable motivation and adds strain."),
        ];

        int motivation = Math.Clamp(
            40 + factors.Sum(factor => factor.MotivationContribution),
            0,
            100);
        int pressure = Math.Clamp(
            factors.Sum(factor => factor.PressureContribution),
            0,
            100);

        return new MatchMotivation(context.ContestId, motivation, pressure, factors);
    }

    private static MotivationFactor Factor(
        MotivationFactorKind kind,
        double motivationContribution,
        double pressureContribution,
        string explanation) =>
        new(
            kind,
            (int)Math.Round(motivationContribution),
            (int)Math.Round(pressureContribution),
            explanation);
}
