using PCDynasty.Simulation.Training;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class TrainingResponseTests
{
    [Fact]
    public void BalancedWorkImprovesFormAndDevelopmentWhileAddingFatigue()
    {
        var policy = new BalancedTrainingResponsePolicy();
        var load = new TrainingLoad(
            durationMinutes: 75,
            intensity: 62,
            formEmphasis: 55,
            developmentEmphasis: 65);
        var participant = new TrainingParticipantState(
            currentForm: 58,
            currentFatigue: 18,
            recoveryCapacity: 76,
            developmentResponsiveness: 72);

        TrainingResponse response = policy.Resolve(load, participant);

        Assert.True(response.FormDelta > 0);
        Assert.True(response.FatigueDelta > 0);
        Assert.True(response.DevelopmentProgress > 0);
        Assert.Equal(load.WorkloadUnits, response.WorkloadUnits);
        Assert.Contains(
            response.Explanations,
            explanation => explanation.Kind == TrainingResponseFactor.FormStimulus);
        Assert.Contains(
            response.Explanations,
            explanation => explanation.Kind == TrainingResponseFactor.DevelopmentStimulus);
    }

    [Fact]
    public void ExcessiveLoadCreatesMoreFatigueAndCanDamageForm()
    {
        var policy = new BalancedTrainingResponsePolicy();
        Assert.IsAssignableFrom<ITrainingResponsePolicy>(policy);
        var participant = new TrainingParticipantState(
            currentForm: 70,
            currentFatigue: 75,
            recoveryCapacity: 30,
            developmentResponsiveness: 65);
        TrainingResponse moderate = policy.Resolve(
            new TrainingLoad(60, 45, 50, 50),
            participant);
        TrainingResponse excessive = policy.Resolve(
            new TrainingLoad(180, 95, 50, 50),
            participant);

        Assert.True(excessive.FatigueDelta > moderate.FatigueDelta);
        Assert.True(excessive.FormDelta < moderate.FormDelta);
        Assert.True(excessive.FormDelta < 0);
        Assert.Contains(
            excessive.Explanations,
            explanation => explanation.Kind == TrainingResponseFactor.Overload);
    }

    [Fact]
    public void ResponseIsDeterministicAndInputsAreValidated()
    {
        var policy = new BalancedTrainingResponsePolicy();
        var load = new TrainingLoad(90, 70, 45, 80);
        var participant = new TrainingParticipantState(50, 20, 70, 80);

        TrainingResponse first = policy.Resolve(load, participant);
        TrainingResponse repeated = policy.Resolve(load, participant);

        Assert.Equal(first.FormDelta, repeated.FormDelta);
        Assert.Equal(first.FatigueDelta, repeated.FatigueDelta);
        Assert.Equal(first.DevelopmentProgress, repeated.DevelopmentProgress);
        Assert.Equal(first.Explanations.ToArray(), repeated.Explanations.ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TrainingLoad(0, 70, 45, 80));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TrainingParticipantState(50, 101, 70, 80));
    }
}
