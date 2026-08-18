using System.Collections.Immutable;

namespace PCDynasty.Contracts.Careers;

public sealed record LineupAssignment
{
    public LineupAssignment(string participantId, string assignmentId, int order)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(order);
        ParticipantId = CareerContractValue.Required(participantId, nameof(participantId));
        AssignmentId = CareerContractValue.Required(assignmentId, nameof(assignmentId));
        Order = order;
    }

    public string ParticipantId { get; }
    public string AssignmentId { get; }
    public int Order { get; }
}

public sealed class LineupPlan
{
    public LineupPlan(
        string clubExternalId,
        string contestId,
        DateTimeOffset submittedAtUtc,
        IEnumerable<LineupAssignment> assignments,
        IEnumerable<KeyValuePair<string, string>> options)
    {
        ArgumentNullException.ThrowIfNull(assignments);
        ArgumentNullException.ThrowIfNull(options);
        LineupAssignment[] ordered = assignments
            .Select(assignment => assignment ?? throw new ArgumentException(
                "Assignments cannot contain null values.",
                nameof(assignments)))
            .OrderBy(assignment => assignment.Order)
            .ToArray();
        if (ordered.Length == 0)
        {
            throw new ArgumentException("At least one lineup assignment is required.", nameof(assignments));
        }

        if (ordered.Select(item => item.ParticipantId).Distinct(StringComparer.Ordinal).Count() != ordered.Length
            || ordered.Select(item => item.Order).Distinct().Count() != ordered.Length)
        {
            throw new ArgumentException(
                "Participant identifiers and ordering values must be unique.",
                nameof(assignments));
        }

        var copiedOptions = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in options)
        {
            string normalizedKey = CareerContractValue.Required(key, nameof(options));
            string normalizedValue = CareerContractValue.Required(value, nameof(options));
            if (!copiedOptions.TryAdd(normalizedKey, normalizedValue))
            {
                throw new ArgumentException("Option identifiers must be unique.", nameof(options));
            }
        }

        ClubExternalId = CareerContractValue.Required(clubExternalId, nameof(clubExternalId));
        ContestId = CareerContractValue.Required(contestId, nameof(contestId));
        SubmittedAtUtc = submittedAtUtc.ToUniversalTime();
        Assignments = ordered.ToImmutableArray();
        Options = copiedOptions.ToImmutable();
    }

    public string ClubExternalId { get; }
    public string ContestId { get; }
    public DateTimeOffset SubmittedAtUtc { get; }
    public ImmutableArray<LineupAssignment> Assignments { get; }
    public ImmutableDictionary<string, string> Options { get; }
}
