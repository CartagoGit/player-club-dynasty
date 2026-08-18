using System.Collections.Immutable;

namespace PCDynasty.Economy.Ledgers;

public sealed class ClubLedger
{
    private readonly ImmutableDictionary<string, LedgerAccount> _accountsById;

    public ClubLedger(
        string clubId,
        string currency,
        IEnumerable<LedgerAccount> accounts,
        IEnumerable<LedgerTransaction> transactions)
    {
        ClubId = LedgerValue.Required(clubId, nameof(clubId));
        Currency = LedgerValue.Currency(currency);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(transactions);

        var accountsById = ImmutableDictionary.CreateBuilder<string, LedgerAccount>(
            StringComparer.Ordinal);
        foreach (LedgerAccount account in accounts)
        {
            ArgumentNullException.ThrowIfNull(account);
            if (!string.Equals(account.OwnerClubId, ClubId, StringComparison.Ordinal))
            {
                throw new ArgumentException("Every ledger account must belong to its club.", nameof(accounts));
            }

            if (!accountsById.TryAdd(account.AccountId, account))
            {
                throw new ArgumentException("Account identifiers must be unique.", nameof(accounts));
            }
        }

        _accountsById = accountsById.ToImmutable();
        Accounts = _accountsById.Values
            .OrderBy(account => account.AccountId, StringComparer.Ordinal)
            .ToImmutableArray();
        var copiedTransactions = ImmutableArray.CreateBuilder<LedgerTransaction>();
        var transactionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (LedgerTransaction transaction in transactions)
        {
            ValidateTransaction(transaction, transactionIds);
            copiedTransactions.Add(transaction);
        }

        Transactions = copiedTransactions.ToImmutable();
    }

    private ClubLedger(ClubLedger source, LedgerTransaction transaction)
    {
        ClubId = source.ClubId;
        Currency = source.Currency;
        Accounts = source.Accounts;
        _accountsById = source._accountsById;
        Transactions = source.Transactions.Add(transaction);
    }

    public string ClubId { get; }
    public string Currency { get; }
    public ImmutableArray<LedgerAccount> Accounts { get; }
    public ImmutableArray<LedgerTransaction> Transactions { get; }

    public ClubLedger Post(LedgerTransaction transaction)
    {
        var existingIds = Transactions
            .Select(item => item.TransactionId)
            .ToHashSet(StringComparer.Ordinal);
        ValidateTransaction(transaction, existingIds);
        return new ClubLedger(this, transaction);
    }

    public long Balance(string accountId)
    {
        string normalizedId = LedgerValue.Required(accountId, nameof(accountId));
        if (!_accountsById.ContainsKey(normalizedId))
        {
            throw new KeyNotFoundException($"Unknown account '{normalizedId}'.");
        }

        return CalculateBalances()[normalizedId];
    }

    public AnnualClubFinancialState Snapshot(int year)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(year);
        ImmutableDictionary<string, long> balances = CalculateBalances();
        long Aggregate(LedgerAccountKind kind) => Accounts
            .Where(account => account.Kind == kind)
            .Sum(account => balances[account.AccountId]);

        return new AnnualClubFinancialState(
            ClubId,
            year,
            Currency,
            Aggregate(LedgerAccountKind.Cash),
            -Aggregate(LedgerAccountKind.Revenue),
            Aggregate(LedgerAccountKind.Cost),
            -Aggregate(LedgerAccountKind.Debt),
            Aggregate(LedgerAccountKind.Budget));
    }

    private void ValidateTransaction(
        LedgerTransaction transaction,
        HashSet<string> transactionIds)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (!transactionIds.Add(transaction.TransactionId))
        {
            throw new ArgumentException("Transaction identifiers must be unique.", nameof(transaction));
        }

        if (!string.Equals(transaction.Currency, Currency, StringComparison.Ordinal))
        {
            throw new ArgumentException("Transaction currency must match its ledger.", nameof(transaction));
        }

        if (!_accountsById.ContainsKey(transaction.DebitAccountId) ||
            !_accountsById.ContainsKey(transaction.CreditAccountId))
        {
            throw new ArgumentException("Both posting accounts must belong to the ledger.", nameof(transaction));
        }
    }

    private ImmutableDictionary<string, long> CalculateBalances()
    {
        var balances = Accounts.ToDictionary(
            account => account.AccountId,
            _ => 0L,
            StringComparer.Ordinal);
        foreach (LedgerTransaction transaction in Transactions)
        {
            balances[transaction.DebitAccountId] = checked(
                balances[transaction.DebitAccountId] + transaction.Amount);
            balances[transaction.CreditAccountId] = checked(
                balances[transaction.CreditAccountId] - transaction.Amount);
        }

        return balances.ToImmutableDictionary(StringComparer.Ordinal);
    }
}
