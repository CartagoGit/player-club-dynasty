namespace PCDynasty.Core.Performance;

public readonly record struct CapabilityProfile
{
    public CapabilityProfile(int technique, int physical, int mental)
    {
        Technique = EnsureScore(technique, nameof(technique));
        Physical = EnsureScore(physical, nameof(physical));
        Mental = EnsureScore(mental, nameof(mental));
    }

    public int Technique { get; }

    public int Physical { get; }

    public int Mental { get; }

    internal static int EnsureScore(int score, string parameterName)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                score,
                "A compiled score must be between zero and one hundred.");
        }

        return score;
    }
}
