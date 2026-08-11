using System.Collections.Immutable;
using PCDynasty.Application.Matches;

namespace PCDynasty.Presentation.Matches;

public sealed class LineupCandidateView
{
    public LineupCandidateView(
        string participantId,
        string displayName,
        string suggestedAssignmentId)
    {
        ParticipantId = Required(participantId, nameof(participantId));
        DisplayName = Required(displayName, nameof(displayName));
        SuggestedAssignmentId = suggestedAssignmentId?.Trim() ??
            throw new ArgumentNullException(nameof(suggestedAssignmentId));
    }

    public string ParticipantId { get; }
    public string DisplayName { get; }
    public string SuggestedAssignmentId { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public sealed class LineupSelectionView
{
    public LineupSelectionView(
        string contestId,
        string controlledEntityId,
        string title,
        IEnumerable<LineupCandidateView> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        LineupCandidateView[] copiedCandidates = candidates.Select(candidate =>
        {
            ArgumentNullException.ThrowIfNull(candidate);
            return candidate;
        }).ToArray();
        if (copiedCandidates.Select(candidate => candidate.ParticipantId)
                .Distinct(StringComparer.Ordinal).Count() != copiedCandidates.Length)
        {
            throw new ArgumentException("Candidate identifiers must be unique.", nameof(candidates));
        }

        ContestId = Required(contestId, nameof(contestId));
        ControlledEntityId = Required(controlledEntityId, nameof(controlledEntityId));
        Title = Required(title, nameof(title));
        Candidates = copiedCandidates.ToImmutableArray();
    }

    public string ContestId { get; }
    public string ControlledEntityId { get; }
    public string Title { get; }
    public ImmutableArray<LineupCandidateView> Candidates { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}

public sealed record LineupDraftSelection(
    string ParticipantId,
    string AssignmentId,
    int Order);

public static class LineupCommandMapper
{
    public static SubmitLineupCommand Map(
        LineupSelectionView source,
        IEnumerable<LineupDraftSelection> selections,
        IEnumerable<KeyValuePair<string, string>> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentNullException.ThrowIfNull(options);
        LineupParticipantSelection[] mapped = selections.Select(selection =>
        {
            ArgumentNullException.ThrowIfNull(selection);
            return new LineupParticipantSelection(
                selection.ParticipantId,
                selection.AssignmentId,
                selection.Order);
        }).ToArray();

        return new SubmitLineupCommand(
            source.ContestId,
            source.ControlledEntityId,
            mapped,
            options);
    }
}
