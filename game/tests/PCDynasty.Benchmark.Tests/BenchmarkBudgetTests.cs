using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace PCDynasty.Benchmarks.Tests;

public sealed class BenchmarkBudgetTests(ITestOutputHelper output)
{
    private static readonly TimeSpan ContestBatchBudget = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan DevelopmentBatchBudget = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void ContestBatchRemainsInsideInteractiveBudget()
    {
        var workload = new ContestBenchmarks();

        TimeSpan elapsed = MeasureMedian(workload.SimulateContestBatch);

        Report("contest", ContestBenchmarks.ContestCount, elapsed, ContestBatchBudget);
        Assert.True(
            elapsed <= ContestBatchBudget,
            $"Contest batch took {elapsed.TotalMilliseconds:F3} ms; budget is " +
            $"{ContestBatchBudget.TotalMilliseconds:F3} ms.");
    }

    [Fact]
    public void DevelopmentBatchRemainsInsideInteractiveBudget()
    {
        var workload = new DevelopmentBenchmarks();

        TimeSpan elapsed = MeasureMedian(workload.AdvanceDevelopmentBatch);

        Report("development", DevelopmentBenchmarks.CareerCount, elapsed, DevelopmentBatchBudget);
        Assert.True(
            elapsed <= DevelopmentBatchBudget,
            $"Development batch took {elapsed.TotalMilliseconds:F3} ms; budget is " +
            $"{DevelopmentBatchBudget.TotalMilliseconds:F3} ms.");
    }

    private static TimeSpan MeasureMedian(Func<long> workload)
    {
        GC.KeepAlive(workload());
        var samples = new long[5];
        for (int index = 0; index < samples.Length; index++)
        {
            long started = Stopwatch.GetTimestamp();
            GC.KeepAlive(workload());
            samples[index] = Stopwatch.GetTimestamp() - started;
        }

        Array.Sort(samples);
        return TimeSpan.FromSeconds(
            (double)samples[samples.Length / 2] / Stopwatch.Frequency);
    }

    private void Report(
        string workload,
        int operations,
        TimeSpan elapsed,
        TimeSpan budget)
    {
        output.WriteLine(
            "{0}: {1} operations in {2:F3} ms (budget {3:F3} ms, {4:F3} us/op)",
            workload,
            operations,
            elapsed.TotalMilliseconds,
            budget.TotalMilliseconds,
            elapsed.TotalMicroseconds / operations);
    }
}
