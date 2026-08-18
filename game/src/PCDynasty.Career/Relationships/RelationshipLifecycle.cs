namespace PCDynasty.Career.Relationships;

public static class RelationshipLifecycle
{
    public static OrganizationRelationship Appoint(
        string relationshipId,
        string managerId,
        string organizationId,
        string roleId,
        DateOnly effectiveOn,
        DateOnly contractThrough,
        AppointmentCause cause,
        string sourceId)
    {
        RelationshipValue.RequiredDate(effectiveOn, nameof(effectiveOn));
        if (contractThrough < effectiveOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contractThrough),
                contractThrough,
                "A contract cannot end before the appointment begins.");
        }

        if (!Enum.IsDefined(cause))
        {
            throw new ArgumentOutOfRangeException(nameof(cause));
        }

        return new OrganizationRelationship(
            RelationshipValue.Required(relationshipId, nameof(relationshipId)),
            RelationshipValue.Required(managerId, nameof(managerId)),
            RelationshipValue.Required(organizationId, nameof(organizationId)),
            RelationshipValue.Required(roleId, nameof(roleId)),
            contractThrough,
            new AppointmentFact(
                effectiveOn,
                cause,
                RelationshipValue.Required(sourceId, nameof(sourceId))),
            departure: null);
    }

    public static OrganizationRelationship Depart(
        OrganizationRelationship current,
        DateOnly effectiveOn,
        DepartureCause cause,
        string sourceId)
    {
        ArgumentNullException.ThrowIfNull(current);
        RelationshipValue.RequiredDate(effectiveOn, nameof(effectiveOn));
        if (current.Departure is not null)
        {
            throw new InvalidOperationException(
                "An organization relationship already has a departure fact.");
        }

        if (effectiveOn < current.Appointment.EffectiveOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(effectiveOn),
                effectiveOn,
                "A departure cannot precede its appointment.");
        }

        if (!Enum.IsDefined(cause))
        {
            throw new ArgumentOutOfRangeException(nameof(cause));
        }

        return current.WithDeparture(new DepartureFact(
            effectiveOn,
            cause,
            RelationshipValue.Required(sourceId, nameof(sourceId))));
    }
}
