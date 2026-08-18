using PCDynasty.Career.Manager;
using PCDynasty.Career.Offers;
using PCDynasty.Career.Relationships;
using Xunit;

namespace PCDynasty.Career.Tests;

public sealed class CareerOfferResolverTests
{
    [Fact]
    public void ApprovesAReproducibleOfferWhenAllConstraintsPass()
    {
        ManagerCareer career = CreateCareer(reputationChange: 25);
        CareerOpportunity opportunity = CreateOpportunity(minimumReputation: 65);
        var organization = new OrganizationHiringState(
            hasVacancy: true,
            boardConfidence: 75,
            resourceCapacity: 80);

        AppointmentDecision first = CareerOfferResolver.Resolve(
            career,
            opportunity,
            organization,
            currentRelationship: null,
            contractReleaseAgreed: false);
        AppointmentDecision repeated = CareerOfferResolver.Resolve(
            career,
            opportunity,
            organization,
            currentRelationship: null,
            contractReleaseAgreed: false);

        Assert.True(first.CanAppoint);
        Assert.Equal(AppointmentDecisionKind.Approved, first.Kind);
        Assert.True(first.SuitabilityScore > 0);
        Assert.Equal(first.Kind, repeated.Kind);
        Assert.Equal(first.SuitabilityScore, repeated.SuitabilityScore);
        Assert.Equal(first.Factors.ToArray(), repeated.Factors.ToArray());
    }

    [Fact]
    public void RejectsInsufficientReputationOrUnavailableOrganization()
    {
        ManagerCareer career = CreateCareer(reputationChange: 0);
        CareerOpportunity demanding = CreateOpportunity(minimumReputation: 80);

        AppointmentDecision reputationRejected = CareerOfferResolver.Resolve(
            career,
            demanding,
            new OrganizationHiringState(true, 80, 80),
            null,
            false);
        AppointmentDecision vacancyRejected = CareerOfferResolver.Resolve(
            CreateCareer(reputationChange: 40),
            CreateOpportunity(minimumReputation: 60),
            new OrganizationHiringState(false, 80, 80),
            null,
            false);
        AppointmentDecision resourcesRejected = CareerOfferResolver.Resolve(
            CreateCareer(reputationChange: 40),
            CreateOpportunity(minimumReputation: 60, requiredResourceCapacity: 70),
            new OrganizationHiringState(true, 80, 40),
            null,
            false);

        Assert.Equal(AppointmentDecisionKind.InsufficientReputation, reputationRejected.Kind);
        Assert.Equal(AppointmentDecisionKind.OrganizationUnavailable, vacancyRejected.Kind);
        Assert.Equal(AppointmentDecisionKind.OrganizationUnavailable, resourcesRejected.Kind);
    }

    [Fact]
    public void ActiveContractRequiresAnExplicitReleaseAgreement()
    {
        ManagerCareer career = CreateCareer(reputationChange: 40);
        CareerOpportunity opportunity = CreateOpportunity(minimumReputation: 60);
        OrganizationRelationship current = RelationshipLifecycle.Appoint(
            "relationship:current",
            "manager:7",
            "club:current",
            "role:manager",
            new DateOnly(2025, 7, 1),
            new DateOnly(2028, 6, 30),
            AppointmentCause.AcceptedOffer,
            "offer:current");
        var organization = new OrganizationHiringState(true, 80, 80);

        AppointmentDecision blocked = CareerOfferResolver.Resolve(
            career,
            opportunity,
            organization,
            current,
            contractReleaseAgreed: false);
        AppointmentDecision released = CareerOfferResolver.Resolve(
            career,
            opportunity,
            organization,
            current,
            contractReleaseAgreed: true);

        Assert.Equal(AppointmentDecisionKind.ContractConflict, blocked.Kind);
        Assert.False(blocked.CanAppoint);
        Assert.Equal(AppointmentDecisionKind.Approved, released.Kind);
    }

    private static ManagerCareer CreateCareer(int reputationChange) =>
        ManagerCareer.Project(
            "manager:7",
            50,
            [
                new ManagerCareerFact(
                    "manager:7",
                    1,
                    new DateOnly(2026, 5, 30),
                    ManagerCareerFactKind.ObjectiveOutcome,
                    reputationChange,
                    "season:1",
                    "Season outcome."),
            ]);

    private static CareerOpportunity CreateOpportunity(
        int minimumReputation,
        int requiredResourceCapacity = 50) =>
        new(
            "offer:next",
            "club:next",
            "role:manager",
            new DateOnly(2026, 7, 1),
            new DateOnly(2029, 6, 30),
            minimumReputation,
            requiredResourceCapacity);
}
