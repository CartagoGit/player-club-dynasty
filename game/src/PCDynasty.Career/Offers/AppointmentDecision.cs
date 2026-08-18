using System.Collections.Immutable;

namespace PCDynasty.Career.Offers;

public enum AppointmentDecisionKind
{
    Approved,
    InsufficientReputation,
    ContractConflict,
    OrganizationUnavailable,
}

public enum CareerOfferFactorKind
{
    Reputation,
    Vacancy,
    BoardConfidence,
    ResourceCapacity,
    CurrentContract,
}

public sealed record CareerOfferFactor(
    CareerOfferFactorKind Kind,
    bool Satisfied,
    int Contribution,
    string Explanation);

public sealed record AppointmentDecision(
    string OfferId,
    AppointmentDecisionKind Kind,
    int SuitabilityScore,
    ImmutableArray<CareerOfferFactor> Factors)
{
    public bool CanAppoint => Kind == AppointmentDecisionKind.Approved;
}

public sealed class OrganizationHiringState
{
    public OrganizationHiringState(
        bool hasVacancy,
        int boardConfidence,
        int resourceCapacity)
    {
        HasVacancy = hasVacancy;
        BoardConfidence = OfferValue.Score(boardConfidence, nameof(boardConfidence));
        ResourceCapacity = OfferValue.Score(resourceCapacity, nameof(resourceCapacity));
    }

    public bool HasVacancy { get; }

    public int BoardConfidence { get; }

    public int ResourceCapacity { get; }
}

public sealed class CareerOpportunity
{
    public CareerOpportunity(
        string offerId,
        string organizationId,
        string roleId,
        DateOnly effectiveOn,
        DateOnly contractThrough,
        int minimumReputation,
        int requiredResourceCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(
            effectiveOn,
            DateOnly.MinValue,
            nameof(effectiveOn));
        ArgumentOutOfRangeException.ThrowIfLessThan(
            contractThrough,
            effectiveOn,
            nameof(contractThrough));

        OfferId = OfferValue.Required(offerId, nameof(offerId));
        OrganizationId = OfferValue.Required(organizationId, nameof(organizationId));
        RoleId = OfferValue.Required(roleId, nameof(roleId));
        EffectiveOn = effectiveOn;
        ContractThrough = contractThrough;
        MinimumReputation = OfferValue.Score(minimumReputation, nameof(minimumReputation));
        RequiredResourceCapacity = OfferValue.Score(
            requiredResourceCapacity,
            nameof(requiredResourceCapacity));
    }

    public string OfferId { get; }

    public string OrganizationId { get; }

    public string RoleId { get; }

    public DateOnly EffectiveOn { get; }

    public DateOnly ContractThrough { get; }

    public int MinimumReputation { get; }

    public int RequiredResourceCapacity { get; }
}

internal static class OfferValue
{
    public static int Score(int value, string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return value;
    }

    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
