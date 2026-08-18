using PCDynasty.Core.Domain;
using PCDynasty.Knowledge.Observations;
using Xunit;

namespace PCDynasty.Knowledge.Tests;

public sealed class KnowledgeFusionTests
{
    private static readonly PersonId SubjectId = new(41);
    private static readonly AttributeId AttributeId = new(2);

    [Fact]
    public void AgesAndFusesConflictingEvidenceWithAnExplanation()
    {
        var oldOptimisticReport = CreateObservation(
            minimum: 72,
            maximum: 80,
            confidence: 90,
            source: "scout:north:4",
            observedOn: new DateOnly(2020, 1, 1));
        var newerConservativeReport = CreateObservation(
            minimum: 32,
            maximum: 40,
            confidence: 80,
            source: "analyst:video:9",
            observedOn: new DateOnly(2024, 1, 1));
        var asOf = new DateOnly(2026, 1, 1);

        AgedObservation aged = ObservationAging.Age(oldOptimisticReport, asOf);
        FusedKnowledgeEstimate fused = KnowledgeFusion.Fuse(
            [oldOptimisticReport, newerConservativeReport],
            asOf);

        Assert.True(aged.Confidence < oldOptimisticReport.Confidence);
        Assert.True(aged.MinimumEstimate < oldOptimisticReport.MinimumEstimate);
        Assert.True(aged.MaximumEstimate > oldOptimisticReport.MaximumEstimate);
        Assert.Contains(
            aged.Explanations,
            explanation => explanation.Kind == KnowledgeExplanationKind.AgeDecay);

        Assert.Equal(2, fused.Evidence.Length);
        Assert.True(fused.MinimumEstimate <= newerConservativeReport.MaximumEstimate);
        Assert.True(fused.MaximumEstimate >= oldOptimisticReport.MinimumEstimate);
        Assert.True(fused.Confidence < fused.Evidence.Max(item => item.Confidence));
        Assert.Contains(
            fused.Explanations,
            explanation => explanation.Kind == KnowledgeExplanationKind.ConflictingEvidence);
        Assert.All(fused.Evidence, evidence => Assert.Equal(asOf, evidence.AsOf));
        Assert.Contains(
            fused.Evidence,
            evidence => evidence.Observation.SourceId == oldOptimisticReport.SourceId);
    }

    [Fact]
    public void ConsistentRecentEvidenceIsMoreCertainThanOldConflict()
    {
        var asOf = new DateOnly(2026, 1, 1);
        FusedKnowledgeEstimate consistent = KnowledgeFusion.Fuse(
            [
                CreateObservation(61, 69, 82, "scout:a", new DateOnly(2025, 12, 1)),
                CreateObservation(63, 70, 78, "scout:b", new DateOnly(2025, 11, 1)),
            ],
            asOf);
        FusedKnowledgeEstimate conflicting = KnowledgeFusion.Fuse(
            [
                CreateObservation(72, 80, 90, "scout:old", new DateOnly(2020, 1, 1)),
                CreateObservation(32, 40, 80, "analyst:old", new DateOnly(2020, 1, 1)),
            ],
            asOf);

        Assert.True(consistent.Confidence > conflicting.Confidence);
        Assert.True(
            consistent.MaximumEstimate - consistent.MinimumEstimate <
            conflicting.MaximumEstimate - conflicting.MinimumEstimate);
        Assert.Contains(
            consistent.Explanations,
            explanation => explanation.Kind == KnowledgeExplanationKind.CorroboratingEvidence);
    }

    [Fact]
    public void RejectsFutureOrMixedSubjectEvidence()
    {
        Observation observation = CreateObservation(
            61,
            69,
            82,
            "scout:a",
            new DateOnly(2026, 2, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ObservationAging.Age(observation, new DateOnly(2026, 1, 1)));

        var anotherSubject = new Observation(
            new PersonId(99),
            AttributeId,
            63,
            70,
            78,
            "scout:b",
            new DateOnly(2025, 11, 1));

        Assert.Throws<ArgumentException>(() => KnowledgeFusion.Fuse(
            [CreateObservation(61, 69, 82, "scout:a", new DateOnly(2025, 12, 1)), anotherSubject],
            new DateOnly(2026, 1, 1)));
    }

    private static Observation CreateObservation(
        int minimum,
        int maximum,
        int confidence,
        string source,
        DateOnly observedOn)
    {
        return new Observation(
            SubjectId,
            AttributeId,
            minimum,
            maximum,
            confidence,
            source,
            observedOn);
    }
}
