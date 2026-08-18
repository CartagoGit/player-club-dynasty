using System.Collections.Immutable;

namespace PCDynasty.Editor.Competitions;

public sealed class CompetitionGraphEditor
{
    public CompetitionGraphEditor(
        string competitionId,
        string entryStageId,
        IEnumerable<EditableCompetitionStage> stages,
        IEnumerable<EditableCompetitionTransition> transitions)
    {
        ArgumentNullException.ThrowIfNull(stages);
        ArgumentNullException.ThrowIfNull(transitions);
        CompetitionId = competitionId;
        EntryStageId = entryStageId;
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
        return Copy(stages: next, transitions: Transitions);
    }

    public CompetitionGraphEditor UpsertTransition(EditableCompetitionTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        int index = FindTransitionIndex(transition.TransitionId);
        ImmutableArray<EditableCompetitionTransition> next = index < 0
            ? Transitions.Add(transition)
            : Transitions.SetItem(index, transition);
        return Copy(stages: Stages, transitions: next);
    }

    public CompetitionValidationReport ValidateAndBuild(string providerId, int schemaVersion)
    {
        ImmutableArray<CompetitionEditorDiagnostic> diagnostics =
            CompetitionGraphValidator.Validate(this, providerId, schemaVersion);
        return diagnostics.IsEmpty
            ? new CompetitionValidationReport(
                CompetitionPackBuilder.Build(this, providerId.Trim(), schemaVersion),
                diagnostics)
            : new CompetitionValidationReport(null, diagnostics);
    }

    private CompetitionGraphEditor Copy(
        ImmutableArray<EditableCompetitionStage> stages,
        ImmutableArray<EditableCompetitionTransition> transitions) =>
        new(CompetitionId, EntryStageId, stages, transitions);

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
            if (string.Equals(Transitions[index].TransitionId, transitionId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
