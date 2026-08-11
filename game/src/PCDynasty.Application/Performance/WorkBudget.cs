namespace PCDynasty.Application.Performance;

public sealed class WorkBudget
{
    public WorkBudget(
        TimeSpan maximumSliceDuration,
        int maximumUnitsPerSlice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            maximumSliceDuration,
            TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumUnitsPerSlice);
        MaximumSliceDuration = maximumSliceDuration;
        MaximumUnitsPerSlice = maximumUnitsPerSlice;
    }

    public TimeSpan MaximumSliceDuration { get; }

    public int MaximumUnitsPerSlice { get; }

    public bool ShouldYield(int completedUnitsInSlice, TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completedUnitsInSlice);
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);

        return completedUnitsInSlice >= MaximumUnitsPerSlice ||
               elapsed >= MaximumSliceDuration;
    }
}
