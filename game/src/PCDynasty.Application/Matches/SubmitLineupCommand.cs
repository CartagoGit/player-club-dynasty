using System.Collections.Immutable;

namespace PCDynasty.Application.Matches;

public sealed class LineupParticipantSelection
{
    public LineupParticipantSelection(
        string participantId,
        string assignmentId,
        int order)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(order);
        ParticipantId = LineupCommandValue.Required(participantId, nameof(participantId));
        AssignmentId = LineupCommandValue.Required(assignmentId, nameof(assignmentId));
        Order = order;
    }

    public string ParticipantId { get; }
    public string AssignmentId { get; }
    public int Order { get; }
}

public sealed class SubmitLineupCommand
{
    public SubmitLineupCommand(
        string contestId,
        string controlledEntityId,
        IEnumerable<LineupParticipantSelection> selections,
        IEnumerable<KeyValuePair<string, string>> options)
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentNullException.ThrowIfNull(options);

        LineupParticipantSelection[] ordered = selections.Select(selection =>
        {
            ArgumentNullException.ThrowIfNull(selection);
            return selection;
        }).OrderBy(selection => selection.Order).ToArray();
        if (ordered.Length == 0)
        {
            throw new ArgumentException("At least one participant selection is required.", nameof(selections));
        }

        if (ordered.Select(selection => selection.ParticipantId)
                .Distinct(StringComparer.Ordinal).Count() != ordered.Length ||
            ordered.Select(selection => selection.Order).Distinct().Count() != ordered.Length)
        {
            throw new ArgumentException(
                "Participant identifiers and ordering values must be unique.",
                nameof(selections));
        }

        var copiedOptions = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in options)
        {
            string normalizedKey = LineupCommandValue.Required(key, nameof(options));
            ArgumentNullException.ThrowIfNull(value);
            if (!copiedOptions.TryAdd(normalizedKey, value))
            {
                throw new ArgumentException("Option identifiers must be unique.", nameof(options));
            }
        }

        ContestId = LineupCommandValue.Required(contestId, nameof(contestId));
        ControlledEntityId = LineupCommandValue.Required(
            controlledEntityId,
            nameof(controlledEntityId));
        Selections = ordered.ToImmutableArray();
        Options = copiedOptions.ToImmutable();
    }

    public string ContestId { get; }
    public string ControlledEntityId { get; }
    public ImmutableArray<LineupParticipantSelection> Selections { get; }
    public ImmutableDictionary<string, string> Options { get; }
}

internal static class LineupCommandValue
{
    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
