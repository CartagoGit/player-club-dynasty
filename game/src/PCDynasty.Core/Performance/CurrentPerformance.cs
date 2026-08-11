namespace PCDynasty.Core.Performance;

public readonly record struct CurrentPerformance
{
    private CurrentPerformance(int score)
    {
        Score = score;
    }

    public int Score { get; }

    public static CurrentPerformance Derive(
        CapabilityProfile capabilities,
        int form,
        CurrentPerformanceWeights weights)
    {
        int validatedForm = CapabilityProfile.EnsureScore(form, nameof(form));
        long totalWeight = weights.Total;
        if (totalWeight == 0)
        {
            throw new ArgumentException("At least one performance factor must contribute.", nameof(weights));
        }

        long weightedScore =
            ((long)capabilities.Technique * weights.Technique) +
            ((long)validatedForm * weights.Form) +
            ((long)capabilities.Physical * weights.Physical) +
            ((long)capabilities.Mental * weights.Mental);

        int roundedScore = (int)((weightedScore + (totalWeight / 2)) / totalWeight);
        return new CurrentPerformance(roundedScore);
    }
}

public readonly record struct CurrentPerformanceWeights
{
    public CurrentPerformanceWeights(int technique, int form, int physical, int mental)
    {
        Technique = EnsureWeight(technique, nameof(technique));
        Form = EnsureWeight(form, nameof(form));
        Physical = EnsureWeight(physical, nameof(physical));
        Mental = EnsureWeight(mental, nameof(mental));

        if (Total == 0)
        {
            throw new ArgumentException("At least one performance factor must contribute.");
        }
    }

    public int Technique { get; }

    public int Form { get; }

    public int Physical { get; }

    public int Mental { get; }

    internal long Total => (long)Technique + Form + Physical + Mental;

    private static int EnsureWeight(int weight, string parameterName)
    {
        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                weight,
                "A performance weight cannot be negative.");
        }

        return weight;
    }
}
