using PCDynasty.Contracts.Careers;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class LineupPlanTests
{
    [Fact]
    public void PlanCopiesOrderedAssignmentsAndOptions()
    {
        var plan = new LineupPlan(
            "club:test",
            "contest:next",
            new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero),
            [Assignment("person:b", 1), Assignment("person:a", 0)],
            [KeyValuePair.Create("approach", "balanced")]);

        Assert.Equal(["person:a", "person:b"], plan.Assignments.Select(item => item.ParticipantId));
        Assert.Equal("balanced", plan.Options["approach"]);
    }

    [Fact]
    public void PlanRejectsDuplicatesEmptySelectionsAndInvalidOrder()
    {
        Assert.Throws<ArgumentException>(() => Plan([]));
        Assert.Throws<ArgumentException>(() => Plan([Assignment("person:a", 0), Assignment("person:a", 1)]));
        Assert.Throws<ArgumentException>(() => Plan([Assignment("person:a", 0), Assignment("person:b", 0)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LineupAssignment("person:a", "role:a", -1));
    }

    private static LineupPlan Plan(IEnumerable<LineupAssignment> assignments) =>
        new("club:test", "contest:next", DateTimeOffset.UtcNow, assignments, []);

    private static LineupAssignment Assignment(string participantId, int order) =>
        new(participantId, $"role:{order}", order);
}
