using System.Collections.Immutable;
using PCDynasty.Editor.Documents;

namespace PCDynasty.Editor.Competitions;

public sealed record EditableCompetitionStage(
    string StageId,
    int MinimumParticipants);

public sealed record EditableCompetitionTransition(
    string TransitionId,
    string FromStageId,
    string ToStageId,
    string OutcomeId);

public sealed class CompetitionGraphEditor
{
    public CompetitionGraphEditor(
        string competitionId,
        string entryStageId,
        IEnumerable<EditableCompetitionStage> stages,
        IEnumerable<EditableCompetitionTransition> transitions)
    {
        CompetitionId = competitionId;
        EntryStageId = entryStageId;
        ArgumentNullException.ThrowIfNull(stages);
        ArgumentNullException.ThrowIfNull(transitions);
        Stages = stages.ToImmutableArray();
        Transitions = transitions.ToImmutableArray();
    }

    public string CompetitionId { get; }

    public string EntryStageId { get; }

    public ImmutableArray<EditableCompetitionStage> Stages { get; }

    public ImmutableArray<EditableCompetitionTransition> Transitions { get; }

    public CompetitionGraphEditor UpsertStage(EditableCompetitionStage stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        int index = FindStageIndex(stage.StageId);
        ImmutableArray<EditableCompetitionStage> next = index < 0
            ? Stages.Add(stage)
            : Stages.SetItem(index, stage);
        return new CompetitionGraphEditor(
            CompetitionId,
            EntryStageId,
            next,
            Transitions);
    }

    public CompetitionGraphEditor UpsertTransition(EditableCompetitionTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        int index = FindTransitionIndex(transition.TransitionId);
        ImmutableArray<EditableCompetitionTransition> next = index < 0
            ? Transitions.Add(transition)
            : Transitions.SetItem(index, transition);
        return new CompetitionGraphEditor(
            CompetitionId,
            EntryStageId,
            Stages,
            next);
    }

    public CompetitionValidationReport ValidateAndBuild(
        string providerId,
        int schemaVersion)
    {
        var diagnostics = ImmutableArray.CreateBuilder<CompetitionEditorDiagnostic>();
        if (!Required(CompetitionId))
        {
            diagnostics.Add(new(
                "editor.competition.id-required",
                "A competition identifier is required."));
        }

        if (!Required(EntryStageId))
        {
            diagnostics.Add(new(
                "editor.competition.entry-required",
                "An entry stage is required."));
        }

        Dictionary<string, EditableCompetitionStage> stages = ValidateStages(diagnostics);
        ValidateTransitions(stages, diagnostics);
        if (Required(EntryStageId) && !stages.ContainsKey(EntryStageId.Trim()))
        {
            diagnostics.Add(new(
                "editor.competition.entry-missing",
                "The entry stage does not exist.",
                EntryStageId));
        }

        if (stages.Count > 0 && Required(EntryStageId) && stages.ContainsKey(EntryStageId.Trim()))
        {
            ValidateReachability(stages, diagnostics);
            ValidateAcyclic(stages, diagnostics);
        }

        if (!Required(providerId) || schemaVersion <= 0)
        {
            diagnostics.Add(new(
                "editor.competition.pack-metadata-invalid",
                "Valid provider and schema metadata are required."));
        }

        if (diagnostics.Count > 0)
        {
            return new CompetitionValidationReport(null, diagnostics.ToImmutable());
        }

        return new CompetitionValidationReport(
            BuildPack(providerId.Trim(), schemaVersion),
            []);
    }

    private Dictionary<string, EditableCompetitionStage> ValidateStages(
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        var index = new Dictionary<string, EditableCompetitionStage>(StringComparer.Ordinal);
        foreach (EditableCompetitionStage stage in Stages)
        {
            if (!Required(stage.StageId))
            {
                diagnostics.Add(new(
                    "editor.competition.stage-id-required",
                    "Every stage needs an identifier."));
                continue;
            }

            string stageId = stage.StageId.Trim();
            if (!index.TryAdd(stageId, stage))
            {
                diagnostics.Add(new(
                    "editor.competition.duplicate-stage",
                    $"Stage '{stageId}' is duplicated.",
                    stageId));
            }

            if (stage.MinimumParticipants <= 0)
            {
                diagnostics.Add(new(
                    "editor.competition.invalid-participants",
                    "Minimum participants must be greater than zero.",
                    stageId));
            }
        }

        if (index.Count == 0)
        {
            diagnostics.Add(new(
                "editor.competition.stage-required",
                "A competition needs at least one stage."));
        }

        return index;
    }

