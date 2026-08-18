using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Competitions.Simulation;

/// <summary>
/// Plays a full round-robin tournament over the participants of a single
/// competition and returns the standings plus the per-fixture results.
/// One win = 3 points, one draw = 1, one loss = 0.
/// </summary>
/// <remarks>
/// Pure helper: takes the snapshot already loaded by
/// <see cref="IManagerWorld.ListCompetitionsForClub"/> and the clubs the
/// caller already resolved, so the season simulator never touches the
/// content layer directly.
/// </remarks>
public static class SeasonSimulator
{
    public static SeasonResult Simulate(
        string competitionExternalId,
        string competitionDisplayName,
        IReadOnlyList<ClubRow> participants,
        long seed,
        MatchSimulatorOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(competitionExternalId);
        ArgumentNullException.ThrowIfNull(participants);
        if (participants.Count < 2)
        {
            throw new ArgumentException(
                "A competition needs at least two participants to play a season.",
                nameof(participants));
        }

        var standings = new Dictionary<string, StandingRow>(StringComparer.Ordinal);
        foreach (var club in participants)
        {
            standings[club.ExternalId] = new StandingRow(
                ClubExternalId: club.ExternalId,
                DisplayName: club.DisplayName,
                ShortName: club.ShortName,
                Points: 0,
                Played: 0,
                Wins: 0,
                Draws: 0,
                Losses: 0,
                GoalsFor: 0,
                GoalsAgainst: 0);
        }

        var fixtures = ImmutableArray.CreateBuilder<SeasonFixture>();
        long nextSeed = seed;

        for (int i = 0; i < participants.Count; i++)
        {
            for (int j = i + 1; j < participants.Count; j++)
            {
                var home = participants[i];
                var away = participants[j];
                long fixtureSeed = nextSeed;
                var result = MatchSimulator.Simulate(home, away, fixtureSeed, options);
                nextSeed++;

                fixtures.Add(new SeasonFixture(
                    HomeClubId: home.ExternalId,
                    AwayClubId: away.ExternalId,
                    HomeDisplayName: home.DisplayName,
                    AwayDisplayName: away.DisplayName,
                    HomeGoals: result.HomeGoals,
                    AwayGoals: result.AwayGoals,
                    Seed: fixtureSeed,
                    GoalEvents: result.Events.ToImmutableArray()));

                ApplyResult(standings, result);
            }
        }

        var ordered = standings.Values
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.GoalDifference)
            .ThenByDescending(row => row.GoalsFor)
            .ThenBy(row => row.DisplayName, StringComparer.Ordinal)
            .ToImmutableArray();

        return new SeasonResult(
            CompetitionExternalId: competitionExternalId,
            CompetitionDisplayName: competitionDisplayName,
            Standings: ordered,
            Fixtures: fixtures.ToImmutable());
    }

    private static void ApplyResult(
        Dictionary<string, StandingRow> standings,
        MatchResult result)
    {
        var home = standings[result.HomeClub.ExternalId];
        standings[result.HomeClub.ExternalId] = home with
        {
            Played = home.Played + 1,
            GoalsFor = home.GoalsFor + result.HomeGoals,
            GoalsAgainst = home.GoalsAgainst + result.AwayGoals,
            Wins = home.Wins + (result.HomeGoals > result.AwayGoals ? 1 : 0),
            Draws = home.Draws + (result.HomeGoals == result.AwayGoals ? 1 : 0),
            Losses = home.Losses + (result.HomeGoals < result.AwayGoals ? 1 : 0),
            Points = home.Points + Points(result.HomeGoals, result.AwayGoals),
        };

        var away = standings[result.AwayClub.ExternalId];
        standings[result.AwayClub.ExternalId] = away with
        {
            Played = away.Played + 1,
            GoalsFor = away.GoalsFor + result.AwayGoals,
            GoalsAgainst = away.GoalsAgainst + result.HomeGoals,
            Wins = away.Wins + (result.AwayGoals > result.HomeGoals ? 1 : 0),
            Draws = away.Draws + (result.AwayGoals == result.HomeGoals ? 1 : 0),
            Losses = away.Losses + (result.AwayGoals < result.HomeGoals ? 1 : 0),
            Points = away.Points + Points(result.AwayGoals, result.HomeGoals),
        };
    }

    private static int Points(int our, int their)
    {
        if (our > their) return 3;
        if (our == their) return 1;
        return 0;
    }
}

public sealed record SeasonResult(
    string CompetitionExternalId,
    string CompetitionDisplayName,
    ImmutableArray<StandingRow> Standings,
    ImmutableArray<SeasonFixture> Fixtures);

public sealed record SeasonFixture(
    string HomeClubId,
    string AwayClubId,
    string HomeDisplayName,
    string AwayDisplayName,
    int HomeGoals,
    int AwayGoals,
    long Seed,
    ImmutableArray<MatchEvent> GoalEvents);
