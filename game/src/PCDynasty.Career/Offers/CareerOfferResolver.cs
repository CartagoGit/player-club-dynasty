using System.Collections.Immutable;
using PCDynasty.Career.Manager;
using PCDynasty.Career.Relationships;

namespace PCDynasty.Career.Offers;

public static class CareerOfferResolver
{
    private const int MinimumBoardConfidence = 40;

    public static AppointmentDecision Resolve(
        ManagerCareer career,
        CareerOpportunity opportunity,
        OrganizationHiringState organization,
        OrganizationRelationship? currentRelationship,
        bool contractReleaseAgreed)
    {
        ArgumentNullException.ThrowIfNull(career);
        ArgumentNullException.ThrowIfNull(opportunity);
        ArgumentNullException.ThrowIfNull(organization);

        if (currentRelationship is not null && !string.Equals(
                currentRelationship.ManagerId,
                career.ManagerId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The current relationship belongs to a different manager.",
                nameof(currentRelationship));
        }

        bool reputationSatisfied = career.Reputation >= opportunity.MinimumReputation;
        bool vacancySatisfied = organization.HasVacancy;
        bool boardSatisfied = organization.BoardConfidence >= MinimumBoardConfidence;
        bool resourcesSatisfied =
            organization.ResourceCapacity >= opportunity.RequiredResourceCapacity;
        bool underContract = currentRelationship?.IsUnderContractOn(
            opportunity.EffectiveOn) == true;
        bool contractSatisfied = !underContract || contractReleaseAgreed;

        ImmutableArray<CareerOfferFactor> factors =
        [
            new(
                CareerOfferFactorKind.Reputation,
                reputationSatisfied,
                career.Reputation - opportunity.MinimumReputation,
                reputationSatisfied
                    ? "Manager reputation meets the role requirement."
                    : "Manager reputation is below the role requirement."),
            new(
                CareerOfferFactorKind.Vacancy,
                vacancySatisfied,
                vacancySatisfied ? 10 : -100,
                vacancySatisfied
                    ? "The organization has an open appointment."
                    : "The organization has no open appointment."),
            new(
                CareerOfferFactorKind.BoardConfidence,
                boardSatisfied,
                organization.BoardConfidence - MinimumBoardConfidence,
                boardSatisfied
                    ? "Board confidence supports an appointment."
                    : "Board confidence is too low to authorize an appointment."),
            new(
                CareerOfferFactorKind.ResourceCapacity,
                resourcesSatisfied,
                organization.ResourceCapacity - opportunity.RequiredResourceCapacity,
                resourcesSatisfied
                    ? "The organization can support the proposed appointment."
                    : "The organization lacks the required appointment resources."),
            new(
                CareerOfferFactorKind.CurrentContract,
                contractSatisfied,
                contractSatisfied ? 0 : -100,
                contractSatisfied
                    ? "No unresolved current contract blocks the appointment."
                    : "A current contract needs an explicit release agreement."),
        ];

        AppointmentDecisionKind kind = !vacancySatisfied ||
            !boardSatisfied ||
            !resourcesSatisfied
            ? AppointmentDecisionKind.OrganizationUnavailable
            : !reputationSatisfied
                ? AppointmentDecisionKind.InsufficientReputation
                : !contractSatisfied
                    ? AppointmentDecisionKind.ContractConflict
                    : AppointmentDecisionKind.Approved;

        int suitability = kind == AppointmentDecisionKind.Approved
            ? Math.Clamp(
                50 +
                (career.Reputation - opportunity.MinimumReputation) * 2 +
                (organization.BoardConfidence - MinimumBoardConfidence) / 2 +
                (organization.ResourceCapacity - opportunity.RequiredResourceCapacity) / 2,
                0,
                100)
            : 0;

        return new AppointmentDecision(
            opportunity.OfferId,
            kind,
            suitability,
            factors);
    }
}
