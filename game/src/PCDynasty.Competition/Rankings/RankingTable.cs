using System.Collections.Immutable;

namespace PCDynasty.Competition.Rankings;

public enum RankingCriterionDirection
{
    Ascending,
    Descending,
}

public sealed class RankingCandidate
{
    public RankingCandidate(string participantId, IEnumerable<long> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ImmutableArray<long> copy = criteria.ToImmutableArray();
        if (copy.IsEmpty)
        {
            throw new ArgumentException("Ranking criteria cannot be empty.", nameof(criteria));
        }

        ParticipantId = CompetitionValue.NormalizeIdentifier(participantId, nameof(participantId));
        Criteria = copy;
    }

    public string ParticipantId { get; }
    public ImmutableArray<long> Criteria { get; }
}

public sealed class RankingEntry
{
    internal RankingEntry(int rank, RankingCandidate candidate)
    {
        Rank = rank;
        ParticipantId = candidate.ParticipantId;
        Criteria = candidate.Criteria;
    }

    public int Rank { get; }
    public string ParticipantId { get; }
    public ImmutableArray<long> Criteria { get; }
}

public sealed class RankingTable
{
    public RankingTable(
        IEnumerable<RankingCandidate> candidates,
        IEnumerable<RankingCriterionDirection> directions)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(directions);

        RankingCandidate[] candidateCopy = candidates.ToArray();
        ImmutableArray<RankingCriterionDirection> directionCopy = directions.ToImmutableArray();
        Validate(candidateCopy, directionCopy);

        Array.Sort(candidateCopy, (left, right) => Compare(left, right, directionCopy));
        Entries = candidateCopy
            .Select((candidate, index) => new RankingEntry(index + 1, candidate))
            .ToImmutableArray();
    }

    public ImmutableArray<RankingEntry> Entries { get; }

    private static void Validate(
        IReadOnlyCollection<RankingCandidate> candidates,
        ImmutableArray<RankingCriterionDirection> directions)
    {
        if (candidates.Count == 0 || directions.IsEmpty)
        {
            throw new ArgumentException("A ranking requires candidates and criteria directions.");
        }

        var participantIds = new HashSet<string>(StringComparer.Ordinal);
        var criteriaSets = new HashSet<ImmutableArray<long>>(CriteriaComparer.Instance);
        foreach (RankingCandidate candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            if (candidate.Criteria.Length != directions.Length)
            {
                throw new ArgumentException("Every candidate must provide every ranking criterion.", nameof(candidates));
            }

            if (!participantIds.Add(candidate.ParticipantId))
            {
                throw new ArgumentException("Ranking participant identifiers must be unique.", nameof(candidates));
            }

            if (!criteriaSets.Add(candidate.Criteria))
            {
                throw new ArgumentException(
                    "Configured criteria must resolve every tie before creating the ranking.",
                    nameof(candidates));
            }
        }
    }

    private static int Compare(
        RankingCandidate left,
        RankingCandidate right,
        ImmutableArray<RankingCriterionDirection> directions)
    {
        for (int index = 0; index < directions.Length; index++)
        {
            int comparison = left.Criteria[index].CompareTo(right.Criteria[index]);
            if (comparison == 0)
            {
                continue;
            }

            return directions[index] == RankingCriterionDirection.Ascending
                ? comparison
                : -comparison;
        }

        return 0;
    }

    private sealed class CriteriaComparer : IEqualityComparer<ImmutableArray<long>>
    {
        public static CriteriaComparer Instance { get; } = new();

        public bool Equals(ImmutableArray<long> left, ImmutableArray<long> right)
        {
            return left.AsSpan().SequenceEqual(right.AsSpan());
        }

        public int GetHashCode(ImmutableArray<long> values)
        {
            var hash = new HashCode();
            foreach (long value in values)
            {
                hash.Add(value);
            }

            return hash.ToHashCode();
        }
    }
}

internal static class CompetitionValue
{
    public static string NormalizeIdentifier(string identifier, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifier, parameterName);
        string normalized = identifier.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("An identifier is required.", parameterName);
    }
}
