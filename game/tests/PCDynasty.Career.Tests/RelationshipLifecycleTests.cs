using PCDynasty.Career.Relationships;
using Xunit;

namespace PCDynasty.Career.Tests;

public sealed class RelationshipLifecycleTests
{
    [Fact]
    public void AppointmentRetainsCauseDatesAndContract()
    {
        OrganizationRelationship relationship = RelationshipLifecycle.Appoint(
            relationshipId: "relationship:7:club-4:1",
            managerId: "manager:7",
            organizationId: "club:4",
            roleId: "role:first-team-manager",
            effectiveOn: new DateOnly(2025, 7, 1),
            contractThrough: new DateOnly(2027, 6, 30),
            cause: AppointmentCause.AcceptedOffer,
            sourceId: "offer:18");

        Assert.Equal(AppointmentCause.AcceptedOffer, relationship.Appointment.Cause);
        Assert.Equal(new DateOnly(2025, 7, 1), relationship.Appointment.EffectiveOn);
        Assert.Equal(new DateOnly(2027, 6, 30), relationship.ContractThrough);
        Assert.True(relationship.IsActiveOn(new DateOnly(2026, 1, 1)));
        Assert.Null(relationship.Departure);
    }

    [Fact]
    public void DepartureIsAnExplicitImmutableLifecycleFact()
    {
        OrganizationRelationship active = CreateActiveRelationship();

        OrganizationRelationship departed = RelationshipLifecycle.Depart(
            active,
            effectiveOn: new DateOnly(2026, 5, 30),
            cause: DepartureCause.MutualAgreement,
            sourceId: "agreement:9");

        Assert.Null(active.Departure);
        Assert.NotNull(departed.Departure);
        Assert.Equal(DepartureCause.MutualAgreement, departed.Departure.Cause);
        Assert.Equal(new DateOnly(2026, 5, 30), departed.Departure.EffectiveOn);
        Assert.Equal("agreement:9", departed.Departure.SourceId);
        Assert.True(departed.IsActiveOn(new DateOnly(2026, 5, 29)));
        Assert.False(departed.IsActiveOn(new DateOnly(2026, 5, 30)));
    }

    [Fact]
    public void RejectsImpossibleOrAmbiguousLifecycleTransitions()
    {
        OrganizationRelationship active = CreateActiveRelationship();
        Assert.Throws<ArgumentOutOfRangeException>(() => RelationshipLifecycle.Depart(
            active,
            new DateOnly(2025, 6, 30),
            DepartureCause.Dismissal,
            "board:1"));

        OrganizationRelationship departed = RelationshipLifecycle.Depart(
            active,
            new DateOnly(2026, 5, 30),
            DepartureCause.Dismissal,
            "board:1");
        Assert.Throws<InvalidOperationException>(() => RelationshipLifecycle.Depart(
            departed,
            new DateOnly(2026, 6, 1),
            DepartureCause.Resignation,
            "manager:notice"));

        Assert.Throws<ArgumentOutOfRangeException>(() => RelationshipLifecycle.Appoint(
            "relationship:bad",
            "manager:7",
            "club:4",
            "role:manager",
            new DateOnly(2025, 7, 1),
            new DateOnly(2025, 6, 30),
            AppointmentCause.AcceptedOffer,
            "offer:bad"));
    }

    private static OrganizationRelationship CreateActiveRelationship() =>
        RelationshipLifecycle.Appoint(
            "relationship:7:club-4:1",
            "manager:7",
            "club:4",
            "role:first-team-manager",
            new DateOnly(2025, 7, 1),
            new DateOnly(2027, 6, 30),
            AppointmentCause.AcceptedOffer,
            "offer:18");
}
