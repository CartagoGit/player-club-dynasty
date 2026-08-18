namespace PCDynasty.Club.Staff;

public sealed class StaffContribution
{
    public StaffContribution(
        string staffId,
        int expertise,
        int cohesion,
        int availability)
    {
        StaffId = EffectValue.Required(staffId, nameof(staffId));
        Expertise = EffectValue.Score(expertise, nameof(expertise));
        Cohesion = EffectValue.Score(cohesion, nameof(cohesion));
        Availability = EffectValue.Score(availability, nameof(availability));
    }

    public string StaffId { get; }
    public int Expertise { get; }
    public int Cohesion { get; }
    public int Availability { get; }
}

public sealed class StaffEffectPolicy
{
    public StaffEffectPolicy(
        string policyId,
        int expertiseWeight,
        int cohesionWeight,
        int availabilityWeight)
    {
        PolicyId = EffectValue.Required(policyId, nameof(policyId));
        ExpertiseWeight = EffectValue.Score(expertiseWeight, nameof(expertiseWeight));
        CohesionWeight = EffectValue.Score(cohesionWeight, nameof(cohesionWeight));
        AvailabilityWeight = EffectValue.Score(availabilityWeight, nameof(availabilityWeight));
        TotalWeight = ExpertiseWeight + CohesionWeight + AvailabilityWeight;
        ArgumentOutOfRangeException.ThrowIfZero(TotalWeight);
    }

    public string PolicyId { get; }
    public int ExpertiseWeight { get; }
    public int CohesionWeight { get; }
    public int AvailabilityWeight { get; }
    internal int TotalWeight { get; }
}

public static class StaffEffectResolver
{
    public static int Resolve(
        StaffEffectPolicy policy,
        IEnumerable<StaffContribution> contributions)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(contributions);
        StaffContribution[] snapshot = contributions.Select(contribution =>
        {
            ArgumentNullException.ThrowIfNull(contribution);
            return contribution;
        }).ToArray();
        if (snapshot.Length == 0)
        {
            return 0;
        }

        long total = snapshot.Sum(contribution =>
            (((long)contribution.Expertise * policy.ExpertiseWeight) +
             ((long)contribution.Cohesion * policy.CohesionWeight) +
             ((long)contribution.Availability * policy.AvailabilityWeight)) /
            policy.TotalWeight);
        return (int)(total / snapshot.Length);
    }
}

internal static class EffectValue
{
    public const int Maximum = 1_000;

    public static int Score(int value, string parameterName) =>
        value is >= 0 and <= Maximum
            ? value
            : throw new ArgumentOutOfRangeException(parameterName);

    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
