using System.Collections.Immutable;
using PCDynasty.Core.Domain;
using PCDynasty.Knowledge.Observations;
using PCDynasty.Knowledge.Scouting;
using PCDynasty.Knowledge.Staff;

namespace PCDynasty.Knowledge.Projections;

public sealed record ProjectionScenario(
    string ScenarioId,
    int MinimumEstimate,
    int MaximumEstimate,
    int ProbabilityPercent);

public sealed record ProjectionEvidence(
    string SourceId,
    DateOnly ObservedOn,
    int MinimumEstimate,
    int MaximumEstimate,
    int AgedConfidence);

public sealed record ProjectionExplanation(string Code, string Message);

public sealed class PlayerProjection
{
    private const double DaysPerYear = 365.2425;

    private PlayerProjection(
        PersonId subjectId,
        AttributeId attributeId,
        DateOnly basedOn,
        DateOnly horizon,
        int confidence,
        ImmutableArray<ProjectionScenario> scenarios,
        ImmutableArray<ProjectionEvidence> evidence,
        ImmutableArray<ProjectionExplanation> explanations)
    {
        SubjectId = subjectId;
        AttributeId = attributeId;
        BasedOn = basedOn;
        Horizon = horizon;
        Confidence = confidence;
        Scenarios = scenarios;
        Evidence = evidence;
        Explanations = explanations;
    }

    public PersonId SubjectId { get; }

    public AttributeId AttributeId { get; }

    public DateOnly BasedOn { get; }

    public DateOnly Horizon { get; }

    public int Confidence { get; }

    public ImmutableArray<ProjectionScenario> Scenarios { get; }

    public ImmutableArray<ProjectionEvidence> Evidence { get; }

    public ImmutableArray<ProjectionExplanation> Explanations { get; }

    public static PlayerProjection Create(
        FusedKnowledgeEstimate knowledge,
        ScoutingAssignment assignment,
        StaffKnowledgeProfile staff,
        DateOnly horizon)
    {
        ArgumentNullException.ThrowIfNull(knowledge);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentNullException.ThrowIfNull(staff);

        if (horizon <= knowledge.AsOf)
        {
            throw new ArgumentOutOfRangeException(
                nameof(horizon),
                horizon,
                "A future projection needs a horizon after its evidence date.");
        }

        if (!string.Equals(assignment.StaffId, staff.StaffId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The knowledge profile does not belong to the assigned staff member.",
                nameof(staff));
        }

        if (!assignment.Includes(knowledge.SubjectId))
        {
            throw new ArgumentException(
                "The projected subject is outside the scouting assignment.",
                nameof(assignment));
        }

        KnowledgeCapability capability = staff.Get(
            assignment.Domain.DomainId,
            assignment.Domain.RegionId);
        int horizonDays = horizon.DayNumber - knowledge.AsOf.DayNumber;
        int annualUncertainty = Math.Max(2, 12 - capability.Expertise / 10);
        int futureUncertainty = (int)Math.Ceiling(
            horizonDays * annualUncertainty / DaysPerYear);
        int center = (knowledge.MinimumEstimate + knowledge.MaximumEstimate) / 2;

        ImmutableArray<ProjectionScenario> scenarios =
        [
            new ProjectionScenario(
                "downside",
                Clamp(knowledge.MinimumEstimate - futureUncertainty),
                Clamp(center),
                20),
            new ProjectionScenario(
                "expected",
                Clamp(knowledge.MinimumEstimate - futureUncertainty / 2),
                Clamp(knowledge.MaximumEstimate + futureUncertainty / 2),
                60),
            new ProjectionScenario(
                "upside",
                Clamp(center),
                Clamp(knowledge.MaximumEstimate + futureUncertainty),
                20),
        ];

        double capabilityFactor =
            (capability.Expertise + capability.Confidence) / 200d;
        int horizonConfidenceLoss = (int)Math.Ceiling(
            horizonDays * 4 / DaysPerYear);
        int confidence = Math.Clamp(
            (int)Math.Round(knowledge.Confidence * capabilityFactor) -
            horizonConfidenceLoss,
            0,
            100);

        ImmutableArray<ProjectionEvidence> evidence = knowledge.Evidence
            .Select(item => new ProjectionEvidence(
                item.Observation.SourceId,
                item.Observation.ObservedOn,
                item.MinimumEstimate,
                item.MaximumEstimate,
                item.Confidence))
            .OrderBy(item => item.ObservedOn)
            .ThenBy(item => item.SourceId, StringComparer.Ordinal)
            .ToImmutableArray();

        ImmutableArray<ProjectionExplanation> explanations =
        [
            new ProjectionExplanation(
                "evidence",
                $"Projection combines {evidence.Length} available observations as of " +
                $"{knowledge.AsOf:yyyy-MM-dd}."),
            new ProjectionExplanation(
                "staff-capability",
                $"Domain expertise {capability.Expertise} and confidence " +
                $"{capability.Confidence} determine projection certainty."),
            new ProjectionExplanation(
                "horizon-uncertainty",
                $"The {horizonDays}-day horizon adds {futureUncertainty} points of uncertainty."),
        ];

        return new PlayerProjection(
            knowledge.SubjectId,
            knowledge.AttributeId,
            knowledge.AsOf,
            horizon,
            confidence,
            scenarios,
            evidence,
            explanations);
    }

    private static int Clamp(int score) => Math.Clamp(score, 0, 100);
}
