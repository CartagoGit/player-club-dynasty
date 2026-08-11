using PCDynasty.Competition.Rankings;

namespace PCDynasty.Competition.Outcomes;

public sealed class QualificationOutcome
{
    public QualificationOutcome(
        string participantId,
        string outcomeId,
        string? destinationStageId)
    {
        ParticipantId = CompetitionValue.NormalizeIdentifier(participantId, nameof(participantId));
        OutcomeId = CompetitionValue.NormalizeIdentifier(outcomeId, nameof(outcomeId));
        DestinationStageId = destinationStageId is null
            ? null
            : CompetitionValue.NormalizeIdentifier(destinationStageId, nameof(destinationStageId));
    }

    public string ParticipantId { get; }
    public string OutcomeId { get; }
    public string? DestinationStageId { get; }
}
