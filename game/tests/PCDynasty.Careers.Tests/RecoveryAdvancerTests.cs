using PCDynasty.Careers.Injuries;
using Xunit;

namespace PCDynasty.Careers.Tests;

public sealed class RecoveryAdvancerTests
{
    [Fact]
    public void MedicalFitnessAndCompetitiveReadinessRecoverIndependently()
    {
        var injury = new InjuryInstance(
            "injury:7",
            "person:9",
            occurredDay: 10_000,
            medicalRecoveryDays: 30,
            readinessRecoveryDays: 60,
            severity: 700,
            durableConsequence: 80);

        RecoveryState atMedicalClearance = RecoveryAdvancer.Advance(
            injury,
            targetDay: 10_030,
            recoveryCapacity: 500);
        RecoveryState fullyAdvanced = RecoveryAdvancer.Advance(
            injury,
            targetDay: 20_000,
            recoveryCapacity: 500);

        Assert.Equal(1_000, atMedicalClearance.MedicalFitness);
        Assert.True(atMedicalClearance.CompetitiveReadiness < 920);
        Assert.Equal(1_000, fullyAdvanced.MedicalFitness);
        Assert.Equal(920, fullyAdvanced.CompetitiveReadiness);
        Assert.Equal(80, fullyAdvanced.DurableConsequence);
        Assert.True(fullyAdvanced.IsMedicallyCleared);
        Assert.True(fullyAdvanced.RecoveryWindowCompleted);
    }

    [Fact]
    public void RecoveryIsDeterministicAndRejectsImpossibleTimelines()
    {
        var injury = new InjuryInstance(
            "injury:8",
            "person:9",
            occurredDay: 100,
            medicalRecoveryDays: 10,
            readinessRecoveryDays: 20,
            severity: 400,
            durableConsequence: 0);

        RecoveryState first = RecoveryAdvancer.Advance(injury, 107, 700);
        RecoveryState replay = RecoveryAdvancer.Advance(injury, 107, 700);

        Assert.Equal(first, replay);
        Assert.Throws<ArgumentOutOfRangeException>(() => new InjuryInstance(
            "injury:bad",
            "person:9",
            occurredDay: 100,
            medicalRecoveryDays: 30,
            readinessRecoveryDays: 20,
            severity: 400,
            durableConsequence: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecoveryAdvancer.Advance(injury, 99, 500));
    }
}
