using System.Diagnostics;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace PCDynasty.Benchmarks.Tests;

public sealed class InteractionBudgetTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task ViewQueriesStayInsideVersionedInteractionBudgets()
    {
        IReadOnlyDictionary<string, TimeSpan> budgets = await LoadBudgetsAsync();
        var workload = new InteractionBenchmarks();

        AssertWithin("open-player-view", workload.OpenPlayerView, budgets);
        AssertWithin("open-club-view", workload.OpenClubView, budgets);
        AssertWithin("open-competition-view", workload.OpenCompetitionView, budgets);
    }

    [Fact]
    public async Task ClubMaterializationStaysInsideItsDeferredBudget()
    {
        IReadOnlyDictionary<string, TimeSpan> budgets = await LoadBudgetsAsync();
        var workload = new MaterializationBenchmarks();

        AssertWithin("materialize-club", workload.MaterializeClub, budgets);
    }

    private void AssertWithin(
        string budgetId,
        Func<long> workload,
        IReadOnlyDictionary<string, TimeSpan> budgets)
    {
        TimeSpan budget = budgets[budgetId];
        TimeSpan elapsed = MeasureMedian(workload);
        output.WriteLine(
            "{0}: median {1:F3} ms; budget {2:F3} ms",
            budgetId,
            elapsed.TotalMilliseconds,
            budget.TotalMilliseconds);
        Assert.True(
            elapsed <= budget,
            $"{budgetId} took {elapsed.TotalMilliseconds:F3} ms; " +
            $"budget is {budget.TotalMilliseconds:F3} ms.");
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

    private static async Task<IReadOnlyDictionary<string, TimeSpan>> LoadBudgetsAsync()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "interaction-budgets.json");
        await using FileStream stream = File.OpenRead(path);
        BudgetContract contract = await JsonSerializer.DeserializeAsync<BudgetContract>(
            stream,
            JsonOptions) ?? throw new InvalidDataException("Interaction budgets are empty.");
        Assert.Equal(1, contract.SchemaVersion);
        Assert.Equal(contract.Budgets.Length, contract.Budgets.Select(item => item.Id).Distinct().Count());
        return contract.Budgets.ToDictionary(
            item => item.Id,
            item => TimeSpan.FromMilliseconds(item.MaxMilliseconds),
            StringComparer.Ordinal);
    }

    private sealed class BudgetContract
    {
        public int SchemaVersion { get; init; }
        public Budget[] Budgets { get; init; } = [];
    }

    private sealed class Budget
    {
        public string Id { get; init; } = string.Empty;
        public double MaxMilliseconds { get; init; }
    }
}
