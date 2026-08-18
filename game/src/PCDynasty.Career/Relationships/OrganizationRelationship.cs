namespace PCDynasty.Career.Relationships;

public enum AppointmentCause
{
    AcceptedOffer,
    Promotion,
    InterimAssignment,
    OwnershipTransition,
}

public enum DepartureCause
{
    Dismissal,
    Resignation,
    ContractExpiry,
    MutualAgreement,
    Retirement,
}

public sealed record AppointmentFact(
    DateOnly EffectiveOn,
    AppointmentCause Cause,
    string SourceId);

public sealed record DepartureFact(
    DateOnly EffectiveOn,
    DepartureCause Cause,
    string SourceId);

public sealed class OrganizationRelationship
{
    internal OrganizationRelationship(
        string relationshipId,
        string managerId,
        string organizationId,
        string roleId,
        DateOnly contractThrough,
        AppointmentFact appointment,
        DepartureFact? departure)
    {
        RelationshipId = relationshipId;
        ManagerId = managerId;
        OrganizationId = organizationId;
        RoleId = roleId;
        ContractThrough = contractThrough;
        Appointment = appointment;
        Departure = departure;
    }

    public string RelationshipId { get; }

    public string ManagerId { get; }

    public string OrganizationId { get; }

    public string RoleId { get; }

    public DateOnly ContractThrough { get; }

    public AppointmentFact Appointment { get; }

    public DepartureFact? Departure { get; }

    public bool IsActiveOn(DateOnly date) =>
        date >= Appointment.EffectiveOn &&
        (Departure is null || date < Departure.EffectiveOn);

    public bool IsUnderContractOn(DateOnly date) =>
        IsActiveOn(date) && date <= ContractThrough;

    internal OrganizationRelationship WithDeparture(DepartureFact departure) =>
        new(
            RelationshipId,
            ManagerId,
            OrganizationId,
            RoleId,
            ContractThrough,
            Appointment,
            departure);
}

internal static class RelationshipValue
{
    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }

    public static void RequiredDate(DateOnly value, string parameterName) =>
        ArgumentOutOfRangeException.ThrowIfEqual(
            value,
            DateOnly.MinValue,
            parameterName);
}
