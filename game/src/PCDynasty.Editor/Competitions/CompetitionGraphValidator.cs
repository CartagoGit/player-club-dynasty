using System.Collections.Immutable;

namespace PCDynasty.Editor.Competitions;

internal static class CompetitionGraphValidator
{
    internal static ImmutableArray<CompetitionEditorDiagnostic> Validate(
        CompetitionGraphEditor graph,
        string providerId,
        int schemaVersion)
    {
        var diagnostics = ImmutableArray.CreateBuilder<CompetitionEditorDiagnostic>();
        AddRequiredMetadataDiagnostics(graph, diagnostics);
        Dictionary<string, EditableCompetitionStage> stages = ValidateStages(graph, diagnostics);
        ValidateTransitions(graph, stages, diagnostics);
        string entryStageId = graph.EntryStageId?.Trim() ?? string.Empty;
        if (Required(graph.EntryStageId) && !stages.ContainsKey(entryStageId))
        {
            Add(diagnostics, "editor.competition.entry-missing", "The entry stage does not exist.", graph.EntryStageId);
        }
        else if (stages.Count > 0 && Required(graph.EntryStageId))
        {
            ValidateReachability(graph, stages, entryStageId, diagnostics);
            ValidateAcyclic(graph, stages, diagnostics);
        }
        if (!Required(providerId) || schemaVersion <= 0)
        {
            Add(diagnostics, "editor.competition.pack-metadata-invalid", "Valid provider and schema metadata are required.");
        }
        return diagnostics.ToImmutable();
    }

    private static void AddRequiredMetadataDiagnostics(
        CompetitionGraphEditor graph,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        if (!Required(graph.CompetitionId))
        {
            Add(diagnostics, "editor.competition.id-required", "A competition identifier is required.");
        }
        if (!Required(graph.EntryStageId))
        {
            Add(diagnostics, "editor.competition.entry-required", "An entry stage is required.");
        }
    }

    private static Dictionary<string, EditableCompetitionStage> ValidateStages(
        CompetitionGraphEditor graph,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        var stages = new Dictionary<string, EditableCompetitionStage>(StringComparer.Ordinal);
        foreach (EditableCompetitionStage stage in graph.Stages)
        {
            if (!Required(stage.StageId))
            {
                Add(diagnostics, "editor.competition.stage-id-required", "Every stage needs an identifier.");
                continue;
            }
            string stageId = stage.StageId.Trim();
            if (!stages.TryAdd(stageId, stage))
            {
                Add(diagnostics, "editor.competition.duplicate-stage", $"Stage '{stageId}' is duplicated.", stageId);
            }
            if (stage.MinimumParticipants <= 0)
            {
                Add(diagnostics, "editor.competition.invalid-participants", "Minimum participants must be greater than zero.", stageId);
            }
        }
        if (stages.Count == 0)
        {
            Add(diagnostics, "editor.competition.stage-required", "A competition needs at least one stage.");
        }
        return stages;
    }

    private static void ValidateTransitions(
        CompetitionGraphEditor graph,
        Dictionary<string, EditableCompetitionStage> stages,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        var transitionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (EditableCompetitionTransition transition in graph.Transitions)
        {
            if (!Required(transition.TransitionId) || !Required(transition.FromStageId)
                || !Required(transition.ToStageId) || !Required(transition.OutcomeId))
            {
                Add(diagnostics, "editor.competition.transition-invalid", "Transition identifiers and outcome are required.");
                continue;
            }
            string transitionId = transition.TransitionId.Trim();
            if (!transitionIds.Add(transitionId))
            {
                Add(diagnostics, "editor.competition.duplicate-transition", $"Transition '{transitionId}' is duplicated.", transitionId);
            }
            if (!stages.ContainsKey(transition.FromStageId.Trim())
                || !stages.ContainsKey(transition.ToStageId.Trim()))
            {
                Add(diagnostics, "editor.competition.unknown-stage", "A transition references an unknown stage.", transitionId);
            }
        }
    }

    private static void ValidateReachability(
        CompetitionGraphEditor graph,
        Dictionary<string, EditableCompetitionStage> stages,
        string entryStageId,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        Dictionary<string, string[]> destinations = Destinations(graph, stages.Keys);
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(entryStageId);
        while (pending.TryPop(out string? stageId))
        {
            if (reachable.Add(stageId))
            {
                foreach (string destination in destinations[stageId])
                {
                    pending.Push(destination);
                }
            }
        }
        foreach (string stageId in stages.Keys.Except(reachable, StringComparer.Ordinal))
        {
            Add(diagnostics, "editor.competition.unreachable-stage", $"Stage '{stageId}' is unreachable from the entry stage.", stageId);
        }
    }

    private static void ValidateAcyclic(
        CompetitionGraphEditor graph,
        Dictionary<string, EditableCompetitionStage> stages,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        Dictionary<string, string[]> destinations = Destinations(graph, stages.Keys);
        var inDegree = stages.Keys.ToDictionary(key => key, _ => 0, StringComparer.Ordinal);
        foreach (string destination in destinations.Values.SelectMany(value => value))
        {
            inDegree[destination]++;
        }
        var pending = new Queue<string>(inDegree.Where(pair => pair.Value == 0).Select(pair => pair.Key));
        int visited = 0;
        while (pending.TryDequeue(out string? stageId))
        {
            visited++;
            foreach (string destination in destinations[stageId])
            {
                if (--inDegree[destination] == 0)
                {
                    pending.Enqueue(destination);
                }
            }
        }
        if (visited != stages.Count)
        {
            Add(diagnostics, "editor.competition.cycle", "Competition stage graphs cannot contain cycles.");
        }
    }

    private static Dictionary<string, string[]> Destinations(
        CompetitionGraphEditor graph,
        IEnumerable<string> stageIds) =>
        stageIds.ToDictionary(
            stageId => stageId,
            stageId => graph.Transitions
                .Where(transition => Required(transition.FromStageId)
                    && Required(transition.ToStageId)
                    && transition.FromStageId.Trim() == stageId)
                .Select(transition => transition.ToStageId.Trim())
                .Where(destination => graph.Stages.Any(stage =>
                    Required(stage.StageId) && stage.StageId.Trim() == destination))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);

    private static bool Required(string? value) => !string.IsNullOrWhiteSpace(value);

    private static void Add(ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics,
        string code, string message, string? elementId = null) =>
        diagnostics.Add(new CompetitionEditorDiagnostic(code, message, elementId));
}
