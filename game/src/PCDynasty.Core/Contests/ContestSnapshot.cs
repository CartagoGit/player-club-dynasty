using System.Collections.Immutable;

namespace PCDynasty.Core.Contests;

public sealed class ContestSnapshot
{
    public ContestSnapshot(
        string contestId,
        string rulesetId,
        long seed,
        IEnumerable<ContestParticipantSnapshot> participants)
    {
        ArgumentNullException.ThrowIfNull(participants);

        ImmutableArray<ContestParticipantSnapshot> copy = participants.ToImmutableArray();
        if (copy.IsEmpty)
        {
            throw new ArgumentException("A contest requires at least one participant.", nameof(participants));
        }

        if (copy.Select(participant => participant.ParticipantId)
            .Distinct(StringComparer.Ordinal)
            .Count() != copy.Length)
        {
            throw new ArgumentException("Contest participant identifiers must be unique.", nameof(participants));
        }

        ContestId = ContestValue.NormalizeIdentifier(contestId, nameof(contestId));
        RulesetId = ContestValue.NormalizeIdentifier(rulesetId, nameof(rulesetId));
        Seed = seed;
        Participants = copy;
    }

    public string ContestId { get; }

    public string RulesetId { get; }

    public long Seed { get; }

    public ImmutableArray<ContestParticipantSnapshot> Participants { get; }
}

public sealed class ContestParticipantSnapshot
{
    public ContestParticipantSnapshot(
        string participantId,
        IEnumerable<int> attributes,
        int form)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        ImmutableArray<int> attributeCopy = attributes.ToImmutableArray();
        if (attributeCopy.IsEmpty)
        {
            throw new ArgumentException(
                "A contest participant requires compiled attributes.",
                nameof(attributes));
        }

        for (int index = 0; index < attributeCopy.Length; index++)
        {
            ContestValue.EnsureScore(attributeCopy[index], $"{nameof(attributes)}[{index}]");
        }

        ParticipantId = ContestValue.NormalizeIdentifier(participantId, nameof(participantId));
        Attributes = attributeCopy;
        Form = ContestValue.EnsureScore(form, nameof(form));
    }

    public string ParticipantId { get; }

    public ImmutableArray<int> Attributes { get; }

    public int Form { get; }
}

internal static class ContestValue
{
    public static int EnsureScore(int score, string parameterName)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                score,
                "A compiled contest score must be between zero and one hundred.");
        }

        return score;
    }

    public static string NormalizeIdentifier(string identifier, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifier, parameterName);

        string normalized = identifier.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("An identifier is required.", parameterName);
        }

        return normalized;
    }
}
