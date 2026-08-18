using System.Collections.Immutable;

namespace PCDynasty.Application.Competitions;

public sealed class SeasonTableView
{
    internal SeasonTableView(ImmutableArray<SeasonTableEntryView> entries)
    {
        Entries = entries;
    }

    public ImmutableArray<SeasonTableEntryView> Entries { get; }
}

public sealed class SeasonTableEntryView
{
    internal SeasonTableEntryView(
        int rank,
        string participantId,
        ImmutableArray<long> criteria,
        string outcomeId,
        string? destinationStageId)
    {
        Rank = rank;
        ParticipantId = participantId;
        Criteria = criteria;
        OutcomeId = outcomeId;
        DestinationStageId = destinationStageId;
    }

    public int Rank { get; }
    public string ParticipantId { get; }
    public ImmutableArray<long> Criteria { get; }
    public string OutcomeId { get; }
    public string? DestinationStageId { get; }
}
