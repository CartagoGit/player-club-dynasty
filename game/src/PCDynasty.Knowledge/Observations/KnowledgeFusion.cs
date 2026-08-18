using System.Collections.Immutable;
using PCDynasty.Core.Domain;

namespace PCDynasty.Knowledge.Observations;

public sealed record FusedKnowledgeEstimate(
    PersonId SubjectId,
    AttributeId AttributeId,
    DateOnly AsOf,
    int MinimumEstimate,
    int MaximumEstimate,
    int Confidence,
    ImmutableArray<AgedObservation> Evidence,
    ImmutableArray<KnowledgeExplanation> Explanations);

public static class KnowledgeFusion
{
    public static FusedKnowledgeEstimate Fuse(
        IEnumerable<Observation> observations,
        DateOnly asOf,
        ObservationAgingPolicy? agingPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(observations);

        ImmutableArray<Observation> evidence = observations.ToImmutableArray();
        if (evidence.IsEmpty)
        {
            throw new ArgumentException(
                "At least one observation is required to estimate knowledge.",
                nameof(observations));
        }

        Observation first = evidence[0];
        if (evidence.Any(item =>
                item.SubjectId != first.SubjectId ||
                item.AttributeId != first.AttributeId))
        {
            throw new ArgumentException(
                "Every observation must describe the same subject and attribute.",
                nameof(observations));
        }

        ImmutableArray<AgedObservation> aged = evidence
            .Select(item => ObservationAging.Age(item, asOf, agingPolicy))
            .ToImmutableArray();

        double totalWeight = aged.Sum(item => Math.Max(1, item.Confidence));
        double weightedCenter = aged.Sum(item =>
            Center(item) * Math.Max(1, item.Confidence)) / totalWeight;
        double weightedHalfWidth = aged.Sum(item =>
            HalfWidth(item) * Math.Max(1, item.Confidence)) / totalWeight;

        double minimumCenter = aged.Min(Center);
        double maximumCenter = aged.Max(Center);
        double disagreement = maximumCenter - minimumCenter;
        double averageWidth = aged.Average(item =>
            item.MaximumEstimate - item.MinimumEstimate);
        bool conflicting = aged.Length > 1 && disagreement > Math.Max(5, averageWidth);

        double fusedHalfWidth = weightedHalfWidth + disagreement / 2;
        int minimum = Math.Clamp(
            (int)Math.Floor(weightedCenter - fusedHalfWidth),
            0,
            100);
        int maximum = Math.Clamp(
            (int)Math.Ceiling(weightedCenter + fusedHalfWidth),
            0,
            100);

        int baseConfidence = (int)Math.Round(aged.Average(item => item.Confidence));
        int agreementAdjustment = conflicting
            ? -(int)Math.Ceiling(disagreement / 2)
            : Math.Min(10, Math.Max(0, aged.Length - 1) * 4);
        int confidence = Math.Clamp(baseConfidence + agreementAdjustment, 0, 100);

        var explanations = ImmutableArray.CreateBuilder<KnowledgeExplanation>();
        foreach (AgedObservation item in aged)
        {
            explanations.AddRange(item.Explanations);
        }

        explanations.Add(conflicting
            ? new KnowledgeExplanation(
                KnowledgeExplanationKind.ConflictingEvidence,
                $"Evidence centers differ by {disagreement:0.#} points; confidence was reduced.")
            : new KnowledgeExplanation(
                KnowledgeExplanationKind.CorroboratingEvidence,
                "Available evidence is mutually consistent; confidence was reinforced."));

        return new FusedKnowledgeEstimate(
            first.SubjectId,
            first.AttributeId,
            asOf,
            minimum,
            maximum,
            confidence,
            aged,
            explanations.ToImmutable());
    }

    private static double Center(AgedObservation observation) =>
        (observation.MinimumEstimate + observation.MaximumEstimate) / 2d;

    private static double HalfWidth(AgedObservation observation) =>
        (observation.MaximumEstimate - observation.MinimumEstimate) / 2d;
}
