using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Competitions.Simulation;

public sealed class CompetitionSimulationService : ICompetitionSimulationService
{
    private readonly object _seedGate = new();
    private readonly TimeProvider _timeProvider;
    private long _lastSeed;

    public CompetitionSimulationService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public MatchExecution PlayMatch(
        ManagedClubContext managed,
        string competitionDisplayName,
        ClubRow home,
        ClubRow away)
    {
        ArgumentNullException.ThrowIfNull(managed);
        ArgumentNullException.ThrowIfNull(home);
        ArgumentNullException.ThrowIfNull(away);
        string competition = Required(competitionDisplayName, nameof(competitionDisplayName));
        EnsureManagedTakesPart(managed, [home.ExternalId, away.ExternalId]);
        MatchResult result = MatchSimulator.Simulate(home, away, ReserveSeedRange(1));
        var history = new HistoryEntry(
            CompetitionDisplayName: competition,
            HomeClubId: home.ExternalId,
            AwayClubId: away.ExternalId,
            HomeDisplayName: home.DisplayName,
            AwayDisplayName: away.DisplayName,
            HomeGoals: result.HomeGoals,
            AwayGoals: result.AwayGoals,
            Seed: result.Seed,
            PlayedAtUtc: UtcNow());
        return new MatchExecution(result, history);
    }

    /// <summary>
    /// A career may only produce facts for matches it takes part in: recording
    /// a fixture between two rivals would let a browsed club rewrite history.
    /// </summary>
    private static void EnsureManagedTakesPart(
        ManagedClubContext managed,
        IReadOnlyList<string> clubExternalIds)
    {
        if (clubExternalIds.Any(managed.Owns))
        {
            return;
        }

        throw new ClubAuthorityException(managed.ClubExternalId, clubExternalIds[0]);
    }

    public MatchResult Replay(MatchResult previous)
    {
        ArgumentNullException.ThrowIfNull(previous);
        return MatchSimulator.Simulate(previous.HomeClub, previous.AwayClub, previous.Seed);
    }

    public SeasonExecution PlaySeason(
        ManagedClubContext managed,
        string competitionExternalId,
        string competitionDisplayName,
        ClubRow managedClub,
        IReadOnlyList<ClubRow> participants)
    {
        ArgumentNullException.ThrowIfNull(managed);
        ArgumentNullException.ThrowIfNull(managedClub);
        ArgumentNullException.ThrowIfNull(participants);
        managed.EnsureOwns(managedClub.ExternalId);
        ImmutableArray<ClubRow> participantSnapshot = participants.ToImmutableArray();
        if (!participantSnapshot.Any(club => string.Equals(
                club.ExternalId,
                managedClub.ExternalId,
                StringComparison.Ordinal)))
        {
            throw new ArgumentException("Participants must contain the managed club.", nameof(participants));
        }

        int fixtureCount = SeasonSimulator.CalculateFixtureCount(participantSnapshot.Length);
        SeasonResult result = SeasonSimulator.Simulate(
            competitionExternalId,
            competitionDisplayName,
            participantSnapshot,
            ReserveSeedRange(fixtureCount));
        DateTime playedAtUtc = UtcNow();
        ImmutableArray<HistoryEntry> matches = result.Fixtures
            .Select(fixture => new HistoryEntry(
                CompetitionDisplayName: result.CompetitionDisplayName,
                HomeClubId: fixture.HomeClubId,
                AwayClubId: fixture.AwayClubId,
                HomeDisplayName: fixture.HomeDisplayName,
                AwayDisplayName: fixture.AwayDisplayName,
                HomeGoals: fixture.HomeGoals,
                AwayGoals: fixture.AwayGoals,
                Seed: fixture.Seed,
                PlayedAtUtc: playedAtUtc))
            .ToImmutableArray();

        int managedIndex = FindStanding(result.Standings, managedClub.ExternalId);
        if (managedIndex < 0)
        {
            throw new InvalidOperationException("The simulation did not produce a managed-club standing.");
        }

        StandingRow managedStanding = result.Standings[managedIndex];
        var season = new SeasonHistoryEntry(
            CompetitionDisplayName: result.CompetitionDisplayName,
            ManagedClubId: managedClub.ExternalId,
            ManagedClubDisplayName: managedClub.DisplayName,
            ManagedPosition: managedIndex + 1,
            ManagedPoints: managedStanding.Points,
            ManagedGoalsFor: managedStanding.GoalsFor,
            ManagedGoalsAgainst: managedStanding.GoalsAgainst,
            TotalClubs: result.Standings.Length,
            TotalFixtures: result.Fixtures.Length,
            PlayedAtUtc: playedAtUtc,
            FinalStandings: result.Standings);
        return new SeasonExecution(result, matches, season);
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private static int FindStanding(ImmutableArray<StandingRow> standings, string clubExternalId)
    {
        for (int index = 0; index < standings.Length; index++)
        {
            if (string.Equals(standings[index].ClubExternalId, clubExternalId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private long ReserveSeedRange(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        long clockSeed = _timeProvider.GetUtcNow().UtcTicks & long.MaxValue;
        lock (_seedGate)
        {
            try
            {
                long firstSeed = Math.Max(clockSeed, checked(_lastSeed + 1));
                _lastSeed = checked(firstSeed + count - 1L);
                return firstSeed;
            }
            catch (OverflowException exception)
            {
                throw new InvalidOperationException("The simulation seed range is exhausted.", exception);
            }
        }
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
