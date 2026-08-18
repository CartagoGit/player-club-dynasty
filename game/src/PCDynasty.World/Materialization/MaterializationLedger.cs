using System.Collections.Immutable;
using PCDynasty.World.Commitments;

namespace PCDynasty.World.Materialization;

public sealed class MaterializationLedger
{
    public MaterializationLedger(
        IEnumerable<FidelityOwnership> ownership,
        IEnumerable<HistoricalCommitment> commitments,
        IEnumerable<FidelityPromotionBoundary> promotionBoundaries)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(commitments);
        ArgumentNullException.ThrowIfNull(promotionBoundaries);

        Ownership = CopyAndValidateOwnership(ownership);
        Commitments = CopyAndValidateCommitments(commitments);
        PromotionBoundaries = CopyAndValidateBoundaries(promotionBoundaries);
    }

    public ImmutableArray<FidelityOwnership> Ownership { get; }
    public ImmutableArray<HistoricalCommitment> Commitments { get; }
    public ImmutableArray<FidelityPromotionBoundary> PromotionBoundaries { get; }

    public MaterializationLedger RecordCommitments(
        IEnumerable<HistoricalCommitment> commitments)
    {
        ArgumentNullException.ThrowIfNull(commitments);
        var merged = Commitments.ToBuilder();
        foreach (HistoricalCommitment candidate in commitments)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            HistoricalCommitment? sameId = merged.FirstOrDefault(existing =>
                string.Equals(existing.CommitmentId, candidate.CommitmentId, StringComparison.Ordinal));
            if (sameId is not null)
            {
                if (!Equivalent(sameId, candidate))
                {
                    throw new InvalidOperationException(
                        $"Confirmed commitment '{candidate.CommitmentId}' cannot be rewritten.");
                }

                continue;
            }

            bool rewritesNaturalKey = merged.Any(existing =>
                string.Equals(existing.SubjectId, candidate.SubjectId, StringComparison.Ordinal) &&
                string.Equals(existing.OutcomeId, candidate.OutcomeId, StringComparison.Ordinal) &&
                existing.EffectiveAtTick == candidate.EffectiveAtTick);
            if (rewritesNaturalKey)
            {
                throw new InvalidOperationException(
                    "A confirmed subject outcome cannot be replaced by a new commitment identifier.");
            }

            merged.Add(candidate);
        }

        return new MaterializationLedger(Ownership, merged, PromotionBoundaries);
    }

    internal MaterializationLedger ApplyPromotion(FidelityPromotionBoundary boundary)
    {
        FidelityOwnership? current = Ownership.SingleOrDefault(candidate =>
            string.Equals(candidate.SubjectId, boundary.SubjectId, StringComparison.Ordinal) &&
            candidate.Fidelity == boundary.FromFidelity &&
            candidate.UntilTickExclusive is null &&
            candidate.Owns(boundary.AtTick));
        if (current is null || boundary.AtTick == current.FromTickInclusive)
        {
            throw new InvalidOperationException(
                "Promotion must split the subject's current open ownership interval.");
        }

        var promotedOwnership = Ownership
            .Where(candidate => !ReferenceEquals(candidate, current))
            .Append(new FidelityOwnership(
                current.SubjectId,
                current.Fidelity,
                current.FromTickInclusive,
                boundary.AtTick))
            .Append(new FidelityOwnership(
                current.SubjectId,
                boundary.ToFidelity,
                boundary.AtTick,
                null));
        return new MaterializationLedger(
            promotedOwnership,
            Commitments,
            PromotionBoundaries.Append(boundary));
    }

    private static ImmutableArray<FidelityOwnership> CopyAndValidateOwnership(
        IEnumerable<FidelityOwnership> ownership)
    {
        FidelityOwnership[] ordered = ownership.Select(item =>
        {
            ArgumentNullException.ThrowIfNull(item);
            return item;
        }).OrderBy(item => item.SubjectId, StringComparer.Ordinal)
          .ThenBy(item => item.FromTickInclusive)
          .ToArray();
        foreach (IGrouping<string, FidelityOwnership> group in ordered.GroupBy(
                     item => item.SubjectId,
                     StringComparer.Ordinal))
        {
            FidelityOwnership? previous = null;
            foreach (FidelityOwnership current in group)
            {
                if (previous?.UntilTickExclusive is null && previous is not null)
                {
                    throw new ArgumentException("An open ownership interval must be the final interval.", nameof(ownership));
                }

                if (previous?.UntilTickExclusive > current.FromTickInclusive)
                {
                    throw new ArgumentException("Ownership intervals cannot overlap.", nameof(ownership));
                }

                previous = current;
            }
        }

        return ordered.ToImmutableArray();
    }

    private static ImmutableArray<HistoricalCommitment> CopyAndValidateCommitments(
        IEnumerable<HistoricalCommitment> commitments)
    {
        var byId = new HashSet<string>(StringComparer.Ordinal);
        var naturalKeys = new HashSet<(string SubjectId, string OutcomeId, long EffectiveAtTick)>();
        HistoricalCommitment[] copied = commitments.Select(commitment =>
        {
            ArgumentNullException.ThrowIfNull(commitment);
            if (!byId.Add(commitment.CommitmentId) ||
                !naturalKeys.Add((commitment.SubjectId, commitment.OutcomeId, commitment.EffectiveAtTick)))
            {
                throw new ArgumentException("Commitments must have unique identities and subject outcomes.", nameof(commitments));
            }

            return commitment;
        }).OrderBy(commitment => commitment.CommittedAtTick)
          .ThenBy(commitment => commitment.CommitmentId, StringComparer.Ordinal)
          .ToArray();
        return copied.ToImmutableArray();
    }

    private static ImmutableArray<FidelityPromotionBoundary> CopyAndValidateBoundaries(
        IEnumerable<FidelityPromotionBoundary> boundaries)
    {
        var keys = new HashSet<(string SubjectId, long AtTick)>();
        FidelityPromotionBoundary[] copied = boundaries.Select(boundary =>
        {
            ArgumentNullException.ThrowIfNull(boundary);
            if (!keys.Add((boundary.SubjectId, boundary.AtTick)))
            {
                throw new ArgumentException("Promotion boundaries must be unique.", nameof(boundaries));
            }

            return boundary;
        }).OrderBy(boundary => boundary.AtTick)
          .ThenBy(boundary => boundary.SubjectId, StringComparer.Ordinal)
          .ToArray();
        return copied.ToImmutableArray();
    }

    private static bool Equivalent(
        HistoricalCommitment left,
        HistoricalCommitment right) =>
        string.Equals(left.CommitmentId, right.CommitmentId, StringComparison.Ordinal) &&
        string.Equals(left.SubjectId, right.SubjectId, StringComparison.Ordinal) &&
        string.Equals(left.OutcomeId, right.OutcomeId, StringComparison.Ordinal) &&
        left.CommittedAtTick == right.CommittedAtTick &&
        left.EffectiveAtTick == right.EffectiveAtTick &&
        left.OriginFidelity == right.OriginFidelity &&
        left.Values.Count == right.Values.Count &&
        left.Values.All(pair =>
            right.Values.TryGetValue(pair.Key, out string? value) &&
            string.Equals(pair.Value, value, StringComparison.Ordinal));
}
