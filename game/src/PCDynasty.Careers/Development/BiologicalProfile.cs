namespace PCDynasty.Careers.Development;

public sealed class BiologicalProfile
{
    public BiologicalProfile(
        int maturationDay,
        int peakDay,
        int declineDay,
        int resilience,
        int recoveryCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maturationDay);
        if (peakDay <= maturationDay)
        {
            throw new ArgumentOutOfRangeException(
                nameof(peakDay),
                "Biological peak must follow maturation.");
        }

        if (declineDay <= peakDay)
        {
            throw new ArgumentOutOfRangeException(
                nameof(declineDay),
                "Biological decline must follow peak.");
        }

        MaturationDay = maturationDay;
        PeakDay = peakDay;
        DeclineDay = declineDay;
        Resilience = DevelopmentScale.Validate(resilience, nameof(resilience));
        RecoveryCapacity = DevelopmentScale.Validate(
            recoveryCapacity,
            nameof(recoveryCapacity));
    }

    public int MaturationDay { get; }
    public int PeakDay { get; }
    public int DeclineDay { get; }
    public int Resilience { get; }
    public int RecoveryCapacity { get; }
}

public sealed class DevelopmentProfile
{
    public DevelopmentProfile(
        int talentFactor,
        int workFactor,
        int opportunityFactor,
        BiologicalProfile biology)
    {
        TalentFactor = DevelopmentScale.Validate(talentFactor, nameof(talentFactor));
        WorkFactor = DevelopmentScale.Validate(workFactor, nameof(workFactor));
        OpportunityFactor = DevelopmentScale.Validate(
            opportunityFactor,
            nameof(opportunityFactor));
        Biology = biology ?? throw new ArgumentNullException(nameof(biology));
    }

    public int TalentFactor { get; }
    public int WorkFactor { get; }
    public int OpportunityFactor { get; }
    public BiologicalProfile Biology { get; }
}

internal static class DevelopmentScale
{
    public const int Maximum = 1_000;

    public static int Validate(int value, string parameterName) =>
        value is >= 0 and <= Maximum
            ? value
            : throw new ArgumentOutOfRangeException(parameterName);
}
