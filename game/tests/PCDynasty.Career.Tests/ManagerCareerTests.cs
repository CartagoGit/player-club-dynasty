using PCDynasty.Career.Manager;
using Xunit;

namespace PCDynasty.Career.Tests;

public sealed class ManagerCareerTests
{
    [Fact]
    public void ProjectsReputationAndTimelineFromDurableFacts()
    {
        ManagerCareerFact[] facts =
        [
            new(
                "manager:7",
                2,
                new DateOnly(2026, 5, 30),
                ManagerCareerFactKind.ObjectiveOutcome,
                reputationChange: 8,
                "season:2025-26",
                "Exceeded the agreed competitive objective."),
            new(
                "manager:7",
                1,
                new DateOnly(2025, 7, 1),
                ManagerCareerFactKind.Appointment,
                reputationChange: 3,
                "appointment:club-4",
                "Accepted a first-team appointment."),
            new(
                "manager:7",
                3,
                new DateOnly(2026, 6, 2),
                ManagerCareerFactKind.Achievement,
                reputationChange: 12,
                "achievement:cup",
                "Won a regional cup."),
        ];

        ManagerCareer career = ManagerCareer.Project(
            "manager:7",
            initialReputation: 50,
            facts);

        Assert.Equal("manager:7", career.ManagerId);
        Assert.Equal(73, career.Reputation);
        Assert.Equal([1L, 2L, 3L], career.History.Select(fact => fact.Sequence));
        Assert.Equal("achievement:cup", career.History[^1].SourceId);
    }

    [Fact]
    public void ReprojectionIsDeterministicAndReputationIsBounded()
    {
        ManagerCareerFact[] facts =
        [
            new(
                "manager:7",
                1,
                new DateOnly(2025, 7, 1),
                ManagerCareerFactKind.Conduct,
                -90,
                "discipline:1",
                "Serious breach of professional conduct."),
        ];

        ManagerCareer first = ManagerCareer.Project("manager:7", 20, facts);
        ManagerCareer repeated = ManagerCareer.Project("manager:7", 20, facts);

        Assert.Equal(0, first.Reputation);
        Assert.Equal(first.Reputation, repeated.Reputation);
        Assert.Equal(first.History.ToArray(), repeated.History.ToArray());
    }

    [Fact]
    public void RejectsAmbiguousOrForeignFacts()
    {
        var fact = new ManagerCareerFact(
            "manager:7",
            1,
            new DateOnly(2025, 7, 1),
            ManagerCareerFactKind.Appointment,
            3,
            "appointment:club-4",
            "Accepted an appointment.");
        var foreign = new ManagerCareerFact(
            "manager:8",
            2,
            new DateOnly(2025, 7, 2),
            ManagerCareerFactKind.Appointment,
            3,
            "appointment:club-5",
            "Accepted an appointment.");

        Assert.Throws<ArgumentException>(() =>
            ManagerCareer.Project("manager:7", 50, [fact, fact]));
        Assert.Throws<ArgumentException>(() =>
            ManagerCareer.Project("manager:7", 50, [fact, foreign]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ManagerCareer.Project("manager:7", 101, []));
    }
}
