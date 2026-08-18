namespace PCDynasty.Editor.Competitions;

public sealed record EditableCompetitionTransition(
    string TransitionId,
    string FromStageId,
    string ToStageId,
    string OutcomeId);
