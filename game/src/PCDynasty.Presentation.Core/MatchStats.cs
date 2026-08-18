using PCDynasty.Application.Competitions.Simulation;

namespace PCDynasty.Presentation.Core;

/// <summary>
/// Per-side aggregate stats for a simulated match: shots, saves, misses,
/// cards, possession approximation, and final score.
/// </summary>
/// <remarks>
/// "Possession" here is approximated by the share of attempts: it's a
/// quick visual hint, not a real-world possession model. Phase 5 will
/// swap it for a proper minute-by-minute possession derivation.
/// </remarks>
public static class MatchStats
{
    public static MatchStatsRow Compute(MatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        int homeShots = 0, homeSaves = 0, homeMisses = 0, homeCards = 0;
        int awayShots = 0, awaySaves = 0, awayMisses = 0, awayCards = 0;

        foreach (var evt in result.Events)
        {
            if (evt.IsHomeActor)
            {
                Increment(evt.Kind, ref homeShots, ref homeSaves, ref homeMisses, ref homeCards);
            }
            else
            {
                Increment(evt.Kind, ref awayShots, ref awaySaves, ref awayMisses, ref awayCards);
            }
        }

        int totalAttempts = homeShots + homeSaves + homeMisses + homeCards + awayShots + awaySaves + awayMisses + awayCards;
        int homeAttempts = homeShots + homeSaves + homeMisses + homeCards;
        int homePossession = totalAttempts == 0 ? 50 : (int)Math.Round(homeAttempts * 100.0 / totalAttempts);
        int awayPossession = 100 - homePossession;

        return new MatchStatsRow(
            result.HomeClub.ShortName,
            result.AwayClub.ShortName,
            HomeGoals: result.HomeGoals,
            AwayGoals: result.AwayGoals,
            HomeShots: homeShots,
            AwayShots: awayShots,
            HomeSaves: homeSaves,
            AwaySaves: awaySaves,
            HomeMisses: homeMisses,
            AwayMisses: awayMisses,
            HomeCards: homeCards,
            AwayCards: awayCards,
            HomePossession: homePossession,
            AwayPossession: awayPossession);
    }

    private static void Increment(MatchEventKind kind, ref int shots, ref int saves, ref int misses, ref int cards)
    {
        switch (kind)
        {
            case MatchEventKind.Goal:
                // Goals are also counted as shots on target.
                shots++;
                break;
            case MatchEventKind.Shot:
                shots++;
                break;
            case MatchEventKind.Save:
                saves++;
                break;
            case MatchEventKind.Miss:
                misses++;
                break;
            case MatchEventKind.Card:
                cards++;
                break;
        }
    }
}

public sealed record MatchStatsRow(
    string HomeShortName,
    string AwayShortName,
    int HomeGoals,
    int AwayGoals,
    int HomeShots,
    int AwayShots,
    int HomeSaves,
    int AwaySaves,
    int HomeMisses,
    int AwayMisses,
    int HomeCards,
    int AwayCards,
    int HomePossession,
    int AwayPossession);
