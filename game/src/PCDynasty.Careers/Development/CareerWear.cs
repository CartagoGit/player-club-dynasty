namespace PCDynasty.Careers.Development;

public sealed class CareerWear
{
    public CareerWear(
        int asOfDay,
        long accumulatedExposure,
        int durableWear)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(asOfDay);
        ArgumentOutOfRangeException.ThrowIfNegative(accumulatedExposure);
        AsOfDay = asOfDay;
        AccumulatedExposure = accumulatedExposure;
        DurableWear = DevelopmentScale.Validate(durableWear, nameof(durableWear));
    }

    public int AsOfDay { get; }
    public long AccumulatedExposure { get; }
    public int DurableWear { get; }

    internal CareerWear Advance(
        int targetDay,
        int exposurePerDay,
        BiologicalProfile biology)
    {
        ArgumentNullException.ThrowIfNull(biology);
        ArgumentOutOfRangeException.ThrowIfNegative(exposurePerDay);
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, AsOfDay);

        int elapsedDays = targetDay - AsOfDay;
        if (elapsedDays == 0)
        {
            return this;
        }

        long exposureDelta = checked((long)elapsedDays * exposurePerDay);
        long wearNumerator = checked(
            exposureDelta * (DevelopmentScale.Maximum - biology.Resilience));
        int wearDelta = (int)Math.Min(
            DevelopmentScale.Maximum,
            wearNumerator / 3_650_000L);
        return new CareerWear(
            targetDay,
            checked(AccumulatedExposure + exposureDelta),
            Math.Min(DevelopmentScale.Maximum, DurableWear + wearDelta));
    }
}
