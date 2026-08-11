using System.Collections.Immutable;

namespace PCDynasty.Simulation.Training;

public sealed record TrainingParticipantState
{
    public TrainingParticipantState(
        int currentForm,
        int currentFatigue,
        int recoveryCapacity,
        int developmentResponsiveness)
    {
        CurrentForm = TrainingLoad.Score(currentForm, nameof(currentForm));
        CurrentFatigue = TrainingLoad.Score(currentFatigue, nameof(currentFatigue));
        RecoveryCapacity = TrainingLoad.Score(recoveryCapacity, nameof(recoveryCapacity));
        DevelopmentResponsiveness = TrainingLoad.Score(
            developmentResponsiveness,
            nameof(developmentResponsiveness));
    }

    public int CurrentForm { get; }

    public int CurrentFatigue { get; }

    public int RecoveryCapacity { get; }

    public int DevelopmentResponsiveness { get; }
}

public enum TrainingResponseFactor
{
    FormStimulus,
    FatigueCost,
    DevelopmentStimulus,
    Overload,
}

public sealed record TrainingResponseExplanation(
    TrainingResponseFactor Kind,
    string Message,
    int Contribution);

public sealed record TrainingResponse(
    int WorkloadUnits,
    int FormDelta,
    int FatigueDelta,
    int DevelopmentProgress,
    ImmutableArray<TrainingResponseExplanation> Explanations);

public interface ITrainingResponsePolicy
{
    TrainingResponse Resolve(
        TrainingLoad load,
        TrainingParticipantState participant);
}

public sealed class BalancedTrainingResponsePolicy : ITrainingResponsePolicy
{
    public TrainingResponse Resolve(
        TrainingLoad load,
        TrainingParticipantState participant)
    {
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(participant);

        double workloadIndex = load.WorkloadUnits / 60d;
        int fatigueDelta = (int)Math.Ceiling(
            workloadIndex * (140 - participant.RecoveryCapacity) / 10d);
        fatigueDelta = Math.Clamp(fatigueDelta, 0, 100);

        double formStimulus = workloadIndex * load.FormEmphasis / 25d;
        double fatigueOverload = Math.Max(
            0,
            participant.CurrentFatigue + fatigueDelta - 75) / 5d;
        double volumeOverload = Math.Max(0, workloadIndex - 1.5) * 4;
        int overloadPenalty = (int)Math.Ceiling(fatigueOverload + volumeOverload);
        int formDelta = Math.Clamp(
            (int)Math.Round(formStimulus) - overloadPenalty,
            -100,
            100);

        double rawDevelopment =
            workloadIndex *
            load.DevelopmentEmphasis *
            participant.DevelopmentResponsiveness /
            100d;
        int developmentProgress = Math.Clamp(
            (int)Math.Round(rawDevelopment) - overloadPenalty * 2,
            0,
            100);

        var explanations = ImmutableArray.CreateBuilder<TrainingResponseExplanation>();
        explanations.Add(new TrainingResponseExplanation(
            TrainingResponseFactor.FormStimulus,
            "Workload and form emphasis create a short-term form stimulus.",
            (int)Math.Round(formStimulus)));
        explanations.Add(new TrainingResponseExplanation(
            TrainingResponseFactor.FatigueCost,
            "Workload adds fatigue according to the participant's recovery capacity.",
            fatigueDelta));
        explanations.Add(new TrainingResponseExplanation(
            TrainingResponseFactor.DevelopmentStimulus,
            "Development emphasis and responsiveness create durable progress.",
            developmentProgress));

        if (overloadPenalty > 0)
        {
            explanations.Add(new TrainingResponseExplanation(
                TrainingResponseFactor.Overload,
                "Existing fatigue or excessive volume reduced the useful response.",
                -overloadPenalty));
        }

        return new TrainingResponse(
            load.WorkloadUnits,
            formDelta,
            fatigueDelta,
            developmentProgress,
            explanations.ToImmutable());
    }
}
