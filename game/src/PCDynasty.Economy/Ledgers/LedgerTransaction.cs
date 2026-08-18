namespace PCDynasty.Economy.Ledgers;

public sealed class LedgerTransaction
{
    public LedgerTransaction(
        string transactionId,
        int occurredDay,
        string debitAccountId,
        string creditAccountId,
        long amount,
        string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(occurredDay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        TransactionId = LedgerValue.Required(transactionId, nameof(transactionId));
        OccurredDay = occurredDay;
        DebitAccountId = LedgerValue.Required(debitAccountId, nameof(debitAccountId));
        CreditAccountId = LedgerValue.Required(creditAccountId, nameof(creditAccountId));
        if (string.Equals(DebitAccountId, CreditAccountId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Debit and credit accounts must be distinct.");
        }

        Amount = amount;
        Currency = LedgerValue.Currency(currency);
    }

    public string TransactionId { get; }

    public int OccurredDay { get; }

    public string DebitAccountId { get; }

    public string CreditAccountId { get; }

    public long Amount { get; }

    public string Currency { get; }
}
