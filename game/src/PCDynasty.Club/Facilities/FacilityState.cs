namespace PCDynasty.Club.Facilities;

public sealed class MaintenancePlan
{
    public MaintenancePlan(
        int annualConditionDecay,
        int qualityRestoration,
        int conditionRestoration)
    {
        AnnualConditionDecay = FacilityValue.Score(
            annualConditionDecay,
            nameof(annualConditionDecay));
        QualityRestoration = FacilityValue.Score(
            qualityRestoration,
            nameof(qualityRestoration));
        ConditionRestoration = FacilityValue.Score(
            conditionRestoration,
            nameof(conditionRestoration));
    }

    public int AnnualConditionDecay { get; }
    public int QualityRestoration { get; }
    public int ConditionRestoration { get; }
}

public sealed class FacilityState
{
    public FacilityState(
        string facilityId,
        string ownerClubId,
        int asOfDay,
        int quality,
        int condition,
        int modernization)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(asOfDay);
        FacilityId = FacilityValue.Required(facilityId, nameof(facilityId));
        OwnerClubId = FacilityValue.Required(ownerClubId, nameof(ownerClubId));
        AsOfDay = asOfDay;
        Quality = FacilityValue.Score(quality, nameof(quality));
        Condition = FacilityValue.Score(condition, nameof(condition));
        Modernization = FacilityValue.Score(modernization, nameof(modernization));
    }

    public string FacilityId { get; }
    public string OwnerClubId { get; }
    public int AsOfDay { get; }
    public int Quality { get; }
    public int Condition { get; }
    public int Modernization { get; }

    public FacilityState ApplyMaintenance(int targetDay, MaintenancePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, AsOfDay);
        int elapsedDays = targetDay - AsOfDay;
        long decay = ((long)elapsedDays * plan.AnnualConditionDecay) / 365;
        int decayedCondition = (int)Math.Max(0, Condition - decay);
        return new FacilityState(
            FacilityId,
            OwnerClubId,
            targetDay,
            Math.Min(FacilityValue.Maximum, Quality + plan.QualityRestoration),
            Math.Min(FacilityValue.Maximum, decayedCondition + plan.ConditionRestoration),
            Modernization);
    }

    public FacilityState Modernize(
        int targetDay,
        int modernizationGain,
        int qualityGain)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, AsOfDay);
        FacilityValue.Score(modernizationGain, nameof(modernizationGain));
        FacilityValue.Score(qualityGain, nameof(qualityGain));
        return new FacilityState(
            FacilityId,
            OwnerClubId,
            targetDay,
            Math.Min(FacilityValue.Maximum, Quality + qualityGain),
            Condition,
            Math.Min(FacilityValue.Maximum, Modernization + modernizationGain));
    }
}

internal static class FacilityValue
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
