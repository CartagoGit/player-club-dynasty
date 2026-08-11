using PCDynasty.Core.Domain;

namespace PCDynasty.Knowledge.Observations;

public sealed class Observation
{
    public Observation(
        PersonId subjectId,
        AttributeId attributeId,
        int minimumEstimate,
        int maximumEstimate,
        int confidence,
        string sourceId,
        DateOnly observedOn)
    {
        if (subjectId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(subjectId),
                subjectId,
                "An observation must identify a person.");
        }

        int validatedMinimum = EnsureScore(minimumEstimate, nameof(minimumEstimate));
        int validatedMaximum = EnsureScore(maximumEstimate, nameof(maximumEstimate));
        if (validatedMinimum > validatedMaximum)
        {
            throw new ArgumentException(
                "The minimum estimate cannot exceed the maximum estimate.",
                nameof(minimumEstimate));
        }

        if (observedOn == DateOnly.MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(observedOn),
                observedOn,
                "An observation date is required.");
        }

        SubjectId = subjectId;
        AttributeId = attributeId;
        MinimumEstimate = validatedMinimum;
        MaximumEstimate = validatedMaximum;
        Confidence = EnsureScore(confidence, nameof(confidence));
        SourceId = NormalizeSource(sourceId);
        ObservedOn = observedOn;
    }

    public PersonId SubjectId { get; }

    public AttributeId AttributeId { get; }

    public int MinimumEstimate { get; }

    public int MaximumEstimate { get; }

    public int Confidence { get; }

    public string SourceId { get; }

    public DateOnly ObservedOn { get; }

    private static int EnsureScore(int score, string parameterName)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                score,
                "An observation score must be between zero and one hundred.");
        }

        return score;
    }

    private static string NormalizeSource(string sourceId)
    {
        ArgumentNullException.ThrowIfNull(sourceId);

        string normalized = sourceId.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("An observation source is required.", nameof(sourceId));
        }

        return normalized;
    }
}
