using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Competitions.Simulation;

public interface ICompetitionSimulationService
{
    MatchExecution PlayMatch(string competitionDisplayName, ClubRow home, ClubRow away);

    MatchResult Replay(MatchResult previous);

    SeasonExecution PlaySeason(
        string competitionExternalId,
        string competitionDisplayName,
        ClubRow managedClub,
        IReadOnlyList<ClubRow> participants);
}

public sealed record MatchExecution(MatchResult Result, HistoryEntry History);

public sealed record SeasonExecution(
    SeasonResult Result,
    ImmutableArray<HistoryEntry> MatchHistory,
    SeasonHistoryEntry SeasonHistory);
