namespace PCDynasty.Careers.Injuries;

public readonly record struct RecoveryState(
    int AsOfDay,
    int MedicalFitness,
    int CompetitiveReadiness,
    int DurableConsequence,
    bool IsMedicallyCleared,
    bool RecoveryWindowCompleted);

public static class RecoveryAdvancer
{
    public static RecoveryState Advance(
        InjuryInstance injury,
        int targetDay,
        int recoveryCapacity)
    {
        ArgumentNullException.ThrowIfNull(injury);
        InjuryValue.ValidateScore(recoveryCapacity, nameof(recoveryCapacity));
        ArgumentOutOfRangeException.ThrowIfLessThan(targetDay, injury.OccurredDay);

        int elapsedDays = targetDay - injury.OccurredDay;
        long effectiveElapsed = checked(
            (long)elapsedDays * (500L + recoveryCapacity)) / InjuryValue.Maximum;
        int initialMedical = InjuryValue.Maximum - injury.Severity;
        int medicalFitness = Interpolate(
            initialMedical,
            InjuryValue.Maximum,
            effectiveElapsed,
            injury.MedicalRecoveryDays);
        int maximumReadiness = InjuryValue.Maximum - injury.DurableConsequence;
        int initialReadiness = Math.Max(
            0,
            maximumReadiness - injury.Severity);
        int competitiveReadiness = Interpolate(
            initialReadiness,
            maximumReadiness,
            effectiveElapsed,
            injury.ReadinessRecoveryDays);

        return new RecoveryState(
            targetDay,
            medicalFitness,
            competitiveReadiness,
            injury.DurableConsequence,
            medicalFitness == InjuryValue.Maximum,
            effectiveElapsed >= injury.ReadinessRecoveryDays);
    }

    private static int Interpolate(
        int start,
        int end,
        long elapsed,
        int duration)
    {
        long boundedElapsed = Math.Min(elapsed, duration);
        return start + (int)(((long)(end - start) * boundedElapsed) / duration);
    }
}
