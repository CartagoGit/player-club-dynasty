using PCDynasty.Core.Domain;
using PCDynasty.Knowledge.Observations;
using PCDynasty.Knowledge.Projections;
using PCDynasty.Knowledge.Scouting;
using PCDynasty.Knowledge.Staff;
using Xunit;

namespace PCDynasty.Knowledge.Tests;

public sealed class PlayerProjectionTests
{
    private static readonly PersonId SubjectId = new(41);
    private static readonly AttributeId AttributeId = new(2);

    [Fact]
    public void BuildsAProbabilisticProjectionTraceableToItsEvidence()
    {
        FusedKnowledgeEstimate knowledge = CreateKnowledge();
        ScoutingAssignment assignment = CreateAssignment();
        StaffKnowledgeProfile staff = CreateStaff(expertise: 88, confidence: 84);
        var horizon = new DateOnly(2028, 1, 1);

        PlayerProjection projection = PlayerProjection.Create(
            knowledge,
            assignment,
            staff,
            horizon);

        Assert.Equal(SubjectId, projection.SubjectId);
        Assert.Equal(AttributeId, projection.AttributeId);
        Assert.Equal(horizon, projection.Horizon);
        Assert.Equal(100, projection.Scenarios.Sum(scenario => scenario.ProbabilityPercent));
        Assert.All(projection.Scenarios, scenario =>
            Assert.InRange(scenario.MinimumEstimate, 0, scenario.MaximumEstimate));
        Assert.Contains(projection.Evidence, evidence => evidence.SourceId == "scout:a");
        Assert.Contains(projection.Evidence, evidence => evidence.SourceId == "analyst:b");
        Assert.All(projection.Evidence, evidence =>
            Assert.True(evidence.ObservedOn <= knowledge.AsOf));
        Assert.NotEmpty(projection.Explanations);

        string[] publicPropertyNames = typeof(PlayerProjection)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain(publicPropertyNames, name =>
            name.Contains("Potential", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Ceiling", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Actual", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Exact", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("True", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StaffCapabilityChangesCertaintyButNeverInventsHiddenTruth()
    {
        FusedKnowledgeEstimate knowledge = CreateKnowledge();
        ScoutingAssignment assignment = CreateAssignment();
        var horizon = new DateOnly(2028, 1, 1);

        PlayerProjection expert = PlayerProjection.Create(
            knowledge,
            assignment,
            CreateStaff(expertise: 95, confidence: 92),
            horizon);
        PlayerProjection novice = PlayerProjection.Create(
            knowledge,
            assignment,
            CreateStaff(expertise: 25, confidence: 40),
            horizon);
        PlayerProjection repeated = PlayerProjection.Create(
            knowledge,
            assignment,
            CreateStaff(expertise: 95, confidence: 92),
            horizon);

        Assert.True(expert.Confidence > novice.Confidence);
        Assert.True(ProjectionWidth(expert) < ProjectionWidth(novice));
        Assert.Equal(expert.Scenarios.ToArray(), repeated.Scenarios.ToArray());
        Assert.Equal(expert.Evidence.ToArray(), repeated.Evidence.ToArray());
    }

    [Fact]
    public void AssignmentControlsStaffDomainSubjectsAndDates()
    {
        Assert.Throws<ArgumentException>(() => new ScoutingAssignment(
            "assignment:1",
            "staff:7",
            new KnowledgeDomain("technical", "iberia"),
            [SubjectId, SubjectId],
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 1)));

        ScoutingAssignment assignment = CreateAssignment();
        StaffKnowledgeProfile wrongStaff = new(
            "staff:other",
            [new KnowledgeCapability(new KnowledgeDomain("technical", "iberia"), 80, 80)]);

        Assert.Throws<ArgumentException>(() => PlayerProjection.Create(
            CreateKnowledge(),
            assignment,
            wrongStaff,
            new DateOnly(2028, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerProjection.Create(
            CreateKnowledge(),
            assignment,
            CreateStaff(80, 80),
            new DateOnly(2025, 12, 31)));
    }

    private static int ProjectionWidth(PlayerProjection projection) =>
        projection.Scenarios.Max(scenario => scenario.MaximumEstimate) -
        projection.Scenarios.Min(scenario => scenario.MinimumEstimate);

    private static FusedKnowledgeEstimate CreateKnowledge()
    {
        return KnowledgeFusion.Fuse(
            [
                new Observation(
                    SubjectId,
                    AttributeId,
                    60,
                    70,
                    82,
                    "scout:a",
                    new DateOnly(2025, 11, 1)),
                new Observation(
                    SubjectId,
                    AttributeId,
                    63,
                    72,
                    76,
                    "analyst:b",
                    new DateOnly(2025, 10, 1)),
            ],
            new DateOnly(2026, 1, 1));
    }

    private static ScoutingAssignment CreateAssignment() => new(
        "assignment:1",
        "staff:7",
        new KnowledgeDomain("technical", "iberia"),
        [SubjectId],
        new DateOnly(2025, 9, 1),
        new DateOnly(2026, 2, 1));

    private static StaffKnowledgeProfile CreateStaff(int expertise, int confidence) => new(
        "staff:7",
        [
            new KnowledgeCapability(
                new KnowledgeDomain("technical", "iberia"),
                expertise,
                confidence),
        ]);
}
