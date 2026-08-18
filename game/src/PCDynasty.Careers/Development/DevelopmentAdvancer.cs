namespace PCDynasty.Careers.Development;

public sealed class DevelopmentState
{
    public DevelopmentState(
        int asOfDay,
        int capability,
        CareerWear wear)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(asOfDay);
        AsOfDay = asOfDay;
        Capability = DevelopmentScale.Validate(capability, nameof(capability));
        Wear = wear ?? throw new ArgumentNullException(nameof(wear));
        if (wear.AsOfDay != asOfDay)
        {
            throw new ArgumentException(
                "Development and wear checkpoints must describe the same day.",
                nameof(wear));
        }
    }

    public int AsOfDay { get; }
    public int Capability { get; }
    public CareerWear Wear { get; }
}

public static class DevelopmentAdvancer
{
    private const long GrowthDenominator = 10_950_000L;
    private const long DeclineDenominator = 3_650L;

    public static DevelopmentState Advance(
        DevelopmentProfile profile,
        DevelopmentState current,
        int targetDay,
        int exposurePerDay)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentOutOfRangeException.ThrowIfNegative(exposurePerDay);
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, current.AsOfDay);

        if (targetDay == current.AsOfDay)
        {
            return current;
        }

        BiologicalProfile biology = profile.Biology;
        long weightedGrowthDays =
            (Overlap(current.AsOfDay, targetDay, 0, biology.MaturationDay) * 250L) +
            (Overlap(current.AsOfDay, targetDay, biology.MaturationDay, biology.PeakDay) * 1_000L) +
            (Overlap(current.AsOfDay, targetDay, biology.PeakDay, biology.DeclineDay) * 400L);
        long developmentFactors =
            profile.TalentFactor + profile.WorkFactor + profile.OpportunityFactor;
        long growth = checked(weightedGrowthDays * developmentFactors) / GrowthDenominator;
        long decliningDays = Math.Max(0, targetDay - Math.Max(current.AsOfDay, biology.DeclineDay));
        long decline = checked(
            decliningDays * (DevelopmentScale.Maximum - biology.Resilience)) /
            DeclineDenominator;

        CareerWear advancedWear = current.Wear.Advance(
            targetDay,
            exposurePerDay,
            biology);
        int wearDelta = advancedWear.DurableWear - current.Wear.DurableWear;
        long nextCapability = (long)current.Capability + growth - decline - wearDelta;
        return new DevelopmentState(
            targetDay,
            (int)Math.Clamp(nextCapability, 0, DevelopmentScale.Maximum),
            advancedWear);
    }

    private static int Overlap(
        int intervalStart,
        int intervalEnd,
        int phaseStart,
        int phaseEnd) =>
        Math.Max(0, Math.Min(intervalEnd, phaseEnd) - Math.Max(intervalStart, phaseStart));
}