    private void ValidateTransitions(
        Dictionary<string, EditableCompetitionStage> stages,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        var transitionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (EditableCompetitionTransition transition in Transitions)
        {
            if (!Required(transition.TransitionId) ||
                !Required(transition.FromStageId) ||
                !Required(transition.ToStageId) ||
                !Required(transition.OutcomeId))
            {
                diagnostics.Add(new(
                    "editor.competition.transition-invalid",
                    "Transition identifiers and outcome are required."));
                continue;
            }

            string transitionId = transition.TransitionId.Trim();
            if (!transitionIds.Add(transitionId))
            {
                diagnostics.Add(new(
                    "editor.competition.duplicate-transition",
                    $"Transition '{transitionId}' is duplicated.",
                    transitionId));
            }

            if (!stages.ContainsKey(transition.FromStageId.Trim()) ||
                !stages.ContainsKey(transition.ToStageId.Trim()))
            {
                diagnostics.Add(new(
                    "editor.competition.unknown-stage",
                    "A transition references an unknown stage.",
                    transitionId));
            }
        }
    }

    private void ValidateReachability(
        IReadOnlyDictionary<string, EditableCompetitionStage> stages,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        Dictionary<string, string[]> destinations = Destinations(stages.Keys);
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(EntryStageId.Trim());
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

        foreach (string stageId in stages.Keys.Except(reachable, StringComparer.Ordinal))
        {
            diagnostics.Add(new(
                "editor.competition.unreachable-stage",
                $"Stage '{stageId}' is unreachable from the entry stage.",
                stageId));
        }
    }

    private void ValidateAcyclic(
        IReadOnlyDictionary<string, EditableCompetitionStage> stages,
        ImmutableArray<CompetitionEditorDiagnostic>.Builder diagnostics)
    {
        Dictionary<string, string[]> destinations = Destinations(stages.Keys);
        var inDegree = stages.Keys.ToDictionary(key => key, _ => 0, StringComparer.Ordinal);
        foreach (string destination in destinations.Values.SelectMany(value => value))
        {
            inDegree[destination]++;
        }

        var pending = new Queue<string>(inDegree
            .Where(pair => pair.Value == 0)
            .Select(pair => pair.Key));
        int visited = 0;
        while (pending.TryDequeue(out string? stageId))
        {
            visited++;
            foreach (string destination in destinations[stageId])
            {
                inDegree[destination]--;
                if (inDegree[destination] == 0)
                {
                    pending.Enqueue(destination);
                }
            }
        }

        if (visited != stages.Count)
        {
            diagnostics.Add(new(
                "editor.competition.cycle",
                "Competition stage graphs cannot contain cycles."));
        }
    }

    private Dictionary<string, string[]> Destinations(IEnumerable<string> stageIds) =>
        stageIds.ToDictionary(
            stageId => stageId,
            stageId => Transitions
                .Where(transition =>
                    Required(transition.FromStageId) &&
                    Required(transition.ToStageId) &&
                    string.Equals(
                        transition.FromStageId.Trim(),
                        stageId,
                        StringComparison.Ordinal))
                .Select(transition => transition.ToStageId.Trim())
                .Where(destination => Stages.Any(stage =>
                    Required(stage.StageId) &&
                    string.Equals(stage.StageId.Trim(), destination, StringComparison.Ordinal)))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);

    private PackDocument BuildPack(string providerId, int schemaVersion)
    {
        var entities = ImmutableArray.CreateBuilder<PackEntityDocument>();
        entities.Add(new PackEntityDocument(
            CompetitionId.Trim(),
            "competition",
            [new PackValueDocument("entry-stage", EntryStageId.Trim())],
            [new PackReferenceDocument("entry-stage", EntryStageId.Trim(), true)]));
        entities.AddRange(Stages.Select(stage => new PackEntityDocument(
            stage.StageId.Trim(),
            "competition-stage",
            [new PackValueDocument(
                "minimum-participants",
                stage.MinimumParticipants.ToString(System.Globalization.CultureInfo.InvariantCulture))],
            [new PackReferenceDocument("dependency:competition", CompetitionId.Trim(), true)])));
        entities.AddRange(Transitions.Select(transition => new PackEntityDocument(
            transition.TransitionId.Trim(),
            "competition-transition",
            [new PackValueDocument("outcome-id", transition.OutcomeId.Trim())],
            [
                new PackReferenceDocument("from-stage", transition.FromStageId.Trim(), true),
                new PackReferenceDocument("to-stage", transition.ToStageId.Trim(), true),
            ])));
        return new PackDocument(providerId, schemaVersion, entities);
    }

    private int FindStageIndex(string stageId)
    {
        for (int index = 0; index < Stages.Length; index++)
        {
            if (string.Equals(Stages[index].StageId, stageId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private int FindTransitionIndex(string transitionId)
    {
        for (int index = 0; index < Transitions.Length; index++)
        {
            if (string.Equals(
                    Transitions[index].TransitionId,
                    transitionId,
                    StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool Required(string? value) => !string.IsNullOrWhiteSpace(value);
}
