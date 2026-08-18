using System.Collections.Immutable;

namespace PCDynasty.Simulation.Teams;

public enum ChemistryEventKind
{
    SharedSuccess,
    SharedAdversity,
    RosterContinuity,
    InternalConflict,
    LeadershipChange,
}

public sealed record ChemistryFactor(
    long EventSequence,
    ChemistryEventKind Kind,
    string SourceId,
    int CohesionDelta,
    int TrustDelta,
    int FamiliarityDelta,
    string Explanation);

public sealed class TeamChemistry
{
    private TeamChemistry(
        string teamId,
        int cohesion,
        int trust,
        int familiarity,
        long lastAppliedSequence,
        ImmutableArray<ChemistryFactor> factors)
    {
        TeamId = teamId;
        Cohesion = cohesion;
        Trust = trust;
        Familiarity = familiarity;
        LastAppliedSequence = lastAppliedSequence;
        Factors = factors;
    }

    public string TeamId { get; }

    public int Cohesion { get; }

    public int Trust { get; }

    public int Familiarity { get; }

    public long LastAppliedSequence { get; }

    public ImmutableArray<ChemistryFactor> Factors { get; }

    public static TeamChemistry Create(
        string teamId,
        int cohesion,
        int trust,
        int familiarity)
    {
        ArgumentNullException.ThrowIfNull(teamId);
        string normalizedTeamId = teamId.Trim();
        if (normalizedTeamId.Length == 0)
        {
            throw new ArgumentException("A team identifier is required.", nameof(teamId));
        }

        return new TeamChemistry(
            normalizedTeamId,
            Score(cohesion, nameof(cohesion)),
            Score(trust, nameof(trust)),
            Score(familiarity, nameof(familiarity)),
            0,
            []);
    }

    internal TeamChemistry WithAdvance(
        int cohesion,
        int trust,
        int familiarity,
        long lastAppliedSequence,
        ImmutableArray<ChemistryFactor> factors) =>
        new(
            TeamId,
            cohesion,
            trust,
            familiarity,
            lastAppliedSequence,
            factors);

    private static int Score(int value, string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A chemistry score must be between zero and one hundred.");
        }

        return value;
    }
}
