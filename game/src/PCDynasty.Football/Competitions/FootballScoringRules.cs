namespace PCDynasty.Football.Competitions;

public readonly record struct FootballPointsAward(int HomePoints, int AwayPoints);

public sealed class FootballScoringRules
{
    public FootballScoringRules(int winPoints, int drawPoints, int lossPoints)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(winPoints);
        ArgumentOutOfRangeException.ThrowIfNegative(drawPoints);
        ArgumentOutOfRangeException.ThrowIfNegative(lossPoints);
        if (winPoints <= drawPoints || drawPoints < lossPoints)
        {
            throw new ArgumentException(
                "Football points must satisfy win > draw >= loss.");
        }

        WinPoints = winPoints;
        DrawPoints = drawPoints;
        LossPoints = lossPoints;
    }

    public int WinPoints { get; }
    public int DrawPoints { get; }
    public int LossPoints { get; }

    public FootballPointsAward Award(int homeScore, int awayScore)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(homeScore);
        ArgumentOutOfRangeException.ThrowIfNegative(awayScore);

        if (homeScore == awayScore)
        {
            return new FootballPointsAward(DrawPoints, DrawPoints);
        }

        return homeScore > awayScore
            ? new FootballPointsAward(WinPoints, LossPoints)
            : new FootballPointsAward(LossPoints, WinPoints);
    }
}
