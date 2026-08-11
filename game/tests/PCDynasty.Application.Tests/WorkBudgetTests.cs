using PCDynasty.Application.Performance;
using Xunit;

namespace PCDynasty.Application.Tests;

public sealed class WorkBudgetTests
{
    [Fact]
    public void WorkSliceYieldsOnEitherTimeOrUnitBoundary()
    {
        var budget = new WorkBudget(
            maximumSliceDuration: TimeSpan.FromMilliseconds(8),
            maximumUnitsPerSlice: 64);

        Assert.False(budget.ShouldYield(63, TimeSpan.FromMilliseconds(7)));
        Assert.True(budget.ShouldYield(64, TimeSpan.FromMilliseconds(1)));
        Assert.True(budget.ShouldYield(1, TimeSpan.FromMilliseconds(8)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            budget.ShouldYield(-1, TimeSpan.Zero));
    }

    [Fact]
    public void CheckpointsReportProgressWithoutPretendingToBeFinal()
    {
        BackgroundWorkStatus started = BackgroundWorkStatus.Start(
            "work:advance-season",
            "manager-command:advance",
            totalUnits: 512,
            requestedFromVersion: 40);

        BackgroundWorkStatus partial = started.ReportCheckpoint(
            completedUnits: 256,
            checkpointVersion: 41);
        BackgroundWorkStatus allUnitsProcessed = partial.ReportCheckpoint(
            completedUnits: 512,
            checkpointVersion: 42);

        Assert.Equal(BackgroundWorkState.Running, partial.State);
        Assert.Equal(0.5, partial.Progress);
        Assert.False(partial.IsFinal);
        Assert.False(partial.CanPublishFinalResult(visibleVersion: 40));
        Assert.False(allUnitsProcessed.IsFinal);

        BackgroundWorkStatus completed = allUnitsProcessed.Complete(checkpointVersion: 42);
        Assert.True(completed.IsFinal);
        Assert.True(completed.CanPublishFinalResult(visibleVersion: 40));
        Assert.False(completed.CanPublishFinalResult(visibleVersion: 41));
    }

    [Fact]
    public void ProgressAndCheckpointVersionsCannotMoveBackward()
    {
        BackgroundWorkStatus status = BackgroundWorkStatus.Start(
                "work:index-pack",
                "pack-import",
                totalUnits: 10,
                requestedFromVersion: 7)
            .ReportCheckpoint(5, checkpointVersion: 8);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            status.ReportCheckpoint(4, checkpointVersion: 9));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            status.ReportCheckpoint(6, checkpointVersion: 7));
        Assert.Throws<InvalidOperationException>(() =>
            status.Complete(checkpointVersion: 8));
    }
}
