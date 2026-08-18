using PCDynasty.Contracts.Careers;

namespace PCDynasty.Application.Market;

public sealed record TransferOfferResult
{
    private TransferOfferResult(
        bool accepted,
        string reason,
        CareerTransferRecord? transfer,
        long remainingTransferBudget)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentOutOfRangeException.ThrowIfNegative(remainingTransferBudget);
        if (accepted != (transfer is not null))
        {
            throw new ArgumentException("Accepted offers require a transfer and rejected offers forbid one.");
        }

        Accepted = accepted;
        Reason = reason.Trim();
        Transfer = transfer;
        RemainingTransferBudget = remainingTransferBudget;
    }

    public bool Accepted { get; }
    public string Reason { get; }
    public CareerTransferRecord? Transfer { get; }
    public long RemainingTransferBudget { get; }

    public static TransferOfferResult Success(
        CareerTransferRecord transfer,
        long remainingTransferBudget) =>
        new(true, "accepted", transfer ?? throw new ArgumentNullException(nameof(transfer)), remainingTransferBudget);

    public static TransferOfferResult Rejected(string reason, long remainingTransferBudget = 0) =>
        new(false, reason, null, remainingTransferBudget);
}
