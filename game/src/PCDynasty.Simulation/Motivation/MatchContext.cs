namespace PCDynasty.Simulation.Motivation;

public sealed class MatchContext
{
    public MatchContext(
        string contestId,
        int importance,
        int rivalry,
        int externalExpectation,
        int homeSupport,
        int recentMomentum,
        int squadFatigue)
    {
        ArgumentNullException.ThrowIfNull(contestId);
        string normalizedContestId = contestId.Trim();
        if (normalizedContestId.Length == 0)
        {
            throw new ArgumentException("A contest identifier is required.", nameof(contestId));
        }

        ContestId = normalizedContestId;
        Importance = Score(importance, nameof(importance));
        Rivalry = Score(rivalry, nameof(rivalry));
        ExternalExpectation = Score(externalExpectation, nameof(externalExpectation));
        HomeSupport = Score(homeSupport, nameof(homeSupport));
        RecentMomentum = Score(recentMomentum, nameof(recentMomentum));
        SquadFatigue = Score(squadFatigue, nameof(squadFatigue));
    }

    public string ContestId { get; }

    public int Importance { get; }

    public int Rivalry { get; }

    public int ExternalExpectation { get; }

    public int HomeSupport { get; }

    public int RecentMomentum { get; }

    public int SquadFatigue { get; }

    private static int Score(int value, string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A match-context score must be between zero and one hundred.");
        }

        return value;
    }
}
