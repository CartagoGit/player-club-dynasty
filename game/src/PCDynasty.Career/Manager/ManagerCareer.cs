using System.Collections.Immutable;

namespace PCDynasty.Career.Manager;

public enum ManagerCareerFactKind
{
    Appointment,
    Departure,
    ObjectiveOutcome,
    Achievement,
    Conduct,
}

public sealed record ManagerCareerFact
{
    public ManagerCareerFact(
        string managerId,
        long sequence,
        DateOnly occurredOn,
        ManagerCareerFactKind kind,
        int reputationChange,
        string sourceId,
        string description)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        ArgumentOutOfRangeException.ThrowIfEqual(
            occurredOn,
            DateOnly.MinValue,
            nameof(occurredOn));
        if (reputationChange is < -100 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(reputationChange));
        }

        ManagerId = CareerValue.Required(managerId, nameof(managerId));
        Sequence = sequence;
        OccurredOn = occurredOn;
        Kind = kind;
        ReputationChange = reputationChange;
        SourceId = CareerValue.Required(sourceId, nameof(sourceId));
        Description = CareerValue.Required(description, nameof(description));
    }

    public string ManagerId { get; }

    public long Sequence { get; }

    public DateOnly OccurredOn { get; }

    public ManagerCareerFactKind Kind { get; }

    public int ReputationChange { get; }

    public string SourceId { get; }

    public string Description { get; }
}

public sealed class ManagerCareer
{
    private ManagerCareer(
        string managerId,
        int initialReputation,
        int reputation,
        ImmutableArray<ManagerCareerFact> history)
    {
        ManagerId = managerId;
        InitialReputation = initialReputation;
        Reputation = reputation;
        History = history;
    }

    public string ManagerId { get; }

    public int InitialReputation { get; }

    public int Reputation { get; }

    public ImmutableArray<ManagerCareerFact> History { get; }

    public static ManagerCareer Project(
        string managerId,
        int initialReputation,
        IEnumerable<ManagerCareerFact> facts)
    {
        string normalizedManagerId = CareerValue.Required(managerId, nameof(managerId));
        if (initialReputation is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(initialReputation));
        }

        ArgumentNullException.ThrowIfNull(facts);
        ImmutableArray<ManagerCareerFact> history = facts
            .Select(fact => fact ?? throw new ArgumentException(
                "Career facts cannot contain null entries.",
                nameof(facts)))
            .OrderBy(fact => fact.Sequence)
            .ToImmutableArray();

        if (history.Any(fact => !string.Equals(
                fact.ManagerId,
                normalizedManagerId,
                StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "Every career fact must belong to the projected manager.",
                nameof(facts));
        }

        if (history.Select(fact => fact.Sequence).Distinct().Count() != history.Length)
        {
            throw new ArgumentException(
                "Career fact sequences must be unique.",
                nameof(facts));
        }

        int reputation = initialReputation;
        foreach (ManagerCareerFact fact in history)
        {
            reputation = Math.Clamp(reputation + fact.ReputationChange, 0, 100);
        }

        return new ManagerCareer(
            normalizedManagerId,
            initialReputation,
            reputation,
            history);
    }
}

internal static class CareerValue
{
    public static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
