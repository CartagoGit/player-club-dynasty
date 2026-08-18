namespace PCDynasty.Economy.Ledgers;

public sealed class LedgerAccount
{
    public LedgerAccount(
        string accountId,
        string ownerClubId,
        LedgerAccountKind kind)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        AccountId = LedgerValue.Required(accountId, nameof(accountId));
        OwnerClubId = LedgerValue.Required(ownerClubId, nameof(ownerClubId));
        Kind = kind;
    }

    public string AccountId { get; }

    public string OwnerClubId { get; }

    public LedgerAccountKind Kind { get; }
}
