using System.Collections.Immutable;
using PCDynasty.World.Commitments;
using PCDynasty.World.State;

namespace PCDynasty.World.Materialization;

public sealed class MaterializationResult
{
    public MaterializationResult(
        WorldCheckpoint sourceCheckpoint,
        MaterializationLedger ledger,
        IEnumerable<ClubSeasonState> sourceStates,
        IEnumerable<HistoricalCommitment> confirmedCommitments)
    {
        SourceCheckpoint = sourceCheckpoint ??
            throw new ArgumentNullException(nameof(sourceCheckpoint));
        Ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        ArgumentNullException.ThrowIfNull(sourceStates);
        ArgumentNullException.ThrowIfNull(confirmedCommitments);

        SourceStates = sourceStates.Select(state =>
        {
            ArgumentNullException.ThrowIfNull(state);
            return state;
        }).ToImmutableArray();
        ConfirmedCommitments = confirmedCommitments.Select(commitment =>
        {
            ArgumentNullException.ThrowIfNull(commitment);
            return commitment;
        }).ToImmutableArray();
    }

    public WorldCheckpoint SourceCheckpoint { get; }
    public MaterializationLedger Ledger { get; }
    public ImmutableArray<ClubSeasonState> SourceStates { get; }
    public ImmutableArray<HistoricalCommitment> ConfirmedCommitments { get; }
}
