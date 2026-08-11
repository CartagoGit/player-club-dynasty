using System.Collections.Immutable;

namespace PCDynasty.Competition.Stages;

public sealed class CompetitionStageDefinition
{
    public CompetitionStageDefinition(string stageId, int minimumParticipants)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumParticipants);

        StageId = CompetitionIdentifier.Normalize(stageId, nameof(stageId));
        MinimumParticipants = minimumParticipants;
    }

    public string StageId { get; }
    public int MinimumParticipants { get; }
}

public sealed class CompetitionTransition
{
    public CompetitionTransition(string fromStageId, string toStageId, string outcomeId)
    {
        FromStageId = CompetitionIdentifier.Normalize(fromStageId, nameof(fromStageId));
        ToStageId = CompetitionIdentifier.Normalize(toStageId, nameof(toStageId));
        OutcomeId = CompetitionIdentifier.Normalize(outcomeId, nameof(outcomeId));
    }

    public string FromStageId { get; }
    public string ToStageId { get; }
    public string OutcomeId { get; }
}

public sealed class CompetitionGraph
{
    public CompetitionGraph(
        string entryStageId,
        IEnumerable<CompetitionStageDefinition> stages,
        IEnumerable<CompetitionTransition> transitions)
    {
        ArgumentNullException.ThrowIfNull(stages);
        ArgumentNullException.ThrowIfNull(transitions);

        ImmutableArray<CompetitionStageDefinition> stageCopy = stages.ToImmutableArray();
        ImmutableArray<CompetitionTransition> transitionCopy = transitions.ToImmutableArray();
        if (stageCopy.IsEmpty)
        {
            throw new ArgumentException("A competition graph requires at least one stage.", nameof(stages));
        }

        EntryStageId = CompetitionIdentifier.Normalize(entryStageId, nameof(entryStageId));
        Validate(stageCopy, transitionCopy, EntryStageId);
        Stages = stageCopy;
        Transitions = transitionCopy;
    }

    public string EntryStageId { get; }
    public ImmutableArray<CompetitionStageDefinition> Stages { get; }
    public ImmutableArray<CompetitionTransition> Transitions { get; }

    private static void Validate(
        ImmutableArray<CompetitionStageDefinition> stages,
        ImmutableArray<CompetitionTransition> transitions,
        string entryStageId)
    {
        Dictionary<string, CompetitionStageDefinition> stagesById;
        try
        {
            stagesById = stages.ToDictionary(stage => stage.StageId, StringComparer.Ordinal);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException("Stage identifiers must be unique.", nameof(stages), exception);
        }

        if (!stagesById.ContainsKey(entryStageId))
        {
            throw new ArgumentException("The entry stage must exist in the graph.", nameof(entryStageId));
        }

        foreach (CompetitionTransition transition in transitions)
        {
            if (!stagesById.ContainsKey(transition.FromStageId) ||
                !stagesById.ContainsKey(transition.ToStageId))
            {
                throw new ArgumentException("Every transition must reference known stages.", nameof(transitions));
            }
        }

        Dictionary<string, string[]> destinations = stagesById.Keys.ToDictionary(
            stageId => stageId,
            stageId => transitions
                .Where(transition => transition.FromStageId == stageId)
                .Select(transition => transition.ToStageId)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(entryStageId);
        while (pending.TryPop(out string? stageId))
        {
            if (!reachable.Add(stageId))
            {
                continue;
            }

            foreach (string destination in destinations[stageId])
            {
                pending.Push(destination);
            }
        }

        if (reachable.Count != stages.Length)
        {
            throw new ArgumentException("Every stage must be reachable from the entry stage.", nameof(stages));
        }

        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        if (HasCycle(entryStageId, destinations, visiting, visited))
        {
            throw new ArgumentException("Competition stage graphs cannot contain cycles.", nameof(transitions));
        }
    }

    private static bool HasCycle(
        string stageId,
        IReadOnlyDictionary<string, string[]> destinations,
        ISet<string> visiting,
        ISet<string> visited)
    {
        if (visiting.Contains(stageId))
        {
            return true;
        }

        if (!visited.Add(stageId))
        {
            return false;
        }

        visiting.Add(stageId);
        foreach (string destination in destinations[stageId])
        {
            if (HasCycle(destination, destinations, visiting, visited))
            {
                return true;
            }
        }

        visiting.Remove(stageId);
        return false;
    }
}

internal static class CompetitionIdentifier
{
    public static string Normalize(string identifier, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifier, parameterName);
        string normalized = identifier.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("An identifier is required.", parameterName);
    }
}
