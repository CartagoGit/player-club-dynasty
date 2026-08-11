using System.Text.Json;
using PCDynasty.World.Commitments;
using PCDynasty.World.Materialization;
using Xunit;

namespace PCDynasty.World.Tests;

public sealed class CrossFidelitySimulationTests
{
    [Fact]
    public void MacroDetailedAndMaterializeAgreeOnCommittedOutcomes()
    {
        FidelityScenario scenario = FidelityScenario.Load();
        HistoricalCommitment macro = CreateCommitment(scenario, SimulationFidelity.Macro);
        HistoricalCommitment detailed = CreateCommitment(scenario, SimulationFidelity.Detailed);
        var macroLedger = new MaterializationLedger(
            [new FidelityOwnership(scenario.SubjectId, SimulationFidelity.Macro, 0, null)],
            [macro],
            []);
        MaterializationLedger materialized = FidelityPromotion.Apply(
            macroLedger,
            new FidelityPromotionBoundary(
                scenario.SubjectId,
                SimulationFidelity.Macro,
                SimulationFidelity.Detailed,
                scenario.PromotionTick));

        string macroInvariant = InvariantFingerprint(macro);
        Assert.Equal(macroInvariant, InvariantFingerprint(detailed));
        Assert.Equal(
            macroInvariant,
            InvariantFingerprint(Assert.Single(materialized.Commitments)));
        Assert.Equal(
            SimulationFidelity.Detailed,
            materialized.Ownership.Single(ownership =>
                ownership.UntilTickExclusive is null).Fidelity);
    }

    [Fact]
    public void ScenarioIsVersionedAndKeepsModeSpecificDetailsOutOfCommitments()
    {
        FidelityScenario scenario = FidelityScenario.Load();
        HistoricalCommitment macro = CreateCommitment(scenario, SimulationFidelity.Macro);
        HistoricalCommitment detailed = CreateCommitment(scenario, SimulationFidelity.Detailed);

        Assert.Equal(1, scenario.Version);
        Assert.Equal(macro.Values, detailed.Values);
        Assert.DoesNotContain("event-count", macro.Values.Keys);
        Assert.True(scenario.DetailedEventCount > 0);
    }

    private static HistoricalCommitment CreateCommitment(
        FidelityScenario scenario,
        SimulationFidelity fidelity) =>
        new(
            scenario.CommitmentId,
            scenario.SubjectId,
            scenario.OutcomeId,
            scenario.CommittedAtTick,
            scenario.EffectiveAtTick,
            fidelity,
            new Dictionary<string, string>
            {
                ["winner"] = scenario.WinnerId,
                ["score"] = scenario.Score,
            });

    private static string InvariantFingerprint(HistoricalCommitment commitment) =>
        string.Join(
            '|',
            commitment.SubjectId,
            commitment.OutcomeId,
            commitment.EffectiveAtTick,
            commitment.Values["winner"],
            commitment.Values["score"]);
}

internal sealed class FidelityScenario
{
    public int Version { get; init; }
    public string CommitmentId { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string OutcomeId { get; init; } = string.Empty;
    public long CommittedAtTick { get; init; }
    public long EffectiveAtTick { get; init; }
    public long PromotionTick { get; init; }
    public string WinnerId { get; init; } = string.Empty;
    public string Score { get; init; } = string.Empty;
    public int DetailedEventCount { get; init; }

    public static FidelityScenario Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "FidelityScenario.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<FidelityScenario>(json, JsonOptions) ??
            throw new InvalidDataException("Fidelity scenario fixture is empty.");
    }

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
