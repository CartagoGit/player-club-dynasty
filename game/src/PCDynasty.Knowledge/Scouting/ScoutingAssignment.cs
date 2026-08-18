using System.Collections.Immutable;
using PCDynasty.Core.Domain;
using PCDynasty.Knowledge.Staff;

namespace PCDynasty.Knowledge.Scouting;

public sealed class ScoutingAssignment
{
    public ScoutingAssignment(
        string assignmentId,
        string staffId,
        KnowledgeDomain domain,
        IEnumerable<PersonId> subjects,
        DateOnly commissionedOn,
        DateOnly dueOn)
    {
        AssignmentId = Required(assignmentId, nameof(assignmentId));
        StaffId = Required(staffId, nameof(staffId));
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        ArgumentOutOfRangeException.ThrowIfEqual(
            commissionedOn,
            DateOnly.MinValue,
            nameof(commissionedOn));

        if (dueOn < commissionedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dueOn),
                dueOn,
                "An assignment cannot be due before it is commissioned.");
        }

        ArgumentNullException.ThrowIfNull(subjects);
        ImmutableArray<PersonId> resolvedSubjects = subjects.ToImmutableArray();
        if (resolvedSubjects.IsEmpty || resolvedSubjects.Any(subject => subject.Value <= 0))
        {
            throw new ArgumentException(
                "An assignment needs at least one identified subject.",
                nameof(subjects));
        }

        if (resolvedSubjects.Distinct().Count() != resolvedSubjects.Length)
        {
            throw new ArgumentException(
                "An assignment cannot contain duplicate subjects.",
                nameof(subjects));
        }

        Subjects = resolvedSubjects
            .OrderBy(subject => subject.Value)
            .ToImmutableArray();
        CommissionedOn = commissionedOn;
        DueOn = dueOn;
    }

    public string AssignmentId { get; }

    public string StaffId { get; }

    public KnowledgeDomain Domain { get; }

    public ImmutableArray<PersonId> Subjects { get; }

    public DateOnly CommissionedOn { get; }

    public DateOnly DueOn { get; }

    public bool Includes(PersonId subjectId) => Subjects.Contains(subjectId);

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty identifier is required.", parameterName);
    }
}
