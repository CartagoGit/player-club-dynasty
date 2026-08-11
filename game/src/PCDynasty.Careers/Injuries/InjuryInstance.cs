namespace PCDynasty.Careers.Injuries;

public sealed class InjuryInstance
{
    public InjuryInstance(
        string injuryId,
        string subjectId,
        int occurredDay,
        int medicalRecoveryDays,
        int readinessRecoveryDays,
        int severity,
        int durableConsequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(occurredDay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(medicalRecoveryDays);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            readinessRecoveryDays,
            medicalRecoveryDays);
        InjuryValue.ValidateScore(severity, nameof(severity));
        ArgumentOutOfRangeException.ThrowIfZero(severity);
        InjuryValue.ValidateScore(durableConsequence, nameof(durableConsequence));

        InjuryId = InjuryValue.Required(injuryId, nameof(injuryId));
        SubjectId = InjuryValue.Required(subjectId, nameof(subjectId));
        OccurredDay = occurredDay;
        MedicalRecoveryDays = medicalRecoveryDays;
        ReadinessRecoveryDays = readinessRecoveryDays;
        Severity = severity;
        DurableConsequence = durableConsequence;
    }

    public string InjuryId { get; }
    public string SubjectId { get; }
    public int OccurredDay { get; }
    public int MedicalRecoveryDays { get; }
    public int ReadinessRecoveryDays { get; }
    public int Severity { get; }
    public int DurableConsequence { get; }
}

internal static class InjuryValue
{
    public const int Maximum = 1_000;

    public static int ValidateScore(int value, string parameterName) =>
        value is >= 0 and <= Maximum
            ? value
            : throw new ArgumentOutOfRangeException(parameterName);

    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
