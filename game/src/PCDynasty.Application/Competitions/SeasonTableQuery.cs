using System.Collections.Immutable;
using PCDynasty.Competition.Outcomes;
using PCDynasty.Competition.Rankings;

namespace PCDynasty.Application.Competitions;

public static class SeasonTableQuery
{
    public static SeasonTableView Execute(
        RankingTable table,
        IEnumerable<QualificationOutcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(outcomes);

        var outcomesByParticipant = new Dictionary<string, QualificationOutcome>(StringComparer.Ordinal);
        foreach (QualificationOutcome outcome in outcomes)
        {
            ArgumentNullException.ThrowIfNull(outcome);
            if (!outcomesByParticipant.TryAdd(outcome.ParticipantId, outcome))
            {
                throw new ArgumentException(
                    "Each ranked participant must have exactly one outcome.",
                    nameof(outcomes));
            }
        }

        if (outcomesByParticipant.Count != table.Entries.Length)
        {
            throw new ArgumentException(
                "The outcome set must cover every ranked participant.",
                nameof(outcomes));
        }

        var entries = ImmutableArray.CreateBuilder<SeasonTableEntryView>(table.Entries.Length);
        foreach (RankingEntry entry in table.Entries)
        {
            if (!outcomesByParticipant.TryGetValue(entry.ParticipantId, out QualificationOutcome? outcome))
            {
                throw new ArgumentException(
                    "The outcome set must cover every ranked participant.",
                    nameof(outcomes));
            }

            entries.Add(new SeasonTableEntryView(
                entry.Rank,
                entry.ParticipantId,
                entry.Criteria,
                outcome.OutcomeId,
                outcome.DestinationStageId));
        }

        return new SeasonTableView(entries.ToImmutable());
    }
}
