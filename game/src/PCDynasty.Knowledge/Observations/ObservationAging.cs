using System.Collections.Immutable;

namespace PCDynasty.Knowledge.Observations;

public enum KnowledgeExplanationKind
{
    AgeDecay,
    ConflictingEvidence,
    CorroboratingEvidence,
}

public sealed record KnowledgeExplanation(
    KnowledgeExplanationKind Kind,
    string Message,
    string? SourceId = null);

public sealed record ObservationAgingPolicy
{
    public static ObservationAgingPolicy Default { get; } = new(
        confidenceLossPerYear: 8,
        uncertaintyGrowthPerYear: 2);

    public ObservationAgingPolicy(
        int confidenceLossPerYear,
        int uncertaintyGrowthPerYear)
    {
        if (confidenceLossPerYear is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(confidenceLossPerYear));
        }

        if (uncertaintyGrowthPerYear is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(uncertaintyGrowthPerYear));
        }

        ConfidenceLossPerYear = confidenceLossPerYear;
        UncertaintyGrowthPerYear = uncertaintyGrowthPerYear;
    }

    public int ConfidenceLossPerYear { get; }

    public int UncertaintyGrowthPerYear { get; }
}

public sealed record AgedObservation(
    Observation Observation,
    DateOnly AsOf,
    int AgeInDays,
    int MinimumEstimate,
    int MaximumEstimate,
    int Confidence,
    ImmutableArray<KnowledgeExplanation> Explanations);

public static class ObservationAging
{
    private const double DaysPerYear = 365.2425;

    public static AgedObservation Age(
        Observation observation,
        DateOnly asOf,
        ObservationAgingPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(observation);

        int ageInDays = asOf.DayNumber - observation.ObservedOn.DayNumber;
        if (ageInDays < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(asOf),
                asOf,
                "Knowledge cannot be evaluated before its observation date.");
        }

        ObservationAgingPolicy resolvedPolicy = policy ?? ObservationAgingPolicy.Default;
        int confidenceLoss = ProportionalLoss(
            ageInDays,
            resolvedPolicy.ConfidenceLossPerYear);
        int uncertaintyGrowth = ProportionalLoss(
            ageInDays,
            resolvedPolicy.UncertaintyGrowthPerYear);

        ImmutableArray<KnowledgeExplanation> explanations = ageInDays == 0
            ? []
            :
            [
                new KnowledgeExplanation(
                    KnowledgeExplanationKind.AgeDecay,
                    $"Evidence is {ageInDays} days old; confidence fell by {confidenceLoss} " +
                    $"and uncertainty grew by {uncertaintyGrowth} points.",
                    observation.SourceId),
            ];

        return new AgedObservation(
            observation,
            asOf,
            ageInDays,
            Math.Max(0, observation.MinimumEstimate - uncertaintyGrowth),
            Math.Min(100, observation.MaximumEstimate + uncertaintyGrowth),
            Math.Max(0, observation.Confidence - confidenceLoss),
            explanations);
    }

    private static int ProportionalLoss(int ageInDays, int annualRate)
    {
        if (ageInDays == 0 || annualRate == 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(ageInDays * annualRate / DaysPerYear);
    }
}
