namespace PCDynasty.Football.Positions;

public sealed class PositionFamiliarity
{
    public PositionFamiliarity(
        string positionId,
        int familiarity,
        int experience,
        int preference)
    {
        PositionId = FootballDomainValue.NormalizeIdentifier(positionId, nameof(positionId));
        Familiarity = FootballDomainValue.EnsureScore(familiarity, nameof(familiarity));
        Experience = FootballDomainValue.EnsureScore(experience, nameof(experience));
        Preference = FootballDomainValue.EnsureScore(preference, nameof(preference));
    }

    public string PositionId { get; }

    public int Familiarity { get; }

    public int Experience { get; }

    public int Preference { get; }
}

internal static class FootballDomainValue
{
    public static int EnsureScore(int score, string parameterName)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                score,
                "A football score must be between zero and one hundred.");
        }

        return score;
    }

    public static string NormalizeIdentifier(string identifier, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifier, parameterName);

        string normalized = identifier.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("An identifier is required.", parameterName);
        }

        return normalized;
    }
}
